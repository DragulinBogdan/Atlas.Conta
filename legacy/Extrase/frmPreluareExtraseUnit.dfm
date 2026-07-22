object frmPreluareExtrase: TfrmPreluareExtrase
  Left = 290
  Top = 155
  Caption = 'Preluare Extrase'
  ClientHeight = 642
  ClientWidth = 920
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  PixelsPerInch = 96
  TextHeight = 13
  object grHeader: TcxGroupBox
    Left = 0
    Top = 0
    Align = alTop
    Caption = 'Fisierul din care se realizeaza preluarea '
    TabOrder = 0
    DesignSize = (
      920
      73)
    Height = 73
    Width = 920
    object edNumeFisier: TcxButtonEdit
      Left = 82
      Top = 16
      Anchors = [akLeft, akTop, akRight]
      Properties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      Properties.OnButtonClick = edNumeFisierPropertiesButtonClick
      TabOrder = 0
      Width = 739
    end
    object lbNumeFisier: TcxLabel
      Left = 12
      Top = 18
      Caption = 'Nume Fisier :'
    end
    object btnPreia: TcxButton
      Left = 828
      Top = 15
      Width = 84
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Citeste'
      TabOrder = 2
      OnClick = btnPreiaClick
    end
    object lbProgresProcesare: TcxLabel
      Left = 12
      Top = 45
      Caption = 'Progres Procesare : '
    end
    object progresProcesare: TcxProgressBar
      Left = 113
      Top = 45
      TabOrder = 4
      Width = 200
    end
    object lbProgresTransfer: TcxLabel
      Left = 605
      Top = 45
      Caption = 'Progres Transfer : '
    end
    object progresTransfer: TcxProgressBar
      Left = 706
      Top = 45
      TabOrder = 6
      Width = 200
    end
    object lbProgresAnaliza: TcxLabel
      Left = 316
      Top = 45
      Caption = 'Progres Analiza : '
    end
    object progresAnaliza: TcxProgressBar
      Left = 403
      Top = 45
      TabOrder = 8
      Width = 200
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 552
    Width = 920
    Height = 90
    Align = alBottom
    TabOrder = 1
    DesignSize = (
      920
      90)
    object BtnOk: TcxButton
      Left = 829
      Top = 6
      Width = 83
      Height = 30
      Anchors = [akRight, akBottom]
      Caption = 'Ok'
      LookAndFeel.Kind = lfOffice11
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
      TabOrder = 0
      OnClick = BtnOkClick
    end
  end
  object pagePreluare: TcxPageControl
    Left = 0
    Top = 73
    Width = 920
    Height = 479
    Align = alClient
    TabOrder = 2
    Properties.ActivePage = tabListaExtras
    Properties.CustomButtons.Buttons = <>
    ExplicitHeight = 534
    ClientRectBottom = 479
    ClientRectRight = 920
    ClientRectTop = 24
    object tabContinut: TcxTabSheet
      Caption = 'Continut Extras'
      ImageIndex = 0
      ExplicitHeight = 510
      object prelInfo: TcxMemo
        Left = 0
        Top = 0
        Align = alClient
        ParentFont = False
        Properties.ScrollBars = ssVertical
        Properties.WordWrap = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'Courier New'
        Style.Font.Style = []
        Style.IsFontAssigned = True
        TabOrder = 0
        ExplicitHeight = 510
        Height = 455
        Width = 920
      end
    end
    object tabListaExtras: TcxTabSheet
      Caption = 'Lista Extras'
      ImageIndex = 1
      ExplicitHeight = 510
      object gridExtras: TcxGrid
        Left = 0
        Top = 0
        Width = 920
        Height = 455
        Align = alClient
        TabOrder = 0
        ExplicitHeight = 510
        object viewExtras: TcxGridDBBandedTableView
          Navigator.Buttons.CustomButtons = <>
          Navigator.Visible = True
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = dtExtras
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.DeletingConfirmation = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          OptionsView.BandHeaders = False
          Preview.Column = viewExtrasexplicatii
          Preview.Visible = True
          Bands = <
            item
            end>
          object viewExtrasRecId: TcxGridDBBandedColumn
            DataBinding.FieldName = 'RecId'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 0
            Position.RowIndex = 0
          end
          object viewExtrascontClient: TcxGridDBBandedColumn
            DataBinding.FieldName = 'contClient'
            HeaderAlignmentHorz = taCenter
            Width = 71
            Position.BandIndex = 0
            Position.ColIndex = 1
            Position.RowIndex = 0
          end
          object viewExtrasnumeContClient: TcxGridDBBandedColumn
            DataBinding.FieldName = 'numeContClient'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 81
            Position.BandIndex = 0
            Position.ColIndex = 2
            Position.RowIndex = 0
          end
          object viewExtrascontCrsp: TcxGridDBBandedColumn
            DataBinding.FieldName = 'contCrsp'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 48
            Position.BandIndex = 0
            Position.ColIndex = 3
            Position.RowIndex = 0
          end
          object viewExtrasnrDoc: TcxGridDBBandedColumn
            DataBinding.FieldName = 'nrDoc'
            HeaderAlignmentHorz = taCenter
            Width = 33
            Position.BandIndex = 0
            Position.ColIndex = 4
            Position.RowIndex = 0
          end
          object viewExtrasdataDoc: TcxGridDBBandedColumn
            DataBinding.FieldName = 'dataDoc'
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 5
            Position.RowIndex = 0
          end
          object viewExtrasdataPlata: TcxGridDBBandedColumn
            DataBinding.FieldName = 'dataPlata'
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 6
            Position.RowIndex = 0
          end
          object viewExtrascuiClient: TcxGridDBBandedColumn
            DataBinding.FieldName = 'cuiClient'
            HeaderAlignmentHorz = taCenter
            Width = 45
            Position.BandIndex = 0
            Position.ColIndex = 7
            Position.RowIndex = 0
          end
          object viewExtrascuiPlatitor: TcxGridDBBandedColumn
            DataBinding.FieldName = 'cuiPlatitor'
            HeaderAlignmentHorz = taCenter
            Width = 52
            Position.BandIndex = 0
            Position.ColIndex = 8
            Position.RowIndex = 0
          end
          object viewExtrascuiBeneficiar: TcxGridDBBandedColumn
            DataBinding.FieldName = 'cuiBeneficiar'
            HeaderAlignmentHorz = taCenter
            Width = 82
            Position.BandIndex = 0
            Position.ColIndex = 9
            Position.RowIndex = 0
          end
          object viewExtrassumaDebit: TcxGridDBBandedColumn
            DataBinding.FieldName = 'sumaDebit'
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 10
            Position.RowIndex = 0
          end
          object viewExtrassumaCredit: TcxGridDBBandedColumn
            DataBinding.FieldName = 'sumaCredit'
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 11
            Position.RowIndex = 0
          end
          object viewExtrasibanClient: TcxGridDBBandedColumn
            DataBinding.FieldName = 'ibanClient'
            HeaderAlignmentHorz = taCenter
            Width = 52
            Position.BandIndex = 0
            Position.ColIndex = 12
            Position.RowIndex = 0
          end
          object viewExtrasibanPlatitor: TcxGridDBBandedColumn
            DataBinding.FieldName = 'ibanPlatitor'
            HeaderAlignmentHorz = taCenter
            Width = 59
            Position.BandIndex = 0
            Position.ColIndex = 13
            Position.RowIndex = 0
          end
          object viewExtrasibanBeneficiar: TcxGridDBBandedColumn
            DataBinding.FieldName = 'ibanBeneficiar'
            HeaderAlignmentHorz = taCenter
            Width = 72
            Position.BandIndex = 0
            Position.ColIndex = 14
            Position.RowIndex = 0
          end
          object viewExtrasnumePlatitor: TcxGridDBBandedColumn
            DataBinding.FieldName = 'numePlatitor'
            HeaderAlignmentHorz = taCenter
            Width = 65
            Position.BandIndex = 0
            Position.ColIndex = 15
            Position.RowIndex = 0
          end
          object viewExtrasnumeBeneficiar: TcxGridDBBandedColumn
            DataBinding.FieldName = 'numeBeneficiar'
            HeaderAlignmentHorz = taCenter
            Width = 78
            Position.BandIndex = 0
            Position.ColIndex = 16
            Position.RowIndex = 0
          end
          object viewExtrasbicBancaDest: TcxGridDBBandedColumn
            DataBinding.FieldName = 'bicBancaDest'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 17
            Position.RowIndex = 0
          end
          object viewExtrasbicBancaExt: TcxGridDBBandedColumn
            DataBinding.FieldName = 'bicBancaExt'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Position.BandIndex = 0
            Position.ColIndex = 18
            Position.RowIndex = 0
          end
          object viewExtrasexplicatii: TcxGridDBBandedColumn
            DataBinding.FieldName = 'explicatii'
            HeaderAlignmentHorz = taCenter
            Width = 46
            Position.BandIndex = 0
            Position.ColIndex = 19
            Position.RowIndex = 0
          end
          object viewExtrasData: TcxGridDBBandedColumn
            DataBinding.FieldName = 'Data'
            HeaderAlignmentHorz = taCenter
            Width = 28
            Position.BandIndex = 0
            Position.ColIndex = 20
            Position.RowIndex = 0
          end
        end
        object nivelExtras: TcxGridLevel
          GridView = viewExtras
        end
      end
    end
  end
  object extrasDialog: TOpenDialog
    DefaultExt = '*.xml'
    Filter = 
      'Fisiere Extras XML|*.xml|Fisiere Extras TXT|*.txt|Fisiere Extras' +
      ' PDF|*.pdf|Toate Fisierele|*.*'
    Title = 'Selectati Fisierul de Extras'
    Left = 232
    Top = 200
  end
  object tblExtras: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 296
    Top = 200
    object tblExtrascontClient: TStringField
      FieldName = 'contClient'
      Size = 128
    end
    object tblExtrasnumeContClient: TStringField
      FieldName = 'numeContClient'
      Size = 128
    end
    object tblExtrascontCrsp: TStringField
      FieldName = 'contCrsp'
      Size = 64
    end
    object tblExtrasnrDoc: TStringField
      FieldName = 'nrDoc'
      Size = 64
    end
    object tblExtrasdataDoc: TDateField
      FieldName = 'dataDoc'
    end
    object tblExtrasdataPlata: TDateField
      FieldName = 'dataPlata'
    end
    object tblExtrascuiClient: TStringField
      FieldName = 'cuiClient'
      Size = 64
    end
    object tblExtrascuiPlatitor: TStringField
      FieldName = 'cuiPlatitor'
      Size = 64
    end
    object tblExtrascuiBeneficiar: TStringField
      FieldName = 'cuiBeneficiar'
      Size = 64
    end
    object tblExtrassumaDebit: TCurrencyField
      FieldName = 'sumaDebit'
    end
    object tblExtrassumaCredit: TCurrencyField
      FieldName = 'sumaCredit'
    end
    object tblExtrasibanClient: TStringField
      FieldName = 'ibanClient'
      Size = 64
    end
    object tblExtrasibanPlatitor: TStringField
      FieldName = 'ibanPlatitor'
      Size = 64
    end
    object tblExtrasibanBeneficiar: TStringField
      FieldName = 'ibanBeneficiar'
      Size = 64
    end
    object tblExtrasnumePlatitor: TStringField
      FieldName = 'numePlatitor'
      Size = 128
    end
    object tblExtrasnumeBeneficiar: TStringField
      FieldName = 'numeBeneficiar'
      Size = 128
    end
    object tblExtrasbicBancaDest: TStringField
      FieldName = 'bicBancaDest'
      Size = 128
    end
    object tblExtrasbicBancaExt: TStringField
      FieldName = 'bicBancaExt'
      Size = 128
    end
    object tblExtrasexplicatii: TStringField
      FieldName = 'explicatii'
      Size = 256
    end
    object tblExtrasData: TDateField
      FieldName = 'Data'
    end
  end
  object dtExtras: TDataSource
    DataSet = tblExtras
    Left = 264
    Top = 200
  end
  object popupGrid: TcxGridPopupMenu
    Grid = gridExtras
    PopupMenus = <>
    Left = 528
    Top = 320
  end
end
