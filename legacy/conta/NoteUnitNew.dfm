object FrmListaNoteNew: TFrmListaNoteNew
  Left = 326
  Top = 196
  Caption = 'Note Contabile'
  ClientHeight = 583
  ClientWidth = 777
  Color = clGray
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnShow = FormShow
  DesignSize = (
    777
    583)
  PixelsPerInch = 96
  TextHeight = 13
  object pnClient: TPanel
    Left = 0
    Top = 0
    Width = 777
    Height = 532
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    BevelOuter = bvNone
    TabOrder = 0
    object pnTop: TPanel
      Left = 0
      Top = 0
      Width = 777
      Height = 57
      Align = alTop
      TabOrder = 0
      DesignSize = (
        777
        57)
      object Label1: TLabel
        Left = 8
        Top = 8
        Width = 122
        Height = 13
        Caption = 'Anul / &Luna de raportare :'
        FocusControl = edListaLuni
      end
      object Label2: TLabel
        Left = 9
        Top = 32
        Width = 100
        Height = 13
        Caption = '&Numar / Data Nota : '
        FocusControl = edNrNota
      end
      object Label3: TLabel
        Left = 310
        Top = 8
        Width = 76
        Height = 13
        Caption = '&Operator Nota : '
      end
      object LbCurent: TLabel
        Left = 483
        Top = 32
        Width = 3
        Height = 13
        Color = clGray
        ParentColor = False
      end
      object Label4: TLabel
        Left = 313
        Top = 32
        Width = 45
        Height = 13
        Caption = 'Nr &Unic : '
        FocusControl = edNrNota
      end
      object edOperator: TcxImageComboBox
        Left = 390
        Top = 4
        Anchors = [akLeft, akTop, akRight]
        Properties.Items = <>
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 6
        Width = 352
      end
      object edListaAni: TcxImageComboBox
        Left = 133
        Top = 5
        Properties.Items = <>
        Properties.OnChange = edListaAniPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 3
        Width = 65
      end
      object edListaLuni: TcxImageComboBox
        Left = 202
        Top = 5
        Properties.Items = <>
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 0
        Width = 103
      end
      object edNrNota: TcxTextEdit
        Left = 133
        Top = 29
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        Width = 65
      end
      object edData: TcxDateEdit
        Left = 202
        Top = 29
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        Width = 103
      end
      object edNrUnic: TcxTextEdit
        Left = 390
        Top = 29
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 5
        Width = 86
      end
      object ChkShowAll: TcxCheckBox
        Left = 602
        Top = 32
        Anchors = [akTop, akRight]
        Caption = 'Arata si notele de istoric'
        Properties.OnChange = edNrNotaPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 4
      end
    end
    object cxGridIstoricNote: TcxGrid
      Left = 0
      Top = 57
      Width = 777
      Height = 475
      Align = alClient
      TabOrder = 1
      LookAndFeel.Kind = lfOffice11
      ExplicitTop = 63
      object GridIstoricNote: TcxGridDBTableView
        OnKeyDown = GridIstoricNoteKeyDown
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        OnCustomDrawCell = GridIstoricNoteCustomDrawCell
        OnFocusedRecordChanged = GridIstoricNoteFocusedRecordChanged
        DataController.DataSource = DTListaNote
        DataController.Filter.MaxValueListCount = 1000
        DataController.Filter.Options = [fcoCaseInsensitive]
        DataController.Filter.OnChanged = GridIstoricNoteDataControllerFilterChanged
        DataController.Filter.Active = True
        DataController.KeyFieldNames = 'NR'
        DataController.Options = [dcoAnsiSort, dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding]
        DataController.Summary.DefaultGroupSummaryItems.Separator = ', '
        DataController.Summary.DefaultGroupSummaryItems = <
          item
            Kind = skSum
            Position = spFooter
            FieldName = 'VALOARE'
          end>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        Filtering.ColumnPopup.MaxDropDownItemCount = 12
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ImmediateEditor = False
        OptionsCustomize.ColumnHiding = True
        OptionsCustomize.ColumnsQuickCustomization = True
        OptionsData.Editing = False
        OptionsSelection.CellSelect = False
        OptionsSelection.HideFocusRectOnExit = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.Footer = True
        OptionsView.GroupByBox = False
        OptionsView.GroupFooters = gfVisibleWhenExpanded
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        object GridIstoricNoteDENJURNAL: TcxGridDBColumn
          Caption = 'Jurnal'
          DataBinding.FieldName = 'DenJurnal'
        end
        object GridIstoricNoteJURNAL: TcxGridDBColumn
          Caption = 'Jurnal Id'
          DataBinding.FieldName = 'JURNAL'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 60
        end
        object GridIstoricNoteNRDOC: TcxGridDBColumn
          Caption = 'Nr.'
          DataBinding.FieldName = 'NRDOC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 63
        end
        object GridIstoricNoteDATA: TcxGridDBColumn
          Caption = 'Data'
          DataBinding.FieldName = 'DATA'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          HeaderAlignmentHorz = taCenter
          Width = 63
        end
        object GridIstoricNoteEXPLICATIE: TcxGridDBColumn
          Caption = 'Explicatie'
          DataBinding.FieldName = 'EXPLICATIE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 91
        end
        object GridIstoricNoteCONT_DEBT: TcxGridDBColumn
          Caption = 'Cont Deb.'
          DataBinding.FieldName = 'CONTD'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 158
        end
        object GridIstoricNoteREPARTITOR_DEBIT: TcxGridDBColumn
          Caption = 'Repartitor Debit'
          DataBinding.FieldName = 'REPARTITOR_DEBIT'
          MinWidth = 16
          Width = 56
        end
        object GridIstoricNoteCONT_CRED: TcxGridDBColumn
          Caption = 'Cont Cred.'
          DataBinding.FieldName = 'CONTC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 45
        end
        object GridIstoricNoteREPARTITOR_CREDIT: TcxGridDBColumn
          Caption = 'Repartitor Credit'
          DataBinding.FieldName = 'REPARTITOR_CREDIT'
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 56
        end
        object GridIstoricNoteVALOARE: TcxGridDBColumn
          Caption = 'Valoare'
          DataBinding.FieldName = 'VALOARE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Width = 45
        end
        object GridIstoricNoteMODUL: TcxGridDBColumn
          Caption = 'Modul'
          DataBinding.FieldName = 'MODUL'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <
            item
              Description = 'Nota introdusa'
              ImageIndex = 0
              Value = '0'
            end
            item
              Description = 'Tranzactii'
              ImageIndex = 1
              Value = '1'
            end
            item
              Description = 'Casa/Banca'
              ImageIndex = 2
              Value = '2'
            end
            item
              Description = 'Mijloace Fixe'
              ImageIndex = 3
              Value = '4'
            end
            item
              Description = 'Salarizare'
              ImageIndex = 4
              Value = '8'
            end
            item
              Description = 'Nota de Inchidere'
              ImageIndex = 5
              Value = '-1'
            end>
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 71
        end
        object GridIstoricNoteBUGET: TcxGridDBColumn
          Caption = 'Buget'
          DataBinding.FieldName = 'BUGET'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 35
        end
        object GridIstoricNoteCOD: TcxGridDBColumn
          Caption = 'Cod'
          DataBinding.FieldName = 'COD'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNotePOZ: TcxGridDBColumn
          Caption = 'Poz'
          DataBinding.FieldName = 'POZ'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteECL: TcxGridDBColumn
          Caption = 'Ecl'
          DataBinding.FieldName = 'ECL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteCOMPUSA: TcxGridDBColumn
          Caption = 'Comp'
          DataBinding.FieldName = 'COMPUSA'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteCONTD: TcxGridDBColumn
          Caption = 'ContD'
          DataBinding.FieldName = 'CONTD'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteCONTC: TcxGridDBColumn
          Caption = 'ContC'
          DataBinding.FieldName = 'CONTC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteC_O: TcxGridDBColumn
          Caption = 'Operator Id'
          DataBinding.FieldName = 'C_O'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 65
        end
        object GridIstoricNoteNumeOperator: TcxGridDBColumn
          Caption = 'Operator'
          DataBinding.FieldName = 'NumeIntreg'
        end
        object GridIstoricNoteDATA_OPERARE: TcxGridDBColumn
          Caption = 'Data Operare'
          DataBinding.FieldName = 'DATA_OPERARE'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          HeaderAlignmentHorz = taCenter
          Width = 32
        end
        object GridIstoricNoteID_INITIAL: TcxGridDBColumn
          Caption = 'Id Initial'
          DataBinding.FieldName = 'ID_INITIAL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 53
        end
        object GridIstoricNoteID_PARINTE: TcxGridDBColumn
          Caption = 'Parinte'
          DataBinding.FieldName = 'ID_PARINTE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 53
        end
        object GridIstoricNoteSTARE: TcxGridDBColumn
          Caption = 'Stare'
          DataBinding.FieldName = 'STARE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 53
        end
        object GridIstoricNoteCOD_FUNCTIONAL: TcxGridDBColumn
          Caption = 'Clas. Func.'
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 48
        end
        object GridIstoricNoteCOD_ECONOMIC: TcxGridDBColumn
          Caption = 'Clas. Ec.'
          DataBinding.FieldName = 'COD_ECONOMIC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 62
        end
        object GridIstoricNoteDATA_OP: TcxGridDBColumn
          Caption = 'Data Doc'
          DataBinding.FieldName = 'DATA_OP'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Width = 21
        end
        object GridIstoricNoteNR_OP: TcxGridDBColumn
          Caption = 'Nr Unic'
          DataBinding.FieldName = 'NR_OP'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 58
        end
        object GridIstoricNoteDATA_CONTRACT: TcxGridDBColumn
          Caption = 'Data Contract'
          DataBinding.FieldName = 'DATA_CONTRACT'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Visible = False
          Width = 53
        end
        object GridIstoricNoteNR_CONTRACT: TcxGridDBColumn
          Caption = 'Nr Contract'
          DataBinding.FieldName = 'NR_CONTRACT'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Width = 53
        end
        object GridIstoricNoteID: TcxGridDBColumn
          Caption = 'Id'
          DataBinding.FieldName = 'NR'
          Visible = False
        end
      end
      object GridIstoricNoteL: TcxGridLevel
        GridView = GridIstoricNote
      end
    end
  end
  object btnIstoric: TcxButton
    Left = 9
    Top = 540
    Width = 104
    Height = 29
    Anchors = [akLeft, akBottom]
    Caption = 'Istoric Nota'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000010000
      000B0000000A0000000000000000000000000000000013111025322D295E3B34
      2F6D24201E450605050B00000000000000000000000000000000000000000000
      0000000000000000000E000000240001023A010506550811147413232895263C
      43B517282DA0000000320000000007070613584D46999D866EF1BCA482FFC3AD
      88FFB49E80FF7B6C5DD02B262351000000000000000000000000000000070309
      0B5611252A9424414AB940646FD8638994EF82A6B1FE96B0BAFFACBCC3FFC7CE
      D1FFC7D9DFFF3E5F66CF03070858564B42BAC5A178FFDBB080FFE09474FFE295
      7BFFE0A27DFFD3AE7FFFA3896DFC342F2B620000000000000000060E115877A2
      B3FBBED4DBFFD3DFE4FFDCE0E2FFDEDEDEFFE0DDDCFFDEDBDAFFD9D7D6FFD3D2
      D1FFF7F3F2FFEAF8FDFF7F9091FABC9E7CFFDBB07CFFDFC099FFDC4639FFD705
      01FFDF7F69FFDDBE8DFFD1A36DFF957F68F21513112800000000162D378EC1D0
      D8FFFDF8F5FFF0EEEDFFE8E7E7FFE2E1E1FFDCDCDCFFD9D9D9FFD6D6D6FFD2D2
      D2FFEFEEEEFFFCF9F9FFB4A497FFE5CDAFFFE7C8A4FFE9D8B8FFE16859FFD603
      00FFE49F8AFFE0C498FFDBAC75FFC29D71FF48413A850000000015293280C0CE
      D5FFF9F7F6FFF0F0EFFFEBEAEAFFE6E5E5FFE0DDDCFFD5D2D1FFC5C2C1FFB9BB
      BBFFCED2D2FFDBD5D1FFBAAFA5FFFDF5EBFFF7E8D6FFF2E6CEFFE3685AFFD604
      00FFE7A08CFFE6CCA4FFE2B988FFDAB383FF665A4EB1000000000A1419558EB0
      C1FFE2DAD8FFD3D1D1FFB8BBBBFF9CA6A8FF7E9195FF688289FF5B7C85FF4469
      77FF538498FF83979DFFC2B9B2FFFFFFFDFFFDF7F0FFFBF8ECFFE4655AFFD501
      00FFEAA493FFECD7B3FFEBC69CFFE2C297FF6E6255B90000000C02090B563377
      92F75B8792FF5A8B97FF578A98FF64929FFF789FABFF8CAAB4FFA4B8C0FF759F
      ABFF2C6177FF447085FFB1AAA4FFFFFDFCFFFFFEFBFFFCF4F0FFE25249FFD816
      0FFFEDA699FFF6E9CDFFF5DCB7FFDACCA8FF5D544BA619333D996F98A5F098BC
      C7FDAAC5CFFFC3D1D6FFD4D7D9FFD8D4D5FFD8D0CEFFD1CBC9FFCAC8C7FFEBEE
      EEFF9BC4CFFF2D7994FF747D7DFFF2EAE6FFFFFFFFFFFEFDFDFFF1AFAAFFE358
      52FFF5CAC5FFFFFEF6FFF8F5E0FFB9B3A3FF35302B656097B1EFE9E1DEFFFBEE
      EAFFE8E1DDFFD3D2D0FFB8BDBDFF9EAFB3FF87A1ABFF6F92A1FF598598FF6E9E
      B2FFC7D9DEFFB0D2DDFF5693A2FFB1ADA6FFFDF9F7FFFFFFFFFFEE918BFFD815
      12FFF7C3BFFFFFFFFFFFD7D4D3FF736961CB0706060D4F778ACB9CC0CAFF83B5
      C5FF69A5B9FF559DB7FF4498B8FF3D92B6FF4795B9FF4D97BAFF4D92B3FF488C
      AEFF4A98B9FF93C6D5FFB6D5DFFF7AA0AAFFA9A5A0FFE9E0DBFFFAF8F7FFFAE6
      E6FFF2EDEBFFCCC8C4FF796E67D01512102700000000030405222E4752994DAC
      D5F930AAE2FF299FD6FF2E9FD5FF2FA1D6FF2EA0D6FF2C9ED4FF2999CDFF2A92
      C3FF2688B5FF1D7FA9FF4A90A6FFA0BEC8FF74B1C6FF6B888BFA98897FF4ACA6
      9FFA8A7F77E0463E37830B09081400000000000000000000000000000000090C
      0D392E5263A23AA0D0EE209DD9FF1B93CAFF1988BAFF1D80ACFF2B83A6FF3782
      A0FF49869DFF638FA1FF6998A8FF408599FF83B3C3FF5FBADBFA143841A70907
      0637090807120000000000000000000000000000000000000000000000000000
      0000000000001623287F4F93ADFB5E9AB0FF7DA8B7FF96B5BEFFAABAC0FFC0C7
      C8FFCCCDCDFFCECCCBFFE5E1DFFFAACDD7FF387D92FF517F92FE0E26308A0000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000202225E818FE5E4E4E2FEEBEAE9FFECE7E6FFEAE5E3FFE3E0DEFFDCDB
      DAFFD7D6D6FFD1D1D1FFE1E1E0FFFFFFFFFFC7DCE2FF4A8A9DFE081A20980000
      0006000000000000000000000000000000000000000000000000000000000000
      000003080B3D8FB3C4FFF9F3F0FFEFEEEEFFE8E8E8FFE3E3E3FFDFDEDEFFDBDA
      DAFFD8D7D6FFD5D2D1FFDED9D8FFFFFFFFFFFFFFFEFFDDEAEFFF5991A2E80416
      1A83000000140000000000000000000000000000000000000000000000000000
      00000103042284A7B7F5F3EDEAFFF3F1F0FFEDE8E6FFE4DEDCFFD7D5D3FFC5C7
      C6FFAFB8BBFF92A8B1FF83A2ADFFC6D1D5FFFBF7F5FFFDF8F7FFEFF3F5FF88C7
      DBFF0B2E38A30000002800000000000000000000000000000000000000000000
      00000000000239505AAABCC9CDFFC3CFD1FFA3C1C6FF86B3BEFF6BA2B4FF5C96
      AFFF6096AFFF5A8DA6FF4C819BFF4F86A0FF9FB8C0FFEDE7E4FFFCF7F6FFFEFA
      FAFF9CD9EFFF17576AC80003033B000000000000000000000000000000000000
      000000000000040506274C798ACE51B0D5FF339CC8FF3096C2FF3194C1FF3493
      C0FF3A97C2FF3F99C4FF449BC4FF4598C0FF3C99C1FF7BB7C7FFD7DAD8FFFFF8
      F5FFFFFDFCFF72AFC7EF030E1251000000000000000000000000000000000000
      000000000000000000000101010D1B2A31724494B9E235B3EEFF28A7E3FF2BA6
      DFFF2BA6DFFF2BA6DFFF2AA6DFFF2AA6DFFF29A5DFFF25A7E0FF4BB4D8FFB1D0
      D6FFFFFEFBFF535F62B700000007000000000000000000000000000000000000
      0000000000000000000000000000000000000303031C2642508C3DADE2F222B5
      FDFF1DACF3FF21ACF1FF21ADF2FF21ADF2FF21ADF2FF20ACF1FF19ABF3FF24B0
      EBFF97C5D6FF3D4E51B70000000C000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000080A0C332F68
      84B42BBBFFFF11B1FFFF10ABFBFF10ACFCFF0FADFEFF10B0FFFF16B8FFFF1EB0
      FBFF3E81A2FF325F72D900010121000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000111A1F532E81A8CB1CBAFFFF1FB6FFFF2BAAE8F42884B0D51E5771AB172F
      3B800B1012510304052800000003000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000101010A101D22590B1519510304042200000009000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 1
    OnClick = btnIstoricClick
  end
  object BtnOk: TcxButton
    Left = 594
    Top = 539
    Width = 75
    Height = 29
    Hint = 'Salvare si inchidere ecran'
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
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
    TabOrder = 2
    OnClick = BtnCancelClick
  end
  object BtnCancel: TcxButton
    Left = 674
    Top = 539
    Width = 75
    Height = 29
    Hint = 'Inchidere ecran cu pastrare a pozitilor introduse pana acum'
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
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
    TabOrder = 3
    OnClick = BtnCancelClick
  end
  object DTListaNote: TDataSource
    DataSet = QryListaNote
    Left = 40
    Top = 64
  end
  object QryListaNote: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryListaNoteAfterOpen
    SQL.Strings = (
      'SELECT '
      
        '(select nume from repartitori where id_repartitori = a.repartito' +
        'r_debit) as REPARTITOR_DEBIT, (select nume from repartitori wher' +
        'e id_repartitori = a.repartitor_credit) as REPARTITOR_CREDIT, '
      
        '(select denumire from cjurnale where jurnal = a.jurnal) as DenJu' +
        'rnal, (select numeintreg from utilizatori where id_utilizatori =' +
        ' a.c_o) as NumeIntreg, * '
      'FROM CNOTE a'
      ''
      'ORDER BY NR')
    Params = <>
    Left = 76
    Top = 64
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = cxGridIstoricNote
    PopupMenus = <>
    Left = 370
    Top = 291
  end
end
