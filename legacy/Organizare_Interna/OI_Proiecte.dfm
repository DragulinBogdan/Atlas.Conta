object frmOIProiecte: TfrmOIProiecte
  Left = 302
  Top = 96
  Caption = 'Intretinere Proiecte'
  ClientHeight = 596
  ClientWidth = 1084
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnContent: TcxGroupBox
    Left = 0
    Top = 0
    Align = alClient
    PanelStyle.Active = True
    TabOrder = 0
    Height = 596
    Width = 1084
    object cxGroupBox: TcxGroupBox
      Left = 793
      Top = 2
      Align = alRight
      ParentBackground = False
      ParentColor = False
      Style.BorderColor = clMenuHighlight
      Style.BorderStyle = ebsThick
      Style.Color = clWindow
      Style.Shadow = False
      TabOrder = 0
      Height = 497
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
      object Label8: TLabel
        Left = 12
        Top = 235
        Width = 63
        Height = 13
        Hint = 'Tipul de produs asociat'
        Caption = 'Tip Proiect'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 8
        Top = 417
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
        Top = 301
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
        Top = 275
        Hint = 
          'Starea proiectului  : activa(casuta bifata) sau incactiva(casuta' +
          ' nebifata)'
        Caption = 'Este Activ(X) sau Inactiv ()'
        DataBinding.DataField = 'STARE'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
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
      object edtDescriere: TcxDBMemo
        Left = 22
        Top = 160
        Hint = 'Descrierea tipului de material'
        DataBinding.DataField = 'DESCRIERE'
        DataBinding.DataSource = DTOIProiecte
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        Height = 67
        Width = 248
      end
      object edtTipProiect: TcxPopupEdit
        Left = 22
        Top = 249
        Hint = 'Tipul de Proiect Asociat'
        Properties.PopupAutoSize = False
        Properties.PopupControl = pnTipProiect
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseQuery = edtTipProiectPropertiesCloseQuery
        Properties.OnInitPopup = edtTipProiectPropertiesInitPopup
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 3
        Width = 248
      end
      object edtAreContabilitateProprie: TcxDBCheckBox
        Left = 12
        Top = 341
        Caption = 'Se tine contabilitate separata (x) Da ( ) Nu'
        DataBinding.DataField = 'ARE_CONTABILITATE'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
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
        TabOrder = 5
      end
      object cxDBTextEdit1: TcxDBTextEdit
        Left = 22
        Top = 76
        Hint = 'Denumirea Proiectului'
        DataBinding.DataField = 'cod_proiect'
        DataBinding.DataSource = DTOIProiecte
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 6
        Width = 250
      end
      object cxDBCheckBox1: TcxDBCheckBox
        Left = 12
        Top = 367
        Caption = 'Credit Angajament (x) Da ( ) Nu'
        DataBinding.DataField = 'ESTE_CREDIT_ANGAJAMENT'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
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
        TabOrder = 7
      end
      object edDataProiect: TcxDBDateEdit
        Left = 22
        Top = 316
        DataBinding.DataField = 'data_proiect'
        DataBinding.DataSource = DTOIProiecte
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 8
        Width = 176
      end
      object chkProcentual: TcxDBCheckBox
        Left = 12
        Top = 391
        Caption = 'Defalcare procentuala (x) Da ( ) Nu'
        DataBinding.DataField = 'ESTE_PROCENTUAL'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
        Properties.ValueGrayed = 'False'
        Properties.OnEditValueChanged = chkProcentualPropertiesEditValueChanged
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
        TabOrder = 9
      end
    end
    object Panel1: TcxGroupBox
      Left = 2
      Top = 2
      Align = alClient
      PanelStyle.Active = True
      TabOrder = 1
      DesignSize = (
        791
        497)
      Height = 497
      Width = 791
      object TreeProiecte: TcxDBTreeList
        Left = 2
        Top = 42
        Width = 787
        Height = 196
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
        TabOrder = 3
        OnCustomDrawDataCell = TreeProiecteCustomDrawDataCell
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
      object pnControl: TcxGroupBox
        Left = 2
        Top = 238
        Align = alBottom
        PanelStyle.Active = True
        TabOrder = 0
        DesignSize = (
          787
          257)
        Height = 257
        Width = 787
        object btnAddProiect: TcxButton
          Left = 2
          Top = 1
          Width = 84
          Height = 29
          Hint = 'Adaugare de proiect'
          Caption = 'Proiect'
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360900000000000036000000280000001800000018000000010020000000
            000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FEFEFEFFA8CFACFF58A561FF2A953DFF2A95
            3DFF54A35DFFA3CDA7FFFBFDFBFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00F5F9F5FF62AA69FF249C42FF24BB60FF16BD5DFF1CBC
            60FF2DBC66FF2A9E48FF5AA562FFF0F7F1FFFFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF0072B478FF3AA659FF46CD82FF2BC970FFA7D2BAFFAAC4
            B6FF34C373FF51CE89FF43AA60FF66AB6DFFFEFEFEFFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FEFE
            FEFFE5E5E5FFA7B7A7FF319943FF66CD93FF45CB7FFF30C670FFD4EDDFFFD4DB
            D8FF37BF71FF4BCC83FF6BCF97FF329746FFC2DEC5FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00F9F9F9FFDBDBDBFFB4B4B4FF9D9D9DFF9A9A
            9AFFC2BCC1FF74A978FF45A961FF7BC89EFF6EBC8FFF64BA89FFDBEAE1FFD9DF
            DCFF68B689FF70BB91FF78C29AFF53B372FF78B67FFFFFFFFF00FFFFFF00FFFF
            FF00E7E7E7FFB3B3B3FF979797FF8E8E8EFF9A9A9AFFA8A8A8FFBBBBBBFFCBCB
            CBFFE1DDE1FF68A86EFF3EAA60FFB6DBC8FFFCF2F9FFF7F0F4FFFDFCFDFFFDFC
            FCFFF7F1F5FFF9EFF5FFADC7B9FF54B87AFF64AC6BFFDBDBDBFFA3A3A3FF9C9C
            9CFFA2A2A2FFB1B1B1FFB8B8B8FFB2B2B2FFACACACFFABABABFFB3B3B3FFC0C0
            C0FFDCD8DCFF69A76FFF37AA5CFF95D4B2FFC7E5D3FFCBE4D6FFF3F7F5FFF5F7
            F6FFCFE4D7FFCAE5D6FF9BD0B3FF51B978FF66AD6EFFAEAEAEFFD1D1D1FFD2D2
            D2FFC4C4C4FFB5B5B5FFAAAAAAFFA7A7A7FFA9A9A9FFA9A9A9FFB2B2B2FFC0C0
            C0FFDED8DEFF89B58BFF2FA14EFF5DCD8FFF63C58EFF7AC49AFFE0EBE5FFD8DC
            D9FF7FBF9AFF71C796FF68CE97FF42AD64FF8CC190FFAEAEAEFFCACACAFFC6C6
            C6FFBABABAFFA2A2A2FF949494FF9E9E9EFFA4A4A4FFAAAAAAFFB2B2B2FFC0C0
            C0FFD7D5D7FFD0D9CFFF30923FFF50C07CFF77CE9FFF93C9ABFFE7ECE9FFDEDE
            DEFF92C4A8FF83CFA5FF65C98FFF27903AFFACBEADFFADADADFFCACACAFFC7C7
            C7FFB0B0B0FFC7C7C7FF9C9C9CFFBBBBBBFFA9A9A9FFA8A8A8FFB2B2B2FFC0C0
            C0FFD2D2D2FFF0EBEFFFA1C7A3FF208F35FF60C086FF9CD1B5FFC4D5CDFFC3D4
            CCFF9ACFB4FF6EC894FF27983FFF6A9C6DFFA19DA1FFACACACFFCBCBCBFFC6C6
            C6FFB9B9B9FF9D9D9DFF818181FF9D9D9DFFA2A2A2FFA5A5A5FFB2B2B2FFC0C0
            C0FFD2D2D2FFE6E6E6FFFDF9FDFFA7CEAAFF2E933EFF35A251FF58B375FF58B3
            76FF39A556FF258F37FF6EA072FFBFBCBFFF9E9E9EFFABABABFFCCCCCCFFC6C6
            C6FFB8B8B8FF989898FFA1A1A1FF878787FF9C9C9CFFA6A6A6FFB2B2B2FFC1C1
            C1FFD4D4D4FFE8E8E8FFF6F5F6FFFFFFFF00CAD7CBFF5B945EFF43974DFF4A9E
            54FF67A66CFFAEC1AFFFCCC7CCFFBEBCBEFF9C9C9CFFAAAAAAFFCECECEFFC8C8
            C8FFB8B8B8FFC0C0C0FFB5B5B5FFB2B2B2FFBBBBBBFFA2A2A2FFB0B0B0FFB8B9
            B9FFC7C7C7FFD2D2D2FFD5D5D5FFCBCBCBFFC5C2C4FFD3CDD2FFBCBBBDFFD9D8
            D8FFE8E3E8FFDBD7DAFFC5C4C4FFBDBDBDFF9B9B9BFFAEAEAEFFCAC9C9FFBEBE
            BEFFA9A9A9FFCACACAFFE1E1E1FFE7E7E7FFBEBEBEFFA2A3A3FFB1B1B1FFB6B6
            B6FFB1B1B1FFA8A8A8FFA3A3A3FF989898FFA3A3A3FFCDCCCDFF9A999AFFACAC
            ADFFBFBFC0FFC8C8C8FFBFBFBFFFBDBDBDFF9D9D9DFF959596FF838384FF8081
            81FF7C7C7CFF8A8A8AFFA1A1A1FF969696FF878787FF7B7B7BFF858585FF8787
            87FF8B8B8BFF939393FF969696FF919191FF9D9D9DFFB9B9B9FF848484FF8181
            81FF878787FF939393FF9F9F9FFF9F9F9FFF9B9C9CFF7F7F7FFF717171FF7777
            77FF7E7E7EFF797979FF818181FF898989FF919191FFA2A2A2FFB9B9B9FFC0C0
            C0FFC8C8C8FFCECECEFFCCCCCCFFBDBDBEFFBEBEBEFFDDDDDDFFAFB0B0FFABAB
            ABFFB4B4B4FFB2B2B2FF9E9E9EFF8A8A8AFF8A8A8AFFA1A1A2FF929292FF9797
            97FF9C9C9CFF9C9C9CFF9D9D9DFFA1A1A2FFA5A5A5FFAAABABFFB0B0B0FFB8B8
            B9FFC3C3C3FFCFD0D0FFDADADAFFE1E1E1FFD0D0D0FFB8B7B6FFBDBDBEFFCACB
            CBFFC8C8C8FFC5C5C6FFBEBEBEFFAEAFAFFF9FA0A0FFB5B5B6FFCDCDCDFFCDCD
            CDFFC4C4C4FFBDBDBDFFB4B4B4FFADADADFFADADADFFB0B1B1FFBBBBBBFFCBCB
            CBFFE0E0E0FFF6F5F5FFFFFFFF00FFFFFF00F4F4F3FFC9C8C5FFE1E1E2FFFDFD
            FDFFF1F1F1FFDCDCDCFFC9CACAFFC1C1C0FFA3A4A4FFBFBFBFFFA1A1A1FF9F9F
            9FFF9A9A9AFF949494FF888888FF808080FF868686FF909090FF999999FFA1A1
            A1FFA5A5A5FFAAAAAAFFA0A0A0FFA0A1A1FF8F9090FF929191FF9F9F9FFFABAB
            ABFFA7A7A7FFA1A1A1FF9A9A9AFF989898FFA7A7A7FFFEFEFEFFF9F9F9FFFAFA
            FAFFFBFBFBFFFBFBFBFFAAAAAAFF4C4E4FFF8E8F90FFE9E9E9FFC0C0C0FFB6B6
            B6FFCFCFCFFFECECECFF555758FF474A4CFF3D4041FFC7C7C7FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FEFEFEFFFDFDFDFFFEFEFEFFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00C7C7C7FF888A8CFF717374FF696A6BFF606162FF6769
            6AFF6E6F70FF838384FF989999FFBABCBCFFA7A7A7FFDADADAFFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00F4F4F4FFB5B5B5FFC0C0C0FFD4D4D4FFE4E4E4FFE8E8
            E8FFE5E6E6FFE4E4E4FFEBEBEAFFE0E0E0FFBBBBBBFFF4F4F4FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DCDCDCFFC3C3C3FFB8B8B8FFAFAF
            AFFFA9A9A9FFAAAAAAFFB4B4B4FFCBCBCBFFF8F8F8FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
          TabOrder = 0
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
          OnClick = btnAddProiectClick
        end
        object btnDelTipMat: TcxButton
          Left = 184
          Top = 1
          Width = 119
          Height = 29
          Hint = 'Stergere proiect'
          Caption = 'Sterge Proiect'
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360900000000000036000000280000001800000018000000010020000000
            000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00F8F8FDFF9A9ADDFF4D4DC6FF2727C2FF2A2A
            C2FF5858C9FFAEAEE4FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00E9E9F9FF4E4EC5FF2525D7FF1E1EF7FF0A0AFEFF1616
            FDFF2F2FF2FF2828CFFF6A6ACEFFF8F8FDFFFFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF005858CCFF4848DFFF5959EDFF3131E4FF0F0FF7FF1918
            F8FF4444E5FF6868E8FF3E3ECFFF8181D5FFFFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F2F2F2FFCFCF
            CFFFB1B1AEFF8383A2FF3333C9FF7474FEFFBABAE4FFE4E4DEFF7979D2FF8585
            DDFFE5E5DFFFB6B5D3FF7777F4FF3131C2FFE2E2F5FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00EDEDEDFFC1C1C1FFA3A3A3FF929292FF969696FFB6B6
            B5FFDFDFD7FF6F6FCEFF5D5CE5FF7675FFFF7C7CF3FFFCFCFEFFECECEBFFF3F3
            F1FFFBFBF9FF8787ECFF7F80FFFF5151DBFF9494DBFFFFFFFF00F5F5F5FFC9C9
            C9FFA0A0A0FF969696FF969696FFA4A4A4FFADADADFFB4B4B4FFB9B9B9FFC4C4
            C3FFD5D5D3FF5050C0FF6767EFFF7676FFFF5858FBFFB9B9EFFFFFFFFF00FFFF
            FF00B7B7E0FF6767FAFF7E7EFFFF5F5FE8FF7474D1FFC2C2C2FFA2A2A2FFB8B8
            B8FFC2C2C2FFC2C2C2FFB6B6B6FFACACACFFA9A9A9FFA9A9A9FFB2B2B2FFC1C1
            C0FFD4D4D2FF5050C0FF5A5AEFFF7070FFFF6767F7FFCAC9EBFFFFFFFF00FFFF
            FF00BEBED6FF7272F2FF7979FFFF5959E8FF7575D2FFAEAEAEFFD2D2D2FFC9C9
            C9FFBFBFBFFFABABABFF9F9F9FFFA1A1A1FFA8A8A8FFA9A9A9FFB2B2B2FFC1C1
            C0FFDDDDD5FF6969C4FF4141E3FF6B6BFFFFA1A1EBFFFFFFFAFFEFEFF4FFEEEE
            F4FFFCFCF2FFA09FDBFF7878FFFF4444DCFF9696D8FFAEAEAEFFCACACAFFC6C6
            C6FFB4B4B4FFACACACFF979797FFA8A8A8FFA2A2A2FFAAAAAAFFB2B2B2FFC0C0
            C0FFDBDBD5FFADADD3FF2525C7FF6F6FFDFFB3B3E9FFD5D5ECFFCECEEEFFCACA
            EEFFD4D4EDFFB5B5E3FF7474FAFF2828C2FF9F9FADFFADADADFFCACACAFFC7C7
            C7FFB1B1B1FFC5C5C5FF979797FFBABABAFFAAAAAAFFA7A7A7FFB2B2B2FFC0C0
            C0FFD3D3D2FFEAEAE6FF6060C6FF3333D6FF8D8DFDFFBFBFFCFFE1E1FFFFD8D8
            FFFFB5B5FDFF8B8BFDFF3232D4FF5C5CB4FFA1A19CFFACACACFFCCCCCCFFC6C6
            C6FFBABABAFF959595FF7F7F7FFF919191FF9C9C9CFFA6A6A6FFB2B2B2FFC0C0
            C0FFD2D2D2FFEAEAE7FFEDEDF1FF5E5ECAFF2A2BCDFF7272EEFFA2A2FCFF9A9A
            FAFF6665EBFF2525C9FF5454B4FFBBBBBAFF9F9F9DFFABABABFFCCCCCCFFC6C6
            C6FFB7B7B7FFA1A1A1FFB1B1B1FF8C8C8CFFA2A2A2FFA6A6A6FFB3B3B3FFC1C1
            C1FFD2D2D2FFE1E1E2FFF6F6F2FFF6F7F7FF7F7FC3FF2829ADFF1D1DB9FF2525
            C0FF3D3DBEFF8686C2FFC9C9C4FFBFBFBCFF9C9C9CFFAAAAAAFFD0D0D0FFC9C9
            C9FFB8B8B8FFC9C9C9FFBBBBBBFFC6C6C6FFC1C1C1FF9E9E9EFFAFAFAFFFBBBC
            BCFFC7C7C7FFCFCFCFFFCDCDCEFFBDBEBDFFC6C6BEFFD2D3D5FFA3A3B1FFBABA
            C7FFD6D6D7FFDBDBD3FFC5C5C4FFBEBEBEFF9B9B9BFFAEAEAEFFBBBBBBFFB1B2
            B2FFA2A2A2FFC0C0C0FFE2E2E2FFDDDDDDFFB5B5B5FFA1A1A1FFA6A6A6FF9F9F
            9FFF9B9B9BFF9B9B9AFF959595FF888888FF929291FFBBBBB9FF8F8F8CFF9E9E
            9BFFB7B8B6FFC3C3C3FFBEBEBEFFB9BABAFF9E9E9FFF8C8D8DFF7B7B7BFF7878
            78FF727272FF7F7F7FFF8E8E8EFF858585FF7D7D7DFF727272FF808080FF8686
            86FF8C8C8CFF969696FF9F9F9FFFA2A2A2FFB2B2B2FFC9C9C9FF848484FF7D7D
            7DFF818181FF8B8B8BFF989898FF9A9A9AFF999A9AFF838384FF757575FF7D7D
            7DFF868686FF7E7E7EFF858585FF8F8F8FFF969696FFA8A8A8FFBFBFBFFFC7C7
            C7FFCCCCCCFFCFD0D0FFCCCDCDFFBEBEBEFFBBBBBBFFD7D7D7FFB3B3B3FFB3B3
            B3FFB8B8B8FFB9B9B9FFA6A6A6FF8E8E8EFF898989FFA2A3A4FF989898FF9C9D
            9DFFA0A0A0FFA0A0A0FFA0A1A1FFA3A3A3FFA6A6A6FFA9A9A9FFAEAEAEFFB7B7
            B7FFC3C4C4FFD2D2D2FFDEDFDFFFE9E9E9FFD8D8D7FFB8B7B5FFC0C0C1FFD0D0
            D0FFCBCBCBFFC5C5C5FFBCBCBDFFAFAFAFFFA2A2A2FFB4B4B4FFD3D3D3FFD2D2
            D2FFC9C9C9FFC1C1C1FFB7B7B7FFB0B0B0FFB0B0B0FFB4B4B4FFBEBEBEFFCDCC
            CCFFE0E0E0FFF5F5F5FFFFFFFF00FFFFFF00F0F0EFFFC6C5C3FFE0E0E1FFFCFC
            FCFFF3F3F3FFE2E2E2FFCECECEFFC4C4C4FFA0A0A0FFC7C7C7FF9E9E9EFF9D9D
            9DFF999999FF969696FF858585FF767777FF828283FF999999FFA2A2A2FFA8A8
            A8FFAAAAAAFFABABABFF8E8F8FFF8B8C8DFF7C7D7EFF929291FFA1A1A1FFA8A8
            A8FFA5A5A5FFA1A1A1FF9B9B9BFF999999FFB0B0B0FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00AEAEAEFF4A4D4EFF8E8F8FFFDDDDDEFFB0B0B0FFA6A7
            A6FFBFBFBFFFE2E2E2FF58595AFF4D5152FF444647FFCDCDCDFFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00C7C7C7FF8B8D8EFF6B6C6DFF5E5F60FF606162FF6869
            6AFF6B6D6DFF7B7B7CFF9A9B9BFFBFC0C0FFABABABFFD9D9D9FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00F4F4F4FFB5B5B5FFC0C1C1FFD5D5D5FFE4E4E4FFE8E8
            E8FFE5E6E6FFE4E4E4FFEAEAEAFFDFDFDFFFBABABAFFF4F4F4FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DCDCDCFFC3C3C3FFB8B8B8FFAFAF
            AFFFA9A9A9FFAAAAAAFFB4B4B4FFCBCBCBFFF8F8F8FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
          TabOrder = 1
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
          OnClick = btnDelTipMatClick
        end
        object cxPageControl: TcxPageControl
          Left = 2
          Top = 31
          Width = 783
          Height = 224
          Align = alBottom
          TabOrder = 2
          Properties.ActivePage = tabContabilitate
          Properties.CustomButtons.Buttons = <>
          Properties.Style = 9
          Properties.TabSlants.Kind = skCutCorner
          Properties.TabSlants.Positions = [spLeft, spRight]
          LookAndFeel.Kind = lfOffice11
          ClientRectBottom = 224
          ClientRectRight = 783
          ClientRectTop = 20
          object tabImplicit: TcxTabSheet
            Caption = 'Informatii Proiect'
            ImageIndex = 2
            DesignSize = (
              783
              204)
            object Label5: TLabel
              Left = 564
              Top = 1
              Width = 86
              Height = 13
              Hint = 'Codul Functional'
              Anchors = [akTop, akRight]
              Caption = 'Cod Functional'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              ExplicitLeft = 725
            end
            object Label7: TLabel
              Left = 564
              Top = 43
              Width = 82
              Height = 13
              Hint = 'Codul Economic'
              Anchors = [akTop, akRight]
              Caption = 'Cod Economic'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              ExplicitLeft = 725
            end
            object btnCFAdd: TcxButton
              Left = 582
              Top = 94
              Width = 59
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Adauga'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 0
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = btnCFAddClick
            end
            object btnCFDel: TcxButton
              Left = 646
              Top = 94
              Width = 60
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Sterge'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 1
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = btnCFDelClick
            end
            object btnCFUpd: TcxButton
              Left = 710
              Top = 94
              Width = 63
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Modifica'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 2
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = btnCFUpdClick
            end
            object edCE: TcxDBButtonEdit
              Left = 582
              Top = 56
              Hint = 'Codul Functional asociat proiectului.'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'COD_ECONOMIC'
              DataBinding.DataSource = DTCF
              Properties.Buttons = <
                item
                  Default = True
                  Kind = bkEllipsis
                end>
              Properties.OnButtonClick = edCEPropertiesButtonClick
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 3
              Width = 192
            end
            object edCF: TcxDBButtonEdit
              Left = 582
              Top = 17
              Hint = 'Codul Functional asociat proiectului.'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'COD_FUNCTIONAL'
              DataBinding.DataSource = DTCF
              Properties.Buttons = <
                item
                  Default = True
                  Kind = bkEllipsis
                end>
              Properties.OnButtonClick = edCFPropertiesButtonClick
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 4
              Width = 192
            end
            object GridCF: TcxGrid
              Left = 0
              Top = 0
              Width = 560
              Height = 204
              Align = alLeft
              Anchors = [akLeft, akTop, akRight, akBottom]
              TabOrder = 5
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
                  Options.Editing = False
                  Width = 77
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
                  Properties.ListSource = frmData.DTOIUnitati
                  Options.Editing = False
                  Width = 68
                end
                object GridCFVCOD_ECONOMIC: TcxGridDBColumn
                  Caption = 'Cod economic'
                  DataBinding.FieldName = 'COD_ECONOMIC'
                  Options.Editing = False
                  Width = 75
                end
                object GridCFVID_OI_PROIECTE: TcxGridDBColumn
                  DataBinding.FieldName = 'ID_OI_PROIECTE'
                  Visible = False
                end
                object GridCFVIBAN: TcxGridDBColumn
                  DataBinding.FieldName = 'IBAN'
                  HeaderAlignmentHorz = taCenter
                  Width = 110
                end
                object GridCFVcodAngajament: TcxGridDBColumn
                  Caption = 'Cod Angajament'
                  DataBinding.FieldName = 'codAngajament'
                  HeaderAlignmentHorz = taCenter
                end
                object GridCFVcodProgram: TcxGridDBColumn
                  Caption = 'Cod Program'
                  DataBinding.FieldName = 'codProgram'
                  HeaderAlignmentHorz = taCenter
                end
                object GridCFVcodIndicator: TcxGridDBColumn
                  Caption = 'Cod Indicator'
                  DataBinding.FieldName = 'codIndicator'
                  HeaderAlignmentHorz = taCenter
                  Width = 47
                end
                object GridCFVPROCENT: TcxGridDBColumn
                  Caption = 'Procent'
                  DataBinding.FieldName = 'procent'
                  PropertiesClassName = 'TcxSpinEditProperties'
                  Properties.AssignedValues.MinValue = True
                  Properties.ImmediatePost = True
                  Properties.MaxValue = 100.000000000000000000
                  Properties.ValueType = vtFloat
                  GroupSummaryAlignment = taRightJustify
                  HeaderAlignmentHorz = taCenter
                end
              end
              object GridCFL: TcxGridLevel
                GridView = GridCFV
              end
            end
            object btnPlanificare: TcxButton
              Left = 599
              Top = 126
              Width = 169
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Planificare'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 6
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = btnPlanificareClick
            end
          end
          object tabBuget: TcxTabSheet
            Caption = 'Situatie Bugetara'
            ImageIndex = 0
            object GridBuget: TcxGrid
              Left = 0
              Top = 0
              Width = 924
              Height = 204
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
            DesignSize = (
              783
              204)
            object Label6: TLabel
              Left = 582
              Top = 3
              Width = 71
              Height = 13
              Hint = 'Cont'
              Anchors = [akTop, akRight]
              Caption = 'Cont Proiect'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              ExplicitLeft = 743
            end
            object Label9: TLabel
              Left = 584
              Top = 128
              Width = 56
              Height = 13
              Anchors = [akTop, akRight]
              Caption = 'Sold debitor'
              ExplicitLeft = 745
            end
            object Label10: TLabel
              Left = 584
              Top = 154
              Width = 59
              Height = 13
              Anchors = [akTop, akRight]
              Caption = 'Sold creditor'
              ExplicitLeft = 745
            end
            object Label12: TLabel
              Left = 582
              Top = 85
              Width = 82
              Height = 13
              Hint = 'Codul Economic'
              Anchors = [akTop, akRight]
              Caption = 'Cod Economic'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              ExplicitLeft = 743
            end
            object Label14: TLabel
              Left = 582
              Top = 43
              Width = 86
              Height = 13
              Hint = 'Codul Functional'
              Anchors = [akTop, akRight]
              Caption = 'Cod Functional'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              ExplicitLeft = 743
            end
            object GridConta: TcxGrid
              Left = 0
              Top = 0
              Width = 568
              Height = 204
              Align = alLeft
              Anchors = [akLeft, akTop, akRight, akBottom]
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
                object GridContaVCOD_FUNCTIONAL: TcxGridDBColumn
                  Caption = 'Cod Functional'
                  DataBinding.FieldName = 'COD_FUNCTIONAL'
                  HeaderAlignmentHorz = taCenter
                  Width = 128
                end
                object GridContaVCOD_ECONOMIC: TcxGridDBColumn
                  Caption = 'Cod Economic'
                  DataBinding.FieldName = 'COD_ECONOMIC'
                  HeaderAlignmentHorz = taCenter
                  Width = 128
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
            object edtCont: TcxDBButtonEdit
              Left = 582
              Top = 20
              Hint = 'Contul proiectului-investitiei'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'CONT'
              DataBinding.DataSource = DTSolduri
              Properties.Buttons = <
                item
                  Default = True
                  Kind = bkEllipsis
                end>
              Properties.OnButtonClick = edtContPropertiesButtonClick
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 1
              Width = 193
            end
            object edtSoldDebit: TcxDBCurrencyEdit
              Left = 652
              Top = 125
              Hint = 'Soldul proiectului'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'SOLD_DEBITOR'
              DataBinding.DataSource = DTSolduri
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 2
              OnEnter = edtSoldDebitEnter
              Width = 123
            end
            object edtSoldCredit: TcxDBCurrencyEdit
              Left = 652
              Top = 151
              Hint = 'Soldul proiectului'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'SOLD_CREDITOR'
              DataBinding.DataSource = DTSolduri
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 3
              OnEnter = edtSoldDebitEnter
              Width = 123
            end
            object cxButton1: TcxButton
              Left = 582
              Top = 178
              Width = 59
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Adauga'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 4
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = cxButton1Click
            end
            object cxButton2: TcxButton
              Left = 646
              Top = 178
              Width = 60
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Sterge'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 5
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = cxButton2Click
            end
            object cxButton4: TcxButton
              Left = 710
              Top = 178
              Width = 63
              Height = 25
              Anchors = [akTop, akRight]
              Caption = 'Modifica'
              Colors.Default = clSkyBlue
              LookAndFeel.Kind = lfOffice11
              TabOrder = 6
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
              OnClick = cxButton4Click
            end
            object cxDBButtonEdit1: TcxDBButtonEdit
              Left = 582
              Top = 98
              Hint = 'Codul Functional asociat proiectului.'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'COD_ECONOMIC'
              DataBinding.DataSource = DTSolduri
              Properties.Buttons = <
                item
                  Default = True
                  Kind = bkEllipsis
                end>
              Properties.OnButtonClick = edCEPropertiesButtonClick
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 7
              Width = 193
            end
            object cxDBButtonEdit2: TcxDBButtonEdit
              Left = 582
              Top = 59
              Hint = 'Codul Functional asociat proiectului.'
              Anchors = [akTop, akRight]
              DataBinding.DataField = 'COD_FUNCTIONAL'
              DataBinding.DataSource = DTSolduri
              Properties.Buttons = <
                item
                  Default = True
                  Kind = bkEllipsis
                end>
              Properties.OnButtonClick = edCFPropertiesButtonClick
              Style.Color = 12910591
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 8
              Width = 193
            end
          end
        end
        object ckDragDrop: TcxCheckBox
          Left = 691
          Top = 3
          Anchors = [akTop, akRight]
          Caption = 'Muta Drag/Drop'
          Properties.ImmediatePost = True
          Properties.OnEditValueChanged = ckDragDropPropertiesEditValueChanged
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 3
        end
        object btnAddSubproiect: TcxButton
          Left = 90
          Top = 1
          Width = 90
          Height = 29
          Hint = 'Adaugare de proiect'
          Caption = 'Subroiect'
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360900000000000036000000280000001800000018000000010020000000
            000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FEFEFEFFA8CFACFF58A561FF2A953DFF2A95
            3DFF54A35DFFA3CDA7FFFBFDFBFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00F5F9F5FF62AA69FF249C42FF24BB60FF16BD5DFF1CBC
            60FF2DBC66FF2A9E48FF5AA562FFF0F7F1FFFFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF0072B478FF3AA659FF46CD82FF2BC970FFA7D2BAFFAAC4
            B6FF34C373FF51CE89FF43AA60FF66AB6DFFFEFEFEFFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FEFE
            FEFFE5E5E5FFA7B7A7FF319943FF66CD93FF45CB7FFF30C670FFD4EDDFFFD4DB
            D8FF37BF71FF4BCC83FF6BCF97FF329746FFC2DEC5FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00F9F9F9FFDBDBDBFFB4B4B4FF9D9D9DFF9A9A
            9AFFC2BCC1FF74A978FF45A961FF7BC89EFF6EBC8FFF64BA89FFDBEAE1FFD9DF
            DCFF68B689FF70BB91FF78C29AFF53B372FF78B67FFFFFFFFF00FFFFFF00FFFF
            FF00E7E7E7FFB3B3B3FF979797FF8E8E8EFF9A9A9AFFA8A8A8FFBBBBBBFFCBCB
            CBFFE1DDE1FF68A86EFF3EAA60FFB6DBC8FFFCF2F9FFF7F0F4FFFDFCFDFFFDFC
            FCFFF7F1F5FFF9EFF5FFADC7B9FF54B87AFF64AC6BFFDBDBDBFFA3A3A3FF9C9C
            9CFFA2A2A2FFB1B1B1FFB8B8B8FFB2B2B2FFACACACFFABABABFFB3B3B3FFC0C0
            C0FFDCD8DCFF69A76FFF37AA5CFF95D4B2FFC7E5D3FFCBE4D6FFF3F7F5FFF5F7
            F6FFCFE4D7FFCAE5D6FF9BD0B3FF51B978FF66AD6EFFAEAEAEFFD1D1D1FFD2D2
            D2FFC4C4C4FFB5B5B5FFAAAAAAFFA7A7A7FFA9A9A9FFA9A9A9FFB2B2B2FFC0C0
            C0FFDED8DEFF89B58BFF2FA14EFF5DCD8FFF63C58EFF7AC49AFFE0EBE5FFD8DC
            D9FF7FBF9AFF71C796FF68CE97FF42AD64FF8CC190FFAEAEAEFFCACACAFFC6C6
            C6FFBABABAFFA2A2A2FF949494FF9E9E9EFFA4A4A4FFAAAAAAFFB2B2B2FFC0C0
            C0FFD7D5D7FFD0D9CFFF30923FFF50C07CFF77CE9FFF93C9ABFFE7ECE9FFDEDE
            DEFF92C4A8FF83CFA5FF65C98FFF27903AFFACBEADFFADADADFFCACACAFFC7C7
            C7FFB0B0B0FFC7C7C7FF9C9C9CFFBBBBBBFFA9A9A9FFA8A8A8FFB2B2B2FFC0C0
            C0FFD2D2D2FFF0EBEFFFA1C7A3FF208F35FF60C086FF9CD1B5FFC4D5CDFFC3D4
            CCFF9ACFB4FF6EC894FF27983FFF6A9C6DFFA19DA1FFACACACFFCBCBCBFFC6C6
            C6FFB9B9B9FF9D9D9DFF818181FF9D9D9DFFA2A2A2FFA5A5A5FFB2B2B2FFC0C0
            C0FFD2D2D2FFE6E6E6FFFDF9FDFFA7CEAAFF2E933EFF35A251FF58B375FF58B3
            76FF39A556FF258F37FF6EA072FFBFBCBFFF9E9E9EFFABABABFFCCCCCCFFC6C6
            C6FFB8B8B8FF989898FFA1A1A1FF878787FF9C9C9CFFA6A6A6FFB2B2B2FFC1C1
            C1FFD4D4D4FFE8E8E8FFF6F5F6FFFFFFFF00CAD7CBFF5B945EFF43974DFF4A9E
            54FF67A66CFFAEC1AFFFCCC7CCFFBEBCBEFF9C9C9CFFAAAAAAFFCECECEFFC8C8
            C8FFB8B8B8FFC0C0C0FFB5B5B5FFB2B2B2FFBBBBBBFFA2A2A2FFB0B0B0FFB8B9
            B9FFC7C7C7FFD2D2D2FFD5D5D5FFCBCBCBFFC5C2C4FFD3CDD2FFBCBBBDFFD9D8
            D8FFE8E3E8FFDBD7DAFFC5C4C4FFBDBDBDFF9B9B9BFFAEAEAEFFCAC9C9FFBEBE
            BEFFA9A9A9FFCACACAFFE1E1E1FFE7E7E7FFBEBEBEFFA2A3A3FFB1B1B1FFB6B6
            B6FFB1B1B1FFA8A8A8FFA3A3A3FF989898FFA3A3A3FFCDCCCDFF9A999AFFACAC
            ADFFBFBFC0FFC8C8C8FFBFBFBFFFBDBDBDFF9D9D9DFF959596FF838384FF8081
            81FF7C7C7CFF8A8A8AFFA1A1A1FF969696FF878787FF7B7B7BFF858585FF8787
            87FF8B8B8BFF939393FF969696FF919191FF9D9D9DFFB9B9B9FF848484FF8181
            81FF878787FF939393FF9F9F9FFF9F9F9FFF9B9C9CFF7F7F7FFF717171FF7777
            77FF7E7E7EFF797979FF818181FF898989FF919191FFA2A2A2FFB9B9B9FFC0C0
            C0FFC8C8C8FFCECECEFFCCCCCCFFBDBDBEFFBEBEBEFFDDDDDDFFAFB0B0FFABAB
            ABFFB4B4B4FFB2B2B2FF9E9E9EFF8A8A8AFF8A8A8AFFA1A1A2FF929292FF9797
            97FF9C9C9CFF9C9C9CFF9D9D9DFFA1A1A2FFA5A5A5FFAAABABFFB0B0B0FFB8B8
            B9FFC3C3C3FFCFD0D0FFDADADAFFE1E1E1FFD0D0D0FFB8B7B6FFBDBDBEFFCACB
            CBFFC8C8C8FFC5C5C6FFBEBEBEFFAEAFAFFF9FA0A0FFB5B5B6FFCDCDCDFFCDCD
            CDFFC4C4C4FFBDBDBDFFB4B4B4FFADADADFFADADADFFB0B1B1FFBBBBBBFFCBCB
            CBFFE0E0E0FFF6F5F5FFFFFFFF00FFFFFF00F4F4F3FFC9C8C5FFE1E1E2FFFDFD
            FDFFF1F1F1FFDCDCDCFFC9CACAFFC1C1C0FFA3A4A4FFBFBFBFFFA1A1A1FF9F9F
            9FFF9A9A9AFF949494FF888888FF808080FF868686FF909090FF999999FFA1A1
            A1FFA5A5A5FFAAAAAAFFA0A0A0FFA0A1A1FF8F9090FF929191FF9F9F9FFFABAB
            ABFFA7A7A7FFA1A1A1FF9A9A9AFF989898FFA7A7A7FFFEFEFEFFF9F9F9FFFAFA
            FAFFFBFBFBFFFBFBFBFFAAAAAAFF4C4E4FFF8E8F90FFE9E9E9FFC0C0C0FFB6B6
            B6FFCFCFCFFFECECECFF555758FF474A4CFF3D4041FFC7C7C7FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FEFEFEFFFDFDFDFFFEFEFEFFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00C7C7C7FF888A8CFF717374FF696A6BFF606162FF6769
            6AFF6E6F70FF838384FF989999FFBABCBCFFA7A7A7FFDADADAFFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00F4F4F4FFB5B5B5FFC0C0C0FFD4D4D4FFE4E4E4FFE8E8
            E8FFE5E6E6FFE4E4E4FFEBEBEAFFE0E0E0FFBBBBBBFFF4F4F4FFFFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DCDCDCFFC3C3C3FFB8B8B8FFAFAF
            AFFFA9A9A9FFAAAAAAFFB4B4B4FFCBCBCBFFF8F8F8FFFFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
            FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
          TabOrder = 4
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
          OnClick = btnAddSubproiectClick
        end
      end
      object pnTipProiect: TcxGroupBox
        Left = 120
        Top = 88
        Anchors = [akLeft, akTop, akBottom]
        PanelStyle.Active = True
        TabOrder = 1
        Visible = False
        DesignSize = (
          273
          160)
        Height = 160
        Width = 273
        object Bevel3: TBevel
          Left = 5
          Top = 116
          Width = 263
          Height = 9
          Anchors = [akLeft, akRight, akBottom]
          Shape = bsBottomLine
          ExplicitTop = 196
        end
        object btnCancelTipProiect: TcxButton
          Left = 205
          Top = 129
          Width = 61
          Height = 25
          Anchors = [akRight, akBottom]
          Caption = 'Cancel'
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360400000000000036000000280000001000000010000000010020000000
            000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
            F800F8F8F800F8F8F800EDEDF1FFBFBFD5FF9191B9FF8F8FB7FFBBBBD3FFEAEA
            F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
            F800EFEFF2FF8787B2FF303089FF12129CFF0E0EACFF0D0DADFF12129EFF2B2B
            89FF7E7EACFFE9E9EFFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DADA
            E5FF42428BFF0D0DA9FF0101DBFF0000E9FF0000EFFF0000F0FF0000EAFF0000
            DEFF0C0CAFFF393987FFCFCFDFFFF8F8F800F8F8F800F8F8F800EAEAF0FF3E3E
            88FF0707B8FF0000E1FF0101EEFF0000F6FF0000F8FF0000F8FF0000F6FF0202
            EFFF0000E3FF0505BEFF333386FFE1E1EAFFF8F8F800F8F8F8007B7BABFF0C0C
            A4FF0000D9FF1010E9FFACACF0FF4444F1FF0101F8FF0000F8FF4343F6FFA6A6
            EDFF1010EAFF0000DAFF0909ADFF68689EFFF7F7F8FFE5E5EDFF242483FF0000
            C4FF0101DBFFB6B6F1FFF8F8F800E6E6F3FF4545F1FF4343F6FFEAEAF6FFF7F7
            F8FFAFAFEDFF0202DEFF0000C9FF1B1B84FFD9D9E5FFB4B4CEFF111190FF0000
            C8FF0000D9FF4242E0FFEDEDF6FFF8F8F800EBEBF4FFEFEFF6FFF8F8F800EEEE
            F7FF4545E8FF0000DBFF0000CAFF0E0E95FFA4A4C4FF8C8CB4FF0C0C97FF0000
            C4FF0000D4FF0000E0FF4343E4FFEFEFF7FFF8F8F800F8F8F800F0F0F7FF4747
            EBFF0000E1FF0000D5FF0000C6FF09099DFF8787B2FF8B8BB4FF0C0C93FF0000
            BDFF0000CBFF0000D7FF5050E7FFF2F2F7FFF8F8F800F8F8F800EDEDF4FF4F4F
            E2FF0101D7FF0000CDFF0000BEFF09099AFF8787B2FFB2B2CDFF111188FF0000
            B3FF0A0AC2FF5555D9FFF0F0F6FFF8F8F800EDEDF6FFEBEBF5FFF8F8F800EBEB
            F2FF5353D7FF0A0AC4FF0000B5FF0E0E8DFFA3A3C3FFE5E5ECFF23237FFF0000
            A8FF1B1BBAFFB6B6E2FFF8F8F800E8E8F5FF3A3AD5FF3737CEFFE6E6F3FFF8F8
            F800B4B4E4FF1C1CBEFF0000AAFF1C1C7FFFD9D9E4FFF8F8F8007A7AA9FF0C0C
            8EFF1919B2FF5353C6FFB1B1DFFF4545CBFF0404C0FF0303C0FF4040C4FFB1B1
            E2FF5454C9FF1C1CB4FF080893FF65659DFFF7F7F8FFF8F8F800E9E9EFFF3C3C
            86FF080895FF4F4FC0FF8282D4FF6C6CD0FF5555CBFF5454CAFF6969CFFF8282
            D5FF5353C2FF070797FF323281FFDFDFE9FFF8F8F800F8F8F800F8F8F800D8D8
            E5FF404088FF12128BFF4F4FB9FFA4A4DBFFBFBFE4FFC0C0E4FFA7A7DDFF5454
            BCFF13138FFF373784FFCDCDDDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
            F800EDEDF2FF8484AFFF2E2E82FF1A1A85FF2E2E95FF2F2F95FF1B1B86FF2929
            80FF7A7AAAFFE6E6EDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
            F800F8F8F800F8F8F800EBEBF0FFBCBCD3FF9191B7FF8D8DB6FFB7B7D1FFE8E8
            EEFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
          TabOrder = 0
          OnClick = btnCancelTipProiectClick
        end
        object btnOkTipProiect: TcxButton
          Left = 154
          Top = 129
          Width = 48
          Height = 25
          Anchors = [akRight, akBottom]
          Caption = 'Ok'
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360400000000000036000000280000001000000010000000010020000000
            000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
            F800F4F4F4FFD9DDD9FFA9BDA9FF7BA57BFF639C64FF639C64FF7EA87EFFABC0
            ABFFD9DED9FFF5F5F5FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EDEE
            EDFFB8C5B8FF58A459FF27B429FF11D015FF06E00DFF06E00CFF10D016FF24B8
            27FF55AB57FFB9C8B9FFEEEFEEFFF8F8F800F8F8F800F8F8F800F0F0F0FFACBC
            ACFF43A846FF0BD711FF03EA0BFF16E11DFF06F20EFF00F708FF00F608FF00F2
            08FF08DD0FFF3DB040FFAAC3AAFFF0F1F0FFF8F8F800F7F7F7FFC6CFC6FF4BA7
            4CFF0AD410FF01F009FF36DF3BFF8FC590FF2EDC34FF00F808FF00F808FF00F8
            08FF00F408FF07DB0EFF42B045FFC4D2C4FFF7F7F7FFE8EAE8FF75AE76FF0EC3
            14FF06E50DFF46DA4BFFC1DAC2FFE9EAE9FFA9CAAAFF12E819FF00F708FF00F8
            08FF00F808FF00ED08FF0ACB10FF69B26BFFE5EAE5FFCFDACFFF3CA43EFF21CF
            27FF5ED662FFCCDBCCFFDFECDFFFC5E9C5FFE0E7E0FF77CB79FF12EA19FF00F8
            08FF00F808FF00F108FF00DB08FF2EAB30FFC9D9C9FFB4CAB4FF1FA622FF31CC
            37FFB0DCB0FFF3F4F3FFA9E7ABFF44DF48FFC7EEC8FFD4DFD4FF67D069FF0EEC
            14FF00F808FF00F108FF00DE08FF13B118FFA4C5A4FFA9C5A9FF16A81AFF06D3
            0EFF25E12DFF86E288FF5BE760FF04F30CFF6EE571FFF4F4F4FFCEDCCFFF64CD
            67FF0BED12FF00EE08FF00DA08FF08B40FFF8DBA8EFFADC9AEFF17A11BFF01CA
            09FF00E308FF00F408FF00F708FF00F808FF05F00DFFB9E9BAFFF5F5F5FFE2E2
            E2FF92C593FF12D519FF00D009FF09AB0EFF91BC91FFC4D6C4FF259928FF04BD
            0BFF02D60AFF00EB08FF00F308FF00F708FF00F808FF3CE942FFCEEFD0FFF6F6
            F6FFD9D9D9FF19C120FF04C20CFF169D1AFFADCAADFFDFE7DFFF53A455FF10AC
            14FF0BC613FF00D908FF00E708FF00EE08FF00F108FF08EE0FFF5BE25FFFD7EE
            D8FFD9D9D9FF0CB612FF0FB214FF399C3BFFD2DFD2FFF2F4F2FF9ABF9BFF229B
            25FF28B92BFF09C410FF00D108FF00DC08FF00E008FF00DF08FF0AD811FF59CF
            5DFFCDDFCEFF2FAC31FF1E9F21FF81B481FFEDF0EDFFF8F8F800E4E9E4FF76AC
            76FF38A33AFF44BE48FF20BD25FF0BBE12FF05C10CFF05C10BFF0BBE10FF1EBB
            21FF57BA59FF43A445FF5FA55FFFD9E2D9FFF8F8F800F8F8F800F8F8F800DAE2
            DAFF78AE78FF53A955FF63BF66FF60C563FF54C457FF54C457FF60C562FF65C0
            67FF54AC56FF66A867FFCBD9CCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8
            F800E4EAE4FFA0C3A0FF7AB07BFF70AC71FF74B274FF74B275FF71AD71FF77AD
            77FF96BD96FFDCE4DCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
            F800F8F8F800F2F4F2FFE0E8E0FFCADACAFFB8CFB8FFB7CEB7FFC9D9C9FFDEE7
            DEFFF0F2F0FFF8F8F800F8F8F800F8F8F800F8F8F800}
          TabOrder = 1
          OnClick = btnOkTipProiectClick
        end
        object cxButton3: TcxButton
          Left = 7
          Top = 129
          Width = 90
          Height = 25
          Anchors = [akLeft, akBottom]
          Caption = 'Tip Proiect'
          LookAndFeel.Kind = lfOffice11
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
          TabOrder = 2
          OnClick = cxButton3Click
        end
        object TreeTipProiect: TcxDBTreeList
          Left = 3
          Top = 4
          Width = 268
          Height = 119
          Anchors = [akLeft, akTop, akRight, akBottom]
          Bands = <
            item
              Caption.Text = 'Band1'
              Width = 300
            end>
          DataController.DataSource = DTOITipuriProiecte
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_OI_TIPURI_PROIECTE'
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.CopyCaptionsToClipboard = False
          OptionsData.Editing = False
          OptionsData.Deleting = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Headers = False
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.TreeListStyleSheetUserFormat4
          TabOrder = 3
          OnChange = TreeTipProiectChange
          OnDblClick = TreeTipProiectDblClick
          object TreeTipProiectID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_OI_TIPURI_PROIECTE'
            Width = 100
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeTipProiectDENUMIRE: TcxDBTreeListColumn
            DataBinding.FieldName = 'DENUMIRE'
            Width = 266
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeTipProiectID_PARINTE: TcxDBTreeListColumn
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
      object pnTop: TcxGroupBox
        AlignWithMargins = True
        Left = 5
        Top = 5
        Align = alTop
        PanelStyle.Active = True
        TabOrder = 2
        OnResize = pnTopResize
        DesignSize = (
          781
          34)
        Height = 34
        Width = 781
        object Label13: TLabel
          Left = 4
          Top = 9
          Width = 80
          Height = 13
          Caption = '&Cod Functional : '
          Layout = tlCenter
        end
        object btnRefresh: TcxButton
          Left = 704
          Top = 5
          Width = 73
          Height = 24
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
          Top = 6
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
          TabOrder = 1
          Width = 610
        end
      end
    end
    object pnBotomSelect: TcxGroupBox
      Left = 2
      Top = 499
      Align = alBottom
      PanelStyle.Active = True
      TabOrder = 2
      DesignSize = (
        1080
        95)
      Height = 95
      Width = 1080
      object btnOkSelect: TcxButton
        Left = 901
        Top = 6
        Width = 80
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Ok'
        LookAndFeel.Kind = lfOffice11
        ModalResult = 1
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
          F800F4F4F4FFD9DDD9FFA9BDA9FF7BA57BFF639C64FF639C64FF7EA87EFFABC0
          ABFFD9DED9FFF5F5F5FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EDEE
          EDFFB8C5B8FF58A459FF27B429FF11D015FF06E00DFF06E00CFF10D016FF24B8
          27FF55AB57FFB9C8B9FFEEEFEEFFF8F8F800F8F8F800F8F8F800F0F0F0FFACBC
          ACFF43A846FF0BD711FF03EA0BFF16E11DFF06F20EFF00F708FF00F608FF00F2
          08FF08DD0FFF3DB040FFAAC3AAFFF0F1F0FFF8F8F800F7F7F7FFC6CFC6FF4BA7
          4CFF0AD410FF01F009FF36DF3BFF8FC590FF2EDC34FF00F808FF00F808FF00F8
          08FF00F408FF07DB0EFF42B045FFC4D2C4FFF7F7F7FFE8EAE8FF75AE76FF0EC3
          14FF06E50DFF46DA4BFFC1DAC2FFE9EAE9FFA9CAAAFF12E819FF00F708FF00F8
          08FF00F808FF00ED08FF0ACB10FF69B26BFFE5EAE5FFCFDACFFF3CA43EFF21CF
          27FF5ED662FFCCDBCCFFDFECDFFFC5E9C5FFE0E7E0FF77CB79FF12EA19FF00F8
          08FF00F808FF00F108FF00DB08FF2EAB30FFC9D9C9FFB4CAB4FF1FA622FF31CC
          37FFB0DCB0FFF3F4F3FFA9E7ABFF44DF48FFC7EEC8FFD4DFD4FF67D069FF0EEC
          14FF00F808FF00F108FF00DE08FF13B118FFA4C5A4FFA9C5A9FF16A81AFF06D3
          0EFF25E12DFF86E288FF5BE760FF04F30CFF6EE571FFF4F4F4FFCEDCCFFF64CD
          67FF0BED12FF00EE08FF00DA08FF08B40FFF8DBA8EFFADC9AEFF17A11BFF01CA
          09FF00E308FF00F408FF00F708FF00F808FF05F00DFFB9E9BAFFF5F5F5FFE2E2
          E2FF92C593FF12D519FF00D009FF09AB0EFF91BC91FFC4D6C4FF259928FF04BD
          0BFF02D60AFF00EB08FF00F308FF00F708FF00F808FF3CE942FFCEEFD0FFF6F6
          F6FFD9D9D9FF19C120FF04C20CFF169D1AFFADCAADFFDFE7DFFF53A455FF10AC
          14FF0BC613FF00D908FF00E708FF00EE08FF00F108FF08EE0FFF5BE25FFFD7EE
          D8FFD9D9D9FF0CB612FF0FB214FF399C3BFFD2DFD2FFF2F4F2FF9ABF9BFF229B
          25FF28B92BFF09C410FF00D108FF00DC08FF00E008FF00DF08FF0AD811FF59CF
          5DFFCDDFCEFF2FAC31FF1E9F21FF81B481FFEDF0EDFFF8F8F800E4E9E4FF76AC
          76FF38A33AFF44BE48FF20BD25FF0BBE12FF05C10CFF05C10BFF0BBE10FF1EBB
          21FF57BA59FF43A445FF5FA55FFFD9E2D9FFF8F8F800F8F8F800F8F8F800DAE2
          DAFF78AE78FF53A955FF63BF66FF60C563FF54C457FF54C457FF60C562FF65C0
          67FF54AC56FF66A867FFCBD9CCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8
          F800E4EAE4FFA0C3A0FF7AB07BFF70AC71FF74B274FF74B275FF71AD71FF77AD
          77FF96BD96FFDCE4DCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F2F4F2FFE0E8E0FFCADACAFFB8CFB8FFB7CEB7FFC9D9C9FFDEE7
          DEFFF0F2F0FFF8F8F800F8F8F800F8F8F800F8F8F800}
        TabOrder = 0
        OnClick = btnOkSelectClick
      end
      object btnCancelSelect: TcxButton
        Left = 987
        Top = 6
        Width = 80
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Abandon'
        LookAndFeel.Kind = lfOffice11
        ModalResult = 2
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800EDEDF1FFBFBFD5FF9191B9FF8F8FB7FFBBBBD3FFEAEA
          F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800EFEFF2FF8787B2FF303089FF12129CFF0E0EACFF0D0DADFF12129EFF2B2B
          89FF7E7EACFFE9E9EFFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DADA
          E5FF42428BFF0D0DA9FF0101DBFF0000E9FF0000EFFF0000F0FF0000EAFF0000
          DEFF0C0CAFFF393987FFCFCFDFFFF8F8F800F8F8F800F8F8F800EAEAF0FF3E3E
          88FF0707B8FF0000E1FF0101EEFF0000F6FF0000F8FF0000F8FF0000F6FF0202
          EFFF0000E3FF0505BEFF333386FFE1E1EAFFF8F8F800F8F8F8007B7BABFF0C0C
          A4FF0000D9FF1010E9FFACACF0FF4444F1FF0101F8FF0000F8FF4343F6FFA6A6
          EDFF1010EAFF0000DAFF0909ADFF68689EFFF7F7F8FFE5E5EDFF242483FF0000
          C4FF0101DBFFB6B6F1FFF8F8F800E6E6F3FF4545F1FF4343F6FFEAEAF6FFF7F7
          F8FFAFAFEDFF0202DEFF0000C9FF1B1B84FFD9D9E5FFB4B4CEFF111190FF0000
          C8FF0000D9FF4242E0FFEDEDF6FFF8F8F800EBEBF4FFEFEFF6FFF8F8F800EEEE
          F7FF4545E8FF0000DBFF0000CAFF0E0E95FFA4A4C4FF8C8CB4FF0C0C97FF0000
          C4FF0000D4FF0000E0FF4343E4FFEFEFF7FFF8F8F800F8F8F800F0F0F7FF4747
          EBFF0000E1FF0000D5FF0000C6FF09099DFF8787B2FF8B8BB4FF0C0C93FF0000
          BDFF0000CBFF0000D7FF5050E7FFF2F2F7FFF8F8F800F8F8F800EDEDF4FF4F4F
          E2FF0101D7FF0000CDFF0000BEFF09099AFF8787B2FFB2B2CDFF111188FF0000
          B3FF0A0AC2FF5555D9FFF0F0F6FFF8F8F800EDEDF6FFEBEBF5FFF8F8F800EBEB
          F2FF5353D7FF0A0AC4FF0000B5FF0E0E8DFFA3A3C3FFE5E5ECFF23237FFF0000
          A8FF1B1BBAFFB6B6E2FFF8F8F800E8E8F5FF3A3AD5FF3737CEFFE6E6F3FFF8F8
          F800B4B4E4FF1C1CBEFF0000AAFF1C1C7FFFD9D9E4FFF8F8F8007A7AA9FF0C0C
          8EFF1919B2FF5353C6FFB1B1DFFF4545CBFF0404C0FF0303C0FF4040C4FFB1B1
          E2FF5454C9FF1C1CB4FF080893FF65659DFFF7F7F8FFF8F8F800E9E9EFFF3C3C
          86FF080895FF4F4FC0FF8282D4FF6C6CD0FF5555CBFF5454CAFF6969CFFF8282
          D5FF5353C2FF070797FF323281FFDFDFE9FFF8F8F800F8F8F800F8F8F800D8D8
          E5FF404088FF12128BFF4F4FB9FFA4A4DBFFBFBFE4FFC0C0E4FFA7A7DDFF5454
          BCFF13138FFF373784FFCDCDDDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800EDEDF2FF8484AFFF2E2E82FF1A1A85FF2E2E95FF2F2F95FF1B1B86FF2929
          80FF7A7AAAFFE6E6EDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800EBEBF0FFBCBCD3FF9191B7FF8D8DB6FFB7B7D1FFE8E8
          EEFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
        TabOrder = 1
        OnClick = btnCancelSelectClick
      end
    end
  end
  object DTOIProiecte: TDataSource
    DataSet = qryOIProiecte
    Left = 328
    Top = 56
  end
  object qryOIProiecte: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryOIProiecteAfterOpen
    UpdateObject = usOIProiecte
    OnNewRecord = qryOIProiecteNewRecord
    SQL.Strings = (
      'SELECT * FROM OI_PROIECTE'
      '')
    Params = <>
    Left = 361
    Top = 55
  end
  object DTOITipuriProiecte: TDataSource
    DataSet = qryOITipuriProiecte
    Left = 312
    Top = 224
  end
  object qryOITipuriProiecte: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec  SP_OI_GET_TIPURI_PROIECTE')
    Params = <>
    Left = 344
    Top = 224
  end
  object DTBuget: TDataSource
    DataSet = qryBuget
    Left = 105
    Top = 505
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
  object DTSolduri: TDataSource
    DataSet = QrySolduri
    Left = 201
    Top = 505
  end
  object QrySolduri: TZQuery
    Connection = frmData.dbContabilitate
    AutoCalcFields = False
    BeforeOpen = QrySolduriBeforeOpen
    AfterOpen = QrySolduriAfterOpen
    OnNewRecord = QrySolduriNewRecord
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
  object DTCF: TDataSource
    DataSet = QryCF
    Left = 9
    Top = 505
  end
  object QryCF: TZQuery
    Connection = frmData.dbContabilitate
    AutoCalcFields = False
    OnNewRecord = QryCFNewRecord
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
      '  este_procentual = :este_procentual,'
      '  id_oi_proiecte_detalii = :id_oi_proiecte_detalii,'
      '  data_proiect = :data_proiect'
      'WHERE'
      
        '  ((OI_PROIECTE.id_oi_proiecte IS NULL AND :OLD_id_oi_proiecte I' +
        'S '
      'NULL) OR (OI_PROIECTE.id_oi_proiecte = :OLD_id_oi_proiecte))')
    UseSequenceFieldForRefreshSQL = False
    Left = 426
    Top = 57
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
        Name = 'este_procentual'
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
  object cxGridPopupMenu: TcxGridPopupMenu
    PopupMenus = <>
    Left = 32
    Top = 112
  end
end
