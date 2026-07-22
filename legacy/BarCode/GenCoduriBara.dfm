object frmGenCoduriBara: TfrmGenCoduriBara
  Left = 308
  Top = 61
  Caption = 'Generare coduri bare'
  ClientHeight = 613
  ClientWidth = 862
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    Left = 0
    Top = 56
    Width = 862
    Height = 448
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 0
    ExplicitHeight = 504
    object grStock: TcxGrid
      Left = 2
      Top = 2
      Width = 858
      Height = 444
      Align = alClient
      TabOrder = 0
      ExplicitHeight = 500
      object GridStock: TcxGridDBBandedTableView
        OnKeyDown = GridStockKeyDown
        Navigator.Buttons.CustomButtons = <>
        Navigator.Visible = True
        ScrollbarAnnotations.CustomAnnotations = <>
        OnCustomDrawCell = GridStockCustomDrawCell
        OnEditKeyDown = GridStockEditKeyDown
        OnFocusedRecordChanged = GridStockFocusedRecordChanged
        DataController.DataSource = DTStock
        DataController.KeyFieldNames = 'RecId'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        FilterRow.InfoText = 'Apasa aici pentru a defini filtrul'
        OptionsBehavior.AlwaysShowEditor = True
        OptionsBehavior.DragHighlighting = False
        OptionsBehavior.DragOpening = False
        OptionsBehavior.DragScrolling = False
        OptionsBehavior.FocusCellOnTab = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = GridStockDENMAT
        OptionsBehavior.ExpandMasterRowOnDblClick = False
        OptionsCustomize.ColumnsQuickCustomization = True
        OptionsCustomize.BandsQuickCustomization = True
        OptionsData.CancelOnExit = False
        OptionsData.Deleting = False
        OptionsData.DeletingConfirmation = False
        OptionsData.Inserting = False
        OptionsSelection.MultiSelect = True
        OptionsView.ColumnAutoWidth = True
        OptionsView.Footer = True
        OptionsView.GroupByBox = False
        OptionsView.Indicator = True
        Bands = <
          item
            Caption = 'Detalii Material'
            FixedKind = fkLeft
            Width = 323
          end
          item
            Caption = 'Detalii Stock'
            Width = 368
          end
          item
            Caption = 'Locatii'
            Width = 151
          end>
        object GridStockSELECTAT: TcxGridDBBandedColumn
          Caption = 'Sel'
          DataBinding.FieldName = 'SELECTAT'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.FullFocusRect = True
          Properties.NullStyle = nssUnchecked
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 47
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridStockCANTITATE_SELECTATA: TcxGridDBBandedColumn
          Caption = 'Selectat'
          DataBinding.FieldName = 'CANTITATE_SELECTATA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Options.IncSearch = False
          Width = 66
          Position.BandIndex = 0
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridStockPRODUS: TcxGridDBBandedColumn
          Caption = 'Categ Prod.'
          DataBinding.FieldName = 'PRODUS'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <>
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 68
          Position.BandIndex = 0
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object GridStockTIP_STOCK: TcxGridDBBandedColumn
          DataBinding.FieldName = 'TIP_STOCK'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <
            item
              Description = 'Stock Predator'
              ImageIndex = 0
              Value = 1
            end
            item
              Description = 'Stock Primitor'
              Value = 2
            end
            item
              Description = 'Stock Predator/Primitor'
              Value = 3
            end>
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object GridStockCANTITATE: TcxGridDBBandedColumn
          Caption = 'Stock'
          DataBinding.FieldName = 'CANTITATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 60
          Position.BandIndex = 0
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object GridStockCONT: TcxGridDBBandedColumn
          Caption = 'Cont Mat.'
          DataBinding.FieldName = 'Cont'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 28
          Position.BandIndex = 1
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object GridStockDATACOD: TcxGridDBBandedColumn
          Caption = 'Data Cod'
          DataBinding.FieldName = 'DATACOD'
          PropertiesClassName = 'TcxDateEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 52
          Position.BandIndex = 1
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object GridStockNR_DOCUM: TcxGridDBBandedColumn
          Caption = 'Nr. Doc'
          DataBinding.FieldName = 'NR_DOCUM'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 51
          Position.BandIndex = 1
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object GridStockCODMAT: TcxGridDBBandedColumn
          Caption = 'CodMat'
          DataBinding.FieldName = 'CODMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 52
          Position.BandIndex = 1
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object GridStockID_INITIAL: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_INITIAL'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object GridStockID_UTILIZATORI: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_UTILIZATORI'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object GridStockPRET_UNITAR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'PRET_UNITAR'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE: TcxGridDBBandedColumn
          Caption = 'Pret Rec.'
          DataBinding.FieldName = 'PRET_RECEPTIE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 46
          Position.BandIndex = 1
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridStockCOTA_TVA: TcxGridDBBandedColumn
          Caption = '% TVA'
          DataBinding.FieldName = 'COTA_TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 44
          Position.BandIndex = 1
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE_TVA: TcxGridDBBandedColumn
          Caption = 'Pret Rec. TVA'
          DataBinding.FieldName = 'PRET_RECEPTIE_TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 46
          Position.BandIndex = 1
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object GridStockDENMAT: TcxGridDBBandedColumn
          Caption = 'Denumire'
          DataBinding.FieldName = 'DENMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          SortIndex = 0
          SortOrder = soAscending
          Width = 174
          Position.BandIndex = 0
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object GridStockTIPMAT: TcxGridDBBandedColumn
          Caption = 'Grupa'
          DataBinding.FieldName = 'TIPMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 44
          Position.BandIndex = 0
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object GridStockUM: TcxGridDBBandedColumn
          DataBinding.FieldName = 'UM'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 44
          Position.BandIndex = 0
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object GridStockLOHN: TcxGridDBBandedColumn
          DataBinding.FieldName = 'LOHN'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 9
          Position.RowIndex = 0
        end
        object GridStockDATA_COD: TcxGridDBBandedColumn
          DataBinding.FieldName = 'DATA_COD'
          PropertiesClassName = 'TcxDateEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 10
          Position.RowIndex = 0
        end
        object GridStockDATA_EXPIRARE: TcxGridDBBandedColumn
          Caption = 'Data Expirare'
          DataBinding.FieldName = 'DATA_EXPIRARE'
          PropertiesClassName = 'TcxDateEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 11
          Position.RowIndex = 0
        end
        object GridStockTVA: TcxGridDBBandedColumn
          DataBinding.FieldName = 'TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 12
          Position.RowIndex = 0
        end
        object GridStockID_GEST_SUMATOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_GEST_SUMATOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 13
          Position.RowIndex = 0
        end
        object GridStockLOT_FABRICATIE: TcxGridDBBandedColumn
          Caption = 'Lot Fabr'
          DataBinding.FieldName = 'LOT_FABRICATIE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 14
          Position.RowIndex = 0
        end
        object GridStockUM_SUPLIMENTARA: TcxGridDBBandedColumn
          Caption = 'UM Supl.'
          DataBinding.FieldName = 'UM_SUPLIMENTARA'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 15
          Position.RowIndex = 0
        end
        object GridStockCONVERSIE_UM: TcxGridDBBandedColumn
          Caption = 'Conv. UM'
          DataBinding.FieldName = 'CONVERSIE_UM'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 16
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE_VALUTA: TcxGridDBBandedColumn
          Caption = 'Pret Rec. Valuta'
          DataBinding.FieldName = 'PRET_RECEPTIE_VALUTA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 17
          Position.RowIndex = 0
        end
        object GridStockCOD_TARIF_VAMAL: TcxGridDBBandedColumn
          Caption = 'Tarif Vama'
          DataBinding.FieldName = 'COD_TARIF_VAMAL'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 18
          Position.RowIndex = 0
        end
        object GridStockTVA_AMANAT: TcxGridDBBandedColumn
          Caption = 'TVA Amanat'
          DataBinding.FieldName = 'TVA_AMANAT'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 19
          Position.RowIndex = 0
        end
        object GridStockADAOS: TcxGridDBBandedColumn
          Caption = 'Adaos'
          DataBinding.FieldName = 'ADAOS'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 20
          Position.RowIndex = 0
        end
        object GridStockADAOS_IMPUS: TcxGridDBBandedColumn
          Caption = 'Adaos Imp'
          DataBinding.FieldName = 'ADAOS_IMPUS'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 21
          Position.RowIndex = 0
        end
        object GridStockCATEGORIE_GRUPARE: TcxGridDBBandedColumn
          Caption = 'Cat. Grupare'
          DataBinding.FieldName = 'CATEGORIE_GRUPARE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 22
          Position.RowIndex = 0
        end
        object GridStockTIP_VALUTA_RECEPTIE: TcxGridDBBandedColumn
          Caption = 'Tip Valuta'
          DataBinding.FieldName = 'TIP_VALUTA_RECEPTIE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 23
          Position.RowIndex = 0
        end
        object GridStockCOTA_ADAOS: TcxGridDBBandedColumn
          Caption = '% Adaos'
          DataBinding.FieldName = 'COTA_ADAOS'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 24
          Position.RowIndex = 0
        end
        object GridStockCOTA_ADAOS_IMPUS: TcxGridDBBandedColumn
          Caption = '% Adaos Impus'
          DataBinding.FieldName = 'COTA_ADAOS_IMPUS'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 25
          Position.RowIndex = 0
        end
        object GridStockRecId: TcxGridDBBandedColumn
          DataBinding.FieldName = 'RecId'
          Visible = False
          Position.BandIndex = 0
          Position.ColIndex = 26
          Position.RowIndex = 0
        end
      end
      object grLevel: TcxGridLevel
        GridView = GridStock
      end
    end
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 862
    Height = 56
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 1
    object Label1: TLabel
      Left = 189
      Top = 8
      Width = 40
      Height = 13
      Caption = 'Tip Stoc'
    end
    object Label2: TLabel
      Left = 7
      Top = 8
      Width = 47
      Height = 13
      Caption = 'Cont Stoc'
    end
    object edtTipStoc: TcxImageComboBox
      Left = 233
      Top = 4
      ParentFont = False
      Properties.Items = <>
      Properties.OnChange = edtTipStocChange
      TabOrder = 0
      Width = 272
    end
    object edtCont: TcxButtonEdit
      Left = 67
      Top = 4
      Properties.Buttons = <>
      Properties.ReadOnly = True
      Properties.OnButtonClick = edtContButtonClick
      TabOrder = 1
      Width = 113
    end
    object ChkShowAllReady: TcxCheckBox
      Left = 5
      Top = 32
      Caption = 'Materialele deja preluate'
      TabOrder = 2
      OnClick = ChkShowAllReadyClick
    end
    object ChkShowData: TcxCheckBox
      Left = 144
      Top = 32
      Caption = 'Intrari anterioare'
      TabOrder = 3
      OnClick = ChkShowAllReadyClick
    end
    object ChkShowNegative: TcxCheckBox
      Left = 244
      Top = 32
      Caption = 'Arata stoc negativ'
      TabOrder = 4
      OnClick = ChkShowAllReadyClick
    end
    object chkStockLazi: TcxCheckBox
      Left = 354
      Top = 32
      Caption = 'Iesiri ulterioare'
      State = cbsChecked
      TabOrder = 5
      OnClick = ChkShowAllReadyClick
    end
  end
  object Panel2: TPanel
    Left = 0
    Top = 504
    Width = 862
    Height = 109
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 2
    DesignSize = (
      862
      109)
    object btnConfigBarCode: TcxButton
      Left = 9
      Top = 7
      Width = 137
      Height = 24
      Anchors = [akTop, akRight]
      Caption = 'Configurare CodBare'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00CEC6C6FF9C6B
        6BFFA57B73FF9C7B7BFF9C7B7BFF9C7B7BFF9C948CFFA59C9CFF9C7B7BFF9C73
        7BFF9C7B7BFFA5847BFFA57373FF9C8C8CFFE7E7EFFFFFEFEFFF213131FF0042
        4AFF004A5AFF005252FF004A52FF106363FF004A4AFF001821FF106B6BFF0852
        5AFF004A52FF004A5AFF00636BFF001010FF847373FFFFF7F7FF7B6363FF00B5
        B5FF10FFFFFF29F7FFFF4AFFFFFF4AEFEFFF102121FF180000FF399494FF4AFF
        FFFF31FFFFFF18FFFFFF00F7F7FF083139FFD6BDBDFFFFFFFF00FFDED6FF4252
        52FF10EFF7FF4AE7FFFF63F7FFFF5ACECEFF312121FF210810FF427373FF6BFF
        FFFF4ADEFFFF42FFFFFF005A5AFF845A5AFFFFFFFF00FFFFFF00FFFFFF00CE9C
        9CFF188C8CFF4AFFFFFF6BDEFFFF8CFFFFFF4A7B7BFF212921FF7BEFEFFF7BF7
        FFFF63FFFFFF29C6C6FF312121FFFFEFE7FFFFFFFF00FFFFFF00FFFFFF00FFF7
        F7FF6B6B63FF42CECEFF84FFFFFF7BDEDEFF393131FF310808FF5A8C8CFF8CFF
        FFFF6BFFFFFF184242FFB59494FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00DEB5B5FF4A7373FF84FFFFFF6BDEE7FF424242FF4A2121FF4A848CFF94FF
        FFFF42A5A5FF634A4AFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00847373FF5ACECEFF7BF7F7FF4A4A4AFF5A3939FF52949CFF8CFF
        F7FF394242FFD6B5B5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00EFD6D6FF52736BFF5ACED6FF636363FF73524AFF6BBDC6FF4A94
        94FF846363FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00A5948CFF297B84FF6B6B6BFF844A42FF31737BFF314A
        52FFEFDEDEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFEFE7FF424A52FF39848CFF6BA5A5FF002929FFA58C
        8CFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00B58C8CFF107B84FF29B5BDFF4A4242FFFFEF
        EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF005A6363FF000808FFDED6D6FFFFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00E7DEDEFFCEC6C6FFFFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
      TabOrder = 0
      OnClick = btnConfigBarCodeClick
    end
    object btnGenerare: TcxButton
      Left = 659
      Top = 4
      Width = 91
      Height = 27
      Anchors = [akRight, akBottom]
      Caption = 'Generare'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360900000000000036000000280000001800000018000000010020000000
        000000000000C40E0000C40E0000000000000000000000000000000000000000
        00007C531BB686521DD186531ED186531ED186531ED186531ED186531ED18653
        1ED186531ED186531ED186531ED186531ED186531ED186531ED186531ED18653
        1ED186521DD14A2C0B7E00000000000000000000000000000000000000000000
        0000B07E37DCDC9B5CFFDC9F60FFDCA061FFDCA061FFDCA061FFDCA061FFDCA0
        61FFDCA061FFDCA061FFDCA061FFDCA061FFDCA061FFDCA061FFDCA061FFDC9F
        61FFDC9C5DFF6642189800000000000000000000000000000000000000000000
        0000B2813CDCDFA66BFFDFAA6FFFDFAB71FFDFAB71FFDFAB71FFDFAB71FFDFAB
        71FFDFAB71FFDFAB71FFDFAB71FFDFAB71FFDFAB71FFDFAB71FFDFAB71FFDFAA
        70FFDFA76CFF67441B9800000000000000000000000000000000000000000000
        0000B38642DCE5B17AFFE5B67FFFE5B781FFE5B781FFE5B781FFE5B781FFE5B7
        81FFE5B781FFE5B781FFE5B781FFE5B781FFE5B781FFE5B781FFE5B781FFE5B6
        80FFE5B37CFF68461E9800000000000000000000000000000000000000000000
        0000B68A47DCEABD89FFEAC190FFEAC392FFEAC392FFEAC392FFEAC392FFEAC3
        92FFEAC392FFEAC392FFEAC392FFEAC392FFEAC392FFEAC392FFEAC392FFEAC2
        91FFEABF8BFF6948209800000000000000000000000000000000000000000000
        0000B88E4DDCEFC898FFEFCDA0FFEFCFA2FFEFCFA2FFEFCFA2FFEFCFA2FFEFCF
        A2FFEFCFA2FFEFCFA2FFE5C69BFFEFCFA2FFEFCFA2FFEFCFA2FFEFCFA2FFEFCE
        A1FFF0CA9BFF6A4B239800000000000000000000000000000000000000000000
        0000B99453DCF5D4A9FFF5D9B0FFF5DBB3FFF5DBB3FFF5DBB3FFF5DBB3FFF5DB
        B3FFF5DBB3FFF5DBB4FF236A1DFF979372FFF6DCB4FFF5DBB3FFF5DBB3FFF5DA
        B1FFF6D6ABFF6B4C269800000000000000000000000000000000000000000000
        0000BB9658DCF9DEB6FFF9E3BFFFF1DEBCFFEFDDBBFFEFDDBBFFEFDDBBFFEFDD
        BBFFEFDDBBFFEFDDBCFF11790DFF0D960AFF72895CFFF4E2BFFFF9E5C2FFF9E4
        C0FFFAE0B9FF6C4E289800000000000000000000000000000000000000000000
        0000BC9A5CDCFDE6C2FFFDEDCDFF2A6D25FF0F710EFF10710EFF0F710EFF0F71
        0EFF0F710EFF0F700EFF0B9808FF05A900FF0AA606FF567F4AFFEDE0C3FFFDED
        CCFFFDE8C5FF6C4F2A9800000000000000000000000000000000000000000000
        0000BD9B60DCFFEBCBFFFFF3D7FF1C8B16FF05AD00FF05AD00FF05AD00FF05AD
        00FF05AD00FF05AD00FF05AD00FF05AD00FF05AD00FF06AE02FF30832BFFCFC6
        B0FFFFEDCEFF6D502C9800000000000000000000000000000000000000000000
        0000BD9C61DCFFEED0FFFFF6DCFF1E8C19FF05B200FF05B200FF05B200FF05B2
        00FF05B200FF05B200FF05B200FF05B200FF05B200FF05B200FF04B300FF639F
        59FFFFF1D4FF6D512D9800000000000000000000000000000000000000000000
        0000BD9E63DCFFF1D4FFFFF8E1FF1F8F1AFF06B800FF06B800FF06B800FF06B8
        00FF06B800FF06B800FF06B800FF06B800FF06B800FF06B800FF4AB644FFFDF6
        E0FFFFF3D8FF6D522D9800000000000000000000000000000000000000000000
        0000BD9E65DCFFF2D8FFFFF9E4FF1E8A1AFF07AC03FF07AC03FF07AC03FF07AC
        03FF07AC03FF07AB03FF06B700FF06BB00FF06BC00FF56C050FFF2F1DBFFFFF9
        E3FFFFF4DBFF6D522E9800000000000000000000000000000000000000000000
        0000BD9E66DCFFF3DCFFFFFAE6FFEFECDCFFEEEBDBFFEEEBDBFFEEEBDBFFEFEB
        DCFFEFEBDCFFEEEADDFF128E0EFF07BF02FF72C76BFFFEFBE9FFFFFBE9FFFFFA
        E7FFFFF5DFFF6D522E9800000000000000000000000000000000000000000000
        0000BD9F68DCFFF5DFFFFFFBEAFFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFD
        EDFFFFFDEDFFFEFCEEFF188C15FFA0D498FFFFFDEDFFFFFDEDFFFFFDEDFFFFFC
        EBFFFFF7E3FF6D522F9800000000000000000000000000000000000000000000
        0000BD9F68DCFFF5E2FFFFFBEDFFFFFDF0FFFFFDF0FFFFFDF0FFFFFDF0FFFFFD
        F0FFFFFDF0FFFFFDF0FFE2E8D7FFFEFDEFFFFDFBEEFFFDFBEEFFFEFCEFFFFEFB
        EDFFFDF6E3FF6550339800000001000000000000000000000000000000000000
        0000BDA069DCFFF6E4FFFFFCEFFFFFFEF3FFFFFEF3FFFFFEF3FFFFFEF3FFFFFE
        F3FFFFFEF3FFFFFEF3FFFEFDF2FFFBFAEDFFF6F3E6FFF3F0E2FFF5F2E4FFF7F2
        E3FFF7EEDBFF644F329903020107010100020000000000000000000000000000
        0000BDA06ADCFFF6E7FFFFFCF1FFFFFEF5FFFFFEF5FFFFFEF5FFFFFEF5FFFFFE
        F5FFFFFEF5FFFFFEF5FFFDFBF2FFF4F0E4FFE4DDCCFFD9CFBCFFDAD1BDFFE0D6
        C3FFE7D9C4FF624D2F9B0604020D010100020000000000000000000000000000
        0000BDA06BDCFFF6EAFFFFFCF4FFFFFEF8FFFFFEF8FFFFFEF8FFFFFEF8FFFFFE
        F8FFFFFEF8FFFFFEF8FFFBF9F2FFEBE5D9FFD1BE9EFFBDA882FFBBAA8EFFC9B7
        9DFFD9C2A3FF3B2D16660503010A000000010000000000000000000000000000
        0000BDA06BDCFFF6EAFFFFFCF5FFFFFEF9FFFFFEF9FFFFFEF9FFFFFEF9FFFFFE
        F9FFFFFEF9FFFFFEF9FFFAF7F2FFE6E0D2FFD7BA8EFFF0C793FFD6B683FFD6B7
        86FF553F178C0A07031502020105000000000000000000000000000000000000
        0000BD9F6BDCFFF3E9FFFFF9F3FFFFFBF7FFFFFBF7FFFFFBF7FFFFFBF7FFFFFB
        F7FFFFFBF7FFFFFBF7FFFAF6F0FFE9E1D6FFE3C696FFFFE9C9FFFEE8C8FF6D55
        2DA00C09041A0302010700000000000000000000000000000000000000000000
        0000BD9D68DCFFEEE0FFFFF3E8FFFFF5EBFFFFF5EBFFFFF5EBFFFFF5EBFFFFF5
        EBFFFFF5EBFFFFF5EBFFFCF2E7FFF2E6D9FFE5C99FFFFEE9D0FF856E45B30F0B
        0520040301090000000100000000000000000000000000000000000000000000
        00006C542989846E489D846F499D846F499D846F499D846F499D846F499D846F
        499D846F499D846F499D846F499D7A67459E7059339F705B359B0B0804180403
        0109000000010000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000010100020201010403020106020101040101
        00020000000000000000000000000000000000000000}
      TabOrder = 1
      OnClick = btnGenerareClick
    end
  end
  object DTStock: TDataSource
    DataSet = MemStock
    Left = 23
    Top = 104
  end
  object MemStock: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 57
    Top = 104
  end
  object QryStock: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryStockAfterOpen
    SQL.Strings = (
      'exec spStockCodBare :TIP_STOC, NULL, :CU_MISCARI, :CONT'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_STOC'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_MISCARI'
        ParamType = ptUnknown
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'CONT'
        ParamType = ptUnknown
        Size = 100
      end>
    Left = 24
    Top = 200
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_STOC'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_MISCARI'
        ParamType = ptUnknown
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'CONT'
        ParamType = ptUnknown
        Size = 100
      end>
  end
  object popupGrid: TcxGridPopupMenu
    Grid = grStock
    PopupMenus = <
      item
        GridView = GridStock
        HitTypes = [gvhtCell, gvhtRecord]
        Index = 0
      end>
    Left = 56
    Top = 201
  end
end
