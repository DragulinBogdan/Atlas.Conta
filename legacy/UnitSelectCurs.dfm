object frmSelectCursValutar: TfrmSelectCursValutar
  Left = 340
  Top = 225
  AutoScroll = False
  ClientHeight = 197
  ClientWidth = 427
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  DesignSize = (
    427
    197)
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 427
    Height = 34
    Align = alTop
    Alignment = taLeftJustify
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Curs Valutar'
    InfoFont.Charset = ANSI_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'Arial Black'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 5
    ActAsCaption = True
    HideCaption = True
    DesignSize = (
      427
      34)
  end
  object pnBottom: TPanel
    Left = 0
    Top = 165
    Width = 427
    Height = 32
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 1
    DesignSize = (
      427
      32)
    object btnCancel: TSpeedButton
      Left = 350
      Top = 4
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = '&Cancel'
      Flat = True
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000010000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333333333000033338833333333333333333F333333333333
        0000333911833333983333333388F333333F3333000033391118333911833333
        38F38F333F88F33300003339111183911118333338F338F3F8338F3300003333
        911118111118333338F3338F833338F3000033333911111111833333338F3338
        3333F8330000333333911111183333333338F333333F83330000333333311111
        8333333333338F3333383333000033333339111183333333333338F333833333
        00003333339111118333333333333833338F3333000033333911181118333333
        33338333338F333300003333911183911183333333383338F338F33300003333
        9118333911183333338F33838F338F33000033333913333391113333338FF833
        38F338F300003333333333333919333333388333338FFF830000333333333333
        3333333333333333333888330000333333333333333333333333333333333333
        0000}
      NumGlyphs = 2
      ParentFont = False
      OnClick = btnCancelClick
    end
    object btnOk: TSpeedButton
      Left = 272
      Top = 3
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = '&Ok'
      Flat = True
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
        555555555555555555555555555555555555555555FF55555555555559055555
        55555555577FF5555555555599905555555555557777F5555555555599905555
        555555557777FF5555555559999905555555555777777F555555559999990555
        5555557777777FF5555557990599905555555777757777F55555790555599055
        55557775555777FF5555555555599905555555555557777F5555555555559905
        555555555555777FF5555555555559905555555555555777FF55555555555579
        05555555555555777FF5555555555557905555555555555777FF555555555555
        5990555555555555577755555555555555555555555555555555}
      NumGlyphs = 2
      ParentFont = False
      OnClick = btnOkClick
    end
  end
  object pnRest: TPanel
    Left = 0
    Top = 34
    Width = 427
    Height = 131
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 2
    object PageControl: TPageControl
      Left = 2
      Top = 2
      Width = 423
      Height = 127
      ActivePage = tabCursNegociat
      Align = alClient
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      MultiLine = True
      ParentFont = False
      ScrollOpposite = True
      TabOrder = 0
      TabPosition = tpBottom
      object tabCursNegociat: TTabSheet
        Caption = 'Curs Negociat'
        Highlighted = True
        DesignSize = (
          415
          101)
        object Label1: TLabel
          Left = 12
          Top = 61
          Width = 105
          Height = 13
          Caption = 'Tipul de Valuta : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Label2: TLabel
          Left = 12
          Top = 87
          Width = 103
          Height = 13
          Caption = 'Cursul Negociat : '
        end
        object Label3: TLabel
          Left = 12
          Top = 4
          Width = 102
          Height = 13
          Caption = 'Furnizor/Client : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Label4: TLabel
          Left = 12
          Top = 28
          Width = 61
          Height = 13
          Caption = 'Contract : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
        end
        object Bevel1: TBevel
          Left = 6
          Top = 46
          Width = 404
          Height = 6
          Shape = bsBottomLine
        end
        object edTipValuta: TdxEdit
          Left = 120
          Top = 57
          Width = 280
          TabOrder = 0
          Anchors = [akLeft, akTop, akRight]
          StyleController = StyleController
        end
        object edFurnizor: TdxEdit
          Left = 120
          Top = 0
          Width = 280
          TabOrder = 1
          Anchors = [akLeft, akTop, akRight]
          StyleController = StyleController
        end
        object edContract: TdxImageEdit
          Left = 120
          Top = 26
          Width = 280
          TabOrder = 2
          Anchors = [akLeft, akTop, akRight]
          StyleController = StyleController
        end
      end
      object tabConfigurareCurs: TTabSheet
        Caption = 'Dechidere Nomenclator'
        Highlighted = True
        ImageIndex = 2
        DesignSize = (
          415
          101)
        object btnOpenNomenclator: TSpeedButton
          Left = 239
          Top = 67
          Width = 169
          Height = 25
          Anchors = [akRight, akBottom]
          Caption = '&Deschidere Nomenclator'
          Flat = True
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          NumGlyphs = 2
          ParentFont = False
          OnClick = btnOpenNomenclatorClick
        end
        object Label5: TLabel
          Left = 12
          Top = 28
          Width = 112
          Height = 13
          Caption = 'Data actualizare : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Label6: TLabel
          Left = 12
          Top = 5
          Width = 105
          Height = 13
          Caption = 'Tipul de Valuta : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Bevel2: TBevel
          Left = 6
          Top = 46
          Width = 404
          Height = 6
          Shape = bsBottomLine
        end
        object Label9: TLabel
          Left = 4
          Top = 55
          Width = 103
          Height = 13
          Caption = 'Cursul Negociat : '
        end
        object edDataNomenclator: TdxDateEdit
          Left = 120
          Top = 24
          Width = 121
          TabOrder = 0
          StyleController = StyleController
          Date = -700000.000000000000000000
          UseEditMask = True
          StoredValues = 4
        end
        object edTipValuta1: TdxEdit
          Left = 120
          Top = 1
          Width = 280
          TabOrder = 1
          Anchors = [akLeft, akTop, akRight]
          StyleController = StyleController
        end
        object edValNomenclator: TcxCurrencyEdit
          Left = 8
          Top = 69
          Anchors = [akLeft, akTop, akRight]
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ';-,0.0000 '
          TabOrder = 2
          Width = 224
        end
      end
      object tabCursOnline: TTabSheet
        Caption = 'Curs Online'
        Highlighted = True
        ImageIndex = 1
        DesignSize = (
          415
          101)
        object Label7: TLabel
          Left = 12
          Top = 5
          Width = 105
          Height = 13
          Caption = 'Tipul de Valuta : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Label8: TLabel
          Left = 12
          Top = 28
          Width = 112
          Height = 13
          Caption = 'Data actualizare : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold, fsItalic]
          ParentFont = False
        end
        object Bevel3: TBevel
          Left = 6
          Top = 46
          Width = 403
          Height = 6
          Anchors = [akLeft, akTop, akRight]
          Shape = bsBottomLine
        end
        object btnCursOnline: TSpeedButton
          Left = 287
          Top = 67
          Width = 102
          Height = 25
          Anchors = [akTop, akRight]
          Caption = 'Curs On&line'
          Flat = True
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          NumGlyphs = 2
          ParentFont = False
          OnClick = btnCursOnlineClick
        end
        object Label10: TLabel
          Left = 4
          Top = 55
          Width = 103
          Height = 13
          Caption = 'Cursul Negociat : '
        end
        object edTipValuta2: TdxEdit
          Left = 120
          Top = 1
          Width = 280
          TabOrder = 0
          Anchors = [akLeft, akTop, akRight]
          StyleController = StyleController
          OnChange = edTipValuta2Change
        end
        object edDataCursOnline: TdxDateEdit
          Left = 120
          Top = 24
          Width = 121
          TabOrder = 1
          StyleController = StyleController
          Date = -700000.000000000000000000
          UseEditMask = True
          StoredValues = 4
        end
        object edValoareOnline: TcxCurrencyEdit
          Left = 16
          Top = 69
          Anchors = [akLeft, akTop, akRight]
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ';-,0.0000 '
          TabOrder = 2
          Width = 257
        end
      end
    end
  end
  object edValoareValuta: TcxCurrencyEdit
    Left = 126
    Top = 123
    Anchors = [akLeft, akTop, akRight]
    Properties.DecimalPlaces = 4
    Properties.DisplayFormat = ';-,0.0000 '
    TabOrder = 3
    Width = 281
  end
  object StyleController: TdxEditStyleController
    BorderColor = 14065456
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 390
    Top = 4
  end
end
