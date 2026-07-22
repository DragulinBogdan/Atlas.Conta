object frmIntretinereTipMat: TfrmIntretinereTipMat
  Left = 277
  Top = 79
  Caption = 'Intretinere nomenclator Tip Materiale'
  ClientHeight = 600
  ClientWidth = 889
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
  object pnBottom: TPanel
    Left = 0
    Top = 512
    Width = 889
    Height = 88
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    OnResize = pnBottomResize
    DesignSize = (
      889
      88)
    object btnOk: TcxButton
      Left = 794
      Top = 6
      Width = 75
      Height = 25
      Hint = 'Inchide Ecranul'
      Anchors = [akRight, akBottom]
      Caption = '&Inchide'
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
      OnClick = btnOkClick
    end
    object chkAllTipProdus: TcxCheckBox
      Left = -1
      Top = -2
      Hint = 'Se vor afisa toate produsele sau numai produsele selectate'
      Caption = 'Toate Produsele'
      ParentBackground = False
      ParentColor = False
      Style.BorderStyle = ebsNone
      Style.Color = clGray
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 1
      OnClick = chkAllTipProdusClick
    end
  end
  object pnContent: TPanel
    Left = 201
    Top = 41
    Width = 688
    Height = 471
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 530
    object cxGroupBox: TcxGroupBox
      Left = 386
      Top = 0
      Align = alRight
      ParentBackground = False
      ParentColor = False
      Style.BorderColor = clMenuHighlight
      Style.BorderStyle = ebsThick
      Style.Color = clWindow
      Style.Shadow = False
      TabOrder = 0
      ExplicitHeight = 530
      Height = 471
      Width = 302
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
        Top = 75
        Width = 55
        Height = 13
        Hint = 'Descrierea tipului de material'
        Caption = 'Descriere'
        FocusControl = edtDescriere
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label3: TLabel
        Left = 12
        Top = 8
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
      object Label4: TLabel
        Left = 12
        Top = 140
        Width = 82
        Height = 13
        Hint = 'Codul Economic'
        Caption = 'Cod Economic'
        FocusControl = edtCodEconomic
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label6: TLabel
        Left = 12
        Top = 43
        Width = 27
        Height = 13
        Hint = 'Contul la care se refera tipul de material/ tipul de chelltuiala'
        Caption = 'Cont'
        FocusControl = edtCont
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label8: TLabel
        Left = 12
        Top = 173
        Width = 62
        Height = 13
        Hint = 'Tipul de produs asociat'
        Caption = 'Tip Produs'
        FocusControl = edtTipProdus
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        OnDblClick = Label8DblClick
      end
      object Label10: TLabel
        Left = 12
        Top = 267
        Width = 79
        Height = 13
        Hint = 
          'Contul de intrare general - va fi folosit numai pentru valoarea ' +
          'implicita in cazul definirii notei contabile'
        Caption = 'Cont de iesire'
        FocusControl = edtContIesire
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label9: TLabel
        Left = 12
        Top = 233
        Width = 85
        Height = 13
        Hint = 
          'Contul de intrare general - va fi folosit numai pentru valoarea ' +
          'implicita in cazul definirii notei contabile'
        Caption = 'Cont de intrare'
        FocusControl = edtContIntrare
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 9
        Top = 223
        Width = 273
        Height = 9
        Shape = bsBottomLine
      end
      object edtDenumire: TcxDBTextEdit
        Left = 22
        Top = 120
        Hint = 'Denumirea tipului de material'
        DataBinding.DataField = 'DENUMIRE'
        DataBinding.DataSource = DTTipMaterial
        Style.Color = 12910591
        TabOrder = 0
        Width = 250
      end
      object edtDescriere: TcxDBTextEdit
        Left = 22
        Top = 88
        Hint = 'Descrierea tipului de material'
        DataBinding.DataField = 'DESCRIERE'
        DataBinding.DataSource = DTTipMaterial
        Style.Color = 12910591
        TabOrder = 1
        Width = 250
      end
      object edtIdGestTipMaterial: TcxDBTextEdit
        Left = 22
        Top = 21
        Hint = 'Identificatorul din nomenclator'
        DataBinding.DataField = 'ID_GEST_TIP_MATERIAL'
        DataBinding.DataSource = DTTipMaterial
        Properties.Alignment.Horz = taLeftJustify
        Properties.ReadOnly = True
        Style.Color = clSilver
        TabOrder = 2
        Width = 250
      end
      object edtCodEconomic: TcxDBButtonEdit
        Left = 22
        Top = 153
        Hint = 'Codul Economic'
        DataBinding.DataField = 'COD_ECONOMIC'
        DataBinding.DataSource = DTTipMaterial
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.OnButtonClick = edtCodEconomicPropertiesButtonClick
        Style.Color = 12910591
        TabOrder = 3
        Width = 250
      end
      object edtCont: TcxDBButtonEdit
        Left = 22
        Top = 56
        Hint = 'Contul la care se refera tipul de material/ tipul de chelltuiala'
        DataBinding.DataField = 'CONT'
        DataBinding.DataSource = DTTipMaterial
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.OnButtonClick = edtContIntrarePropertiesButtonClick
        Style.Color = 12910591
        TabOrder = 4
        Width = 250
      end
      object edtTipProdus: TcxDBImageComboBox
        Left = 22
        Top = 186
        Hint = 'Tipul de produs asociat'
        AutoSize = False
        DataBinding.DataField = 'ID_GEST_TIP_PRODUSE'
        DataBinding.DataSource = DTTipMaterial
        Properties.Alignment.Horz = taCenter
        Properties.Items = <
          item
            ImageIndex = 0
          end
          item
            Value = '1'
          end
          item
            Value = '2'
          end>
        Style.Color = 12910591
        TabOrder = 5
        Height = 21
        Width = 250
      end
      object edtContIesire: TcxDBButtonEdit
        Left = 22
        Top = 282
        Hint = 
          'Contul de intrare general - va fi folosit numai pentru valoarea ' +
          'implicita in cazul definirii notei contabile'
        DataBinding.DataField = 'CONT_IESIRE'
        DataBinding.DataSource = DTTipMaterial
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.OnButtonClick = edtContIntrarePropertiesButtonClick
        Style.Color = 12910591
        TabOrder = 6
        Width = 250
      end
      object edtSeAfiseaza: TcxDBCheckBox
        Left = 12
        Top = 206
        Hint = 'Daca acest tip de material va fi afisat la culegerea din TCV'
        Caption = 'Se Afiseaza'
        DataBinding.DataField = 'SE_AFISEAZA'
        DataBinding.DataSource = DTTipMaterial
        ParentFont = False
        Properties.NullStyle = nssUnchecked
        Properties.ValueGrayed = 'False'
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        TabOrder = 8
      end
      object edtContIntrare: TcxDBButtonEdit
        Left = 22
        Top = 246
        Hint = 
          'Contul de intrare general - va fi folosit numai pentru valoarea ' +
          'implicita in cazul definirii notei contabile'
        DataBinding.DataField = 'CONT_INTRARE'
        DataBinding.DataSource = DTTipMaterial
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.OnButtonClick = edtContIntrarePropertiesButtonClick
        Style.Color = 12910591
        TabOrder = 7
        Width = 250
      end
    end
    object Panel1: TPanel
      Left = 0
      Top = 0
      Width = 386
      Height = 471
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 1
      ExplicitHeight = 530
      object pnControl: TPanel
        Left = 0
        Top = 439
        Width = 386
        Height = 32
        Align = alBottom
        BevelOuter = bvNone
        TabOrder = 1
        ExplicitTop = 498
        object btnAddTipMat: TcxButton
          Left = 12
          Top = 3
          Width = 33
          Height = 25
          Hint = 'Adauga Tip Material'
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
          OnClick = btnAddTipMatClick
        end
        object btnDelTipMat: TcxButton
          Left = 50
          Top = 2
          Width = 33
          Height = 25
          Hint = 'Sterge Tip Material'
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
          OnClick = btnDelTipMatClick
        end
        object btnImportPlan: TcxButton
          Left = 90
          Top = 2
          Width = 33
          Height = 25
          Hint = 'Adauga prin selectie multipla din planul de conturi'
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360900000000000036000000280000001800000018000000010020000000
            000000000000C40E0000C40E0000000000000000000000000000000000000000
            0000000000000000000000000000000000000000000000000000000000000101
            01020E0D0B14221E1B30352F2A4B2F3D286A104E159B0D5D14BE266626C44276
            3CD7506742C20103010600000000000000000000000000000000000000000000
            00000F0D0C1524201C3438322C504A41396B5C5147886F6255A5877768CAA08A
            7AF1A8917FFFA3967CFF3B8637FF1A8F30FF1CA84AFF17B956FF19B957FF22AB
            50FF1E9236FF0D6416CA02120425000000000000000000000000000000000000
            0000827264C6927D6CFF8E7A6CFF978477FFA19083FFAC9C90FFBBAAA0FFCAB9
            B0FFB2AB9CFF217D26FF35A959FF37C875FF1FC265FF5EB783FF5CB180FF2DBD
            6BFF41C87BFF40AF64FF10741AEB020D031B0000000000000000000000000000
            0000716459ABEFDDD6FFFBE7DFFFFCE7DFFFFAE4DCFFF7E2DAFFF6E0D8FFF1DC
            D4FF589856FF3BA357FF55C987FF3BC878FF24BC66FFF6F7F7FFE3E1E2FF39B2
            6DFF43C87CFF5BC98AFF4CAF6AFF0D6316C90000000000000000000000000000
            00005B504799FDE8E0FFFBE5DDFFF9E3DBFFF7E2DAFFF6E0D8FFF4DED6FFDED3
            C5FF1F882DFF6ECA95FF59C989FF43C87CFF31BD6DFFFAFBFAFFE9E7E8FF43B3
            73FF4AC980FF5CC98BFF6ECA96FF20892FFF041D063A00000000000000000000
            0000544A4189F8E3DAFFF8E2DBFFF7E1D9FFF5DFD7FFF3DDD5FFF1DBD3FFA2BA
            93FF3D9E53FF70C595FF78BB94FF6BB88CFF65B386FFF8F8F8FFEBEAEBFF6FAD
            8AFF6FB88FFF76B892FF71C093FF46A963FF09450F8C00000000000000000000
            0000463E3779F4DFD7FFF7E1D9FFF5DFD7FFF2DDD5FFF0DAD2FFEED8D0FF86AD
            7CFF44A65EFF73C295FFF4F4F4FFEEEEEEFFEFEFEFFFFDFDFDFFFDFDFDFFF0F0
            F0FFEEEEEEFFE3E3E3FF7FB897FF55B776FF0E6A18D700000000000000000000
            000037312B68F0DAD2FFF4DED6FFF2DCD4FFF0DAD2FFEDD7CFFFEBD6CEFF87AD
            7CFF3EA559FF6AC290FFDFEAE4FFE0EAE4FFE0E9E4FFFEFEFEFFFDFDFDFFE3EA
            E7FFE1EAE5FFDDE8E2FF7FC19CFF51B774FF0E6A17D600000000000000000000
            0000332E2857E9D4CBFFF1DBD3FFEFD9D1FFECD6CEFFEAD4CCFFE8D2CAFFAAB9
            97FF2F9946FF5AC989FF5DC88BFF6DC994FF77BF95FFFAFBFBFFEAEAEAFF7EB5
            96FF7AC99CFF6AC893FF61C98EFF3DA65BFF09400E8200000000000000000000
            000018151348E5D0C6FFEED8D0FFEBD5CDFFEAD4CCFFE7D1C9FFE5CFC7FFDCCA
            BFFF1F852AFF57C484FF64CA8FFF7CCB9EFF8AC1A1FFFBFBFBFFE8E7E7FF8BB6
            9DFF87CBA4FF72CA98FF64CA90FF1B872AFF0318053100000000000000000000
            000016131137DEC9C0FFEAD5CDFFE8D2CAFFE6D0C8FFE3CDC5FFE1CBC3FFDFC9
            C1FF77A26DFF28943EFF66CA91FF84CBA2FF96C2A9FFF1F2F1FFEAEBEAFF94BA
            A5FF8BCCA7FF73CA98FF3CA559FF0C5814B20000000000000000000000000000
            000008080728D9C4BAFFE7D1C9FFE5CFC7FFE2CCC4FFE0CAC2FFDEC8C0FFDBC5
            BDFFD6C2B9FF4E924BFF2B9743FF6EC692FF96CCADFFA6C2B2FFA2C2AFFF96CA
            ADFF77CA9BFF3AA457FF0E6A17D9010802110000000000000000000000000000
            000004040316D7C2B7FFE3CEC6FFE1CBC3FFDFC9C1FFDDC7BFFFDAC4BCFFD8C2
            BBFFD6C0B8FFD2BDB4FF6D9C64FF178224FF2E9B48FF46B068FF48B16AFF34A2
            52FF178527FF0E5715AF01080210000000000000000000000000000000000000
            000002020209CEB9AEFCE0CAC2FFDDC7BFFFDBC5BDFFD8C2BBFFD6C0B8FFD4BE
            B6FFD2BCB5FFD0BAB2FFCEB8B0FFBAB1A1FF829F75FF64965CFF61945AFF7296
            67FF919477FF1F221A3800000000000000000000000000000000000000000000
            000000000002C2AEA4F3DCC6BEFFD9C3BCFFD8C2BAFFD5BFB7FFD3BDB5FFD1BB
            B3FFCEB8B0FFCCB6AFFFCBB5AEFFC9B3ABFFC8B2AAFFC6B0A8FFC4AEA6FFC0AB
            A3FFAB9A89FF241F1B4000000000000000000000000000000000000000000000
            000000000000B09F93E6D8C2BBFFD6C0B8FFD4BEB6FFD2BCB4FFD0BAB2FFCDB7
            B0FFCBB5ADFFCAB4ACFFC8B2AAFFC6B0A8FFC5AFA7FFC3ADA5FFC1ABA4FFBFAA
            A2FFAF9F8EFF2F2A254800000000000000000000000000000000000000000000
            000000000000988A7FD5D4BEB6FFD2BCB4FFD0BAB3FFCEB8B0FFCCB6AEFFCAB4
            ACFFC8B2ABFFC6B1A9FFC5AFA7FFC3ADA5FFC1ACA4FFC0ABA3FFBFA9A1FFBEA8
            A0FFB2A291FF332E294F00000000000000000000000000000000000000000000
            000000000000887B71C4C6AEA7FFCAB4ACFFCAB5ADFFCAB5ADFFC9B3ABFFC7B1
            A9FFC5AFA7FFC4AEA6FFC2ACA5FFC0ABA3FFC0AAA2FFBEA8A0FFBDA79FFFBEA7
            A0FFAD9C8DFF3A332E5800000000000000000000000000000000000000000000
            00000000000070655BB293624FFF89533DFF8B5642FF8E5B47FF92614EFF9768
            55FF9B6E5CFF9E7361FFA07767FFA27C6CFFA68172FFA78476FFAB8B7FFFB69A
            90FF9F8C7FFF3C352F5E00000000000000000000000000000000000000000000
            000000000000685D549DB39180FF9B654BFF9C664CFF9C664DFF9D684DFF9F69
            4FFFA06A4FFFA16B50FFA26C51FFA36D52FFA36E53FFA46F53FFA57055FFAF8D
            7EFFAA9789FF413B346700000000000000000000000000000000000000000000
            00000000000062574D86CBB19DFFBE9B87FFB9866CFFB88165FFB68063FFB57F
            63FFB47E61FFB17B5FFFB07A5EFFB0795DFFAF795DFFB07A5FFFB18E7DFFBDA8
            9FFFBAA696FF473F386E00000000000000000000000000000000000000000000
            0000000000001D1A1729544C43726A5E538B7A6C60A49D7F6EC9D7A184FFD59D
            81FFD19A7EFFCF987CFFCD967AFFC7957BFFC19883FFC2A695FFCDB5A2FFDAC0
            ACFFE3CAB5FF4D453D7800000000000000000000000000000000000000000000
            0000000000000000000000000000000000000000000000000000483E3A5EDBA7
            8BFADFA88BFFD7A084FF64544C86433C355E5F544B817F7164A59E8C7DC3BBA6
            94DED8C2ACF84D453D7500000000000000000000000000000000000000000000
            0000000000000000000000000000000000000000000000000000000000001D1A
            19287E675EAD60514C8301010101000000000000000000000000000000000000
            00000101010201010101000000000000000000000000}
          TabOrder = 2
          OnClick = btnImportPlanClick
        end
      end
      object TreeTipMat: TcxDBTreeList
        Left = 0
        Top = 0
        Width = 386
        Height = 439
        Align = alClient
        Bands = <
          item
            Caption.Text = 'Band1'
            Width = 680
          end>
        DataController.DataSource = DTTipMaterial
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID_GEST_TIP_MATERIAL'
        DragCursor = crDrag
        DragMode = dmAutomatic
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.FocusCellOnCycle = True
        OptionsBehavior.IncSearch = True
        OptionsData.Editing = False
        OptionsData.AnsiSort = True
        OptionsData.CaseInsensitive = True
        OptionsData.Deleting = False
        OptionsData.SummaryNullIgnore = True
        OptionsSelection.MultiSelect = True
        OptionsView.ColumnAutoWidth = True
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        StateImages = CheckList
        Styles.StyleSheet = TreeListStyleSheetUserFormat4
        TabOrder = 0
        OnDragOver = TreeTipMatDragOver
        ExplicitHeight = 498
        object TreeTipMatDENUMIRE: TcxDBTreeListColumn
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'DENUMIRE'
          Options.Editing = False
          Width = 204
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatDESCRIERE: TcxDBTreeListColumn
          Caption.Text = 'Descriere'
          DataBinding.FieldName = 'DESCRIERE'
          Options.Editing = False
          Width = 183
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatID_GEST_TIP_MATERIAL: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Identificator'
          DataBinding.FieldName = 'ID_GEST_TIP_MATERIAL'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatCOD_ECONOMIC: TcxDBTreeListColumn
          Caption.Text = 'Cod Economic'
          DataBinding.FieldName = 'COD_ECONOMIC'
          Options.Editing = False
          Width = 142
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatMAN_ID: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'MAN_ID'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatCONT: TcxDBTreeListColumn
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT'
          Options.Editing = False
          Width = 63
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatID_PARINTE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id Parinte'
          DataBinding.FieldName = 'ID_PARINTE'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatID_GEST_TIP_PRODUSE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <>
          Caption.Text = 'Tip Produs'
          DataBinding.FieldName = 'ID_GEST_TIP_PRODUSE'
          Options.Editing = False
          Width = 88
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatCONT_INTRARE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cont Intrare'
          DataBinding.FieldName = 'CONT_INTRARE'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatCONT_IESIRE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cont Iesire'
          DataBinding.FieldName = 'CONT_IESIRE'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMatSE_AFISEAZA: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Se Afiseaza'
          DataBinding.FieldName = 'SE_AFISEAZA'
          Options.Editing = False
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
  object pnLeft: TPanel
    Left = 0
    Top = 41
    Width = 193
    Height = 471
    Align = alLeft
    BevelOuter = bvNone
    TabOrder = 2
    ExplicitHeight = 530
    object TreeTipProdus: TcxTreeList
      Left = 0
      Top = 0
      Width = 193
      Height = 471
      Align = alClient
      Bands = <
        item
          Caption.Text = 'Band1'
          Width = 400
        end>
      Images = CheckList
      Navigator.Buttons.CustomButtons = <>
      OptionsBehavior.ImmediateEditor = False
      OptionsData.Editing = False
      OptionsData.AnsiSort = True
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.PaintStyle = tlpsCategorized
      OptionsView.ShowRoot = False
      ScrollbarAnnotations.CustomAnnotations = <>
      Styles.StyleSheet = TreeListStyleSheetStormVGA
      TabOrder = 0
      OnCustomDrawDataCell = TreeTipProdusCustomDrawDataCell
      OnGetNodeImageIndex = TreeTipProdusGetNodeImageIndex
      OnMouseUp = TreeTipProdusMouseUp
      ExplicitHeight = 530
      object TreeTipProdusDENUMIRE: TcxTreeListColumn
        PropertiesClassName = 'TcxTextEditProperties'
        Caption.Text = 'Denumire'
        Width = 191
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreeTipProdusID_GEST_TIP_PRODUSE: TcxTreeListColumn
        PropertiesClassName = 'TcxSpinEditProperties'
        Visible = False
        DataBinding.ValueType = 'Integer'
        Width = 100
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreeTipProdusTIP_PRODUS: TcxTreeListColumn
        PropertiesClassName = 'TcxTextEditProperties'
        Visible = False
        Width = 100
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreeTipProdusSE_AFISEAZA: TcxTreeListColumn
        PropertiesClassName = 'TcxCheckBoxProperties'
        Visible = False
        DataBinding.ValueType = 'Boolean'
        Width = 100
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
  end
  object NetscapeSplitter1: TcxSplitter
    Left = 193
    Top = 41
    Width = 8
    Height = 471
    Cursor = crHSplit
    HotZoneClassName = 'TcxMediaPlayer9Style'
    AutoSnap = True
    MinSize = 1
    Control = pnLeft
    ExplicitHeight = 530
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 889
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Intretinere nomenclator Tip Materiale'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 4
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object DTProdus: TDataSource
    DataSet = QryTipProdus
    Left = 240
    Top = 152
  end
  object QryTipProdus: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_TIP_PRODUSE')
    Params = <>
    Left = 272
    Top = 152
  end
  object DTTipMaterial: TDataSource
    DataSet = QryTipMaterial
    Left = 240
    Top = 184
  end
  object QryTipMaterial: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_TIP_MATERIAL')
    Params = <>
    Left = 272
    Top = 184
  end
  object cxStyleRepository1: TcxStyleRepository
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svColor]
      Color = 14671839
    end
    object cxStyle2: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11504771
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle3: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11504771
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle4: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle5: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle6: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle7: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11504771
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle8: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13746093
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle9: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle10: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clGray
    end
    object cxStyle11: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 12625805
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle12: TcxStyle
      AssignedValues = [svColor]
      Color = clSilver
    end
    object cxStyle13: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle14: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle15: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle16: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle17: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 15461355
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle18: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle19: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6447714
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle20: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle21: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clSilver
    end
    object cxStyle22: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBlack
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object TreeListStyleSheetStormVGA: TcxTreeListStyleSheet
      Caption = 'Storm (VGA)'
      Styles.Content = cxStyle4
      Styles.Inactive = cxStyle8
      Styles.Selection = cxStyle11
      Styles.BandBackground = cxStyle1
      Styles.BandHeader = cxStyle2
      Styles.ColumnHeader = cxStyle3
      Styles.ContentEven = cxStyle5
      Styles.ContentOdd = cxStyle6
      Styles.Footer = cxStyle7
      Styles.Indicator = cxStyle9
      Styles.Preview = cxStyle10
      BuiltIn = True
    end
    object TreeListStyleSheetUserFormat4: TcxTreeListStyleSheet
      Caption = 'UserFormat4'
      Styles.Content = cxStyle15
      Styles.Inactive = cxStyle19
      Styles.Selection = cxStyle22
      Styles.BandBackground = cxStyle12
      Styles.BandHeader = cxStyle13
      Styles.ColumnHeader = cxStyle14
      Styles.ContentEven = cxStyle16
      Styles.ContentOdd = cxStyle17
      Styles.Footer = cxStyle18
      Styles.Indicator = cxStyle20
      Styles.Preview = cxStyle21
      BuiltIn = True
    end
  end
  object CheckList: TImageList
    DrawingStyle = dsTransparent
    Left = 112
    Top = 120
    Bitmap = {
      494C010106000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C60084848400C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600848484008484840084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400848484008484840084848400C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000FFFFFF00000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400C6C6C600848484008484840084848400C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF0000000000000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C60084848400C6C6C600C6C6C600C6C6C60084848400848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C6008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF00FFFFFFFF00000000FFFFFFFF00000000
      C003C00300000000DFFBDFFB00000000DFFBD9FB00000000DFFBD0FB00000000
      DFFBD07B00000000DFFBD63B00000000DFFBDF1B00000000DFFBDF8B00000000
      DFFBDFCB00000000DFFBDFEB00000000DFFBDFFB00000000C003C00300000000
      FFFFFFFF00000000FFFFFFFF00000000FFFFFFFFFFFFFFFFC001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000
      000000000000}
  end
end
