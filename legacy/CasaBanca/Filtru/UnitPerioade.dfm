object frmSelectPeriod: TfrmSelectPeriod
  Left = 290
  Top = 279
  Caption = 'Selectare Perioada'
  ClientHeight = 334
  ClientWidth = 712
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 712
    Height = 34
    Hint = 'Validare Decont'
    Align = alTop
    Alignment = taLeftJustify
    BevelInner = bvRaised
    BevelOuter = bvNone
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Selectare perioada de filtrare'
    InfoFont.Charset = ANSI_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'Arial Black'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 5
    ActAsCaption = False
    HideCaption = False
    DesignSize = (
      712
      34)
  end
  object pnBottom: TPanel
    Left = 0
    Top = 304
    Width = 712
    Height = 30
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 1
    ExplicitTop = 141
    DesignSize = (
      712
      30)
    object BitBtn1: TSpeedButton
      Left = 548
      Top = 3
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'OK'
      Enabled = False
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
        3333333333333333333333330000333333333333333333333333F33333333333
        00003333344333333333333333388F3333333333000033334224333333333333
        338338F3333333330000333422224333333333333833338F3333333300003342
        222224333333333383333338F3333333000034222A22224333333338F338F333
        8F33333300003222A3A2224333333338F3838F338F33333300003A2A333A2224
        33333338F83338F338F33333000033A33333A222433333338333338F338F3333
        0000333333333A222433333333333338F338F33300003333333333A222433333
        333333338F338F33000033333333333A222433333333333338F338F300003333
        33333333A222433333333333338F338F00003333333333333A22433333333333
        3338F38F000033333333333333A223333333333333338F830000333333333333
        333A333333333333333338330000333333333333333333333333333333333333
        0000}
      NumGlyphs = 2
      ParentFont = False
      OnClick = BitBtn1Click
    end
    object BitBtn2: TSpeedButton
      Left = 628
      Top = 3
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'Cancel'
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
      OnClick = BitBtn2Click
    end
  end
  object pnRest: TPanel
    Left = 0
    Top = 34
    Width = 712
    Height = 270
    Align = alClient
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 2
    ExplicitHeight = 107
    object pnTimeSaptamana: TPanel
      Tag = 2
      Left = 2
      Top = 2
      Width = 410
      Height = 50
      Color = clWhite
      TabOrder = 0
      object Label4: TLabel
        Left = 6
        Top = 2
        Width = 72
        Height = 13
        Caption = 'In Saptamna'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label5: TLabel
        Left = 4
        Top = 29
        Width = 69
        Height = 13
        Caption = 'In Saptamana '
      end
      object Label6: TLabel
        Left = 246
        Top = 27
        Width = 37
        Height = 13
        Caption = 'din luna'
      end
      object edSaptamana: TdxImageEdit
        Left = 77
        Top = 21
        Width = 158
        TabOrder = 0
        StyleController = StyleController
      end
      object edLunaAn: TdxImageEdit
        Left = 293
        Top = 20
        Width = 114
        TabOrder = 1
        StyleController = StyleController
      end
      object rbSaptamana: TRadioButton
        Tag = 2
        Left = 4
        Top = 2
        Width = 105
        Height = 17
        Alignment = taLeftJustify
        Caption = 'In Saptamana'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 2
      end
    end
    object pnTimeZiua: TPanel
      Tag = 5
      Left = 413
      Top = 2
      Width = 122
      Height = 50
      Color = clWhite
      TabOrder = 1
      object edZi: TdxDateEdit
        Left = 25
        Top = 21
        Width = 88
        TabOrder = 0
        StyleController = StyleController
        Date = 37987.000000000000000000
        UseEditMask = True
        StoredValues = 4
      end
      object rbZiua: TRadioButton
        Left = 4
        Top = 1
        Width = 66
        Height = 16
        Alignment = taLeftJustify
        Caption = 'In Ziua'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 1
      end
    end
    object pnTimeLunaAn: TPanel
      Tag = 3
      Left = 536
      Top = 1
      Width = 172
      Height = 50
      Color = clWhite
      TabOrder = 2
      object edLuna: TdxImageEdit
        Left = 15
        Top = 21
        Width = 154
        TabOrder = 0
        StyleController = StyleController
      end
      object rbLuna: TRadioButton
        Left = 4
        Top = 2
        Width = 67
        Height = 16
        Alignment = taLeftJustify
        Caption = 'In Luna'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 1
      end
    end
    object pnTimeAnul: TPanel
      Tag = 4
      Left = 491
      Top = 55
      Width = 154
      Height = 50
      Color = clWhite
      TabOrder = 3
      object edAn: TdxImageEdit
        Left = 32
        Top = 20
        Width = 114
        TabOrder = 0
        StyleController = StyleController
      end
      object rbAnul: TRadioButton
        Left = 4
        Top = 1
        Width = 69
        Height = 17
        Alignment = taLeftJustify
        Caption = 'In Anul'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 1
      end
    end
    object pnTimePeriod: TPanel
      Tag = 6
      Left = 2
      Top = 53
      Width = 233
      Height = 50
      Color = clWhite
      TabOrder = 4
      object edNrZile: TdxSpinEdit
        Left = 23
        Top = 23
        Width = 49
        TabOrder = 0
        StyleController = StyleController
      end
      object rb_Zile: TRadioButton
        Left = 105
        Top = 13
        Width = 40
        Height = 16
        Alignment = taLeftJustify
        Caption = 'Zile'
        Checked = True
        TabOrder = 1
        TabStop = True
      end
      object rb_Sapt: TRadioButton
        Left = 105
        Top = 28
        Width = 73
        Height = 16
        Alignment = taLeftJustify
        Caption = 'Saptamani'
        TabOrder = 2
      end
      object rb_Luni: TRadioButton
        Left = 185
        Top = 13
        Width = 40
        Height = 16
        Alignment = taLeftJustify
        Caption = 'Luni'
        TabOrder = 3
      end
      object rb_Ani: TRadioButton
        Left = 185
        Top = 29
        Width = 40
        Height = 17
        Alignment = taLeftJustify
        Caption = 'Ani'
        TabOrder = 4
      end
      object rbLast: TRadioButton
        Left = 4
        Top = 2
        Width = 101
        Height = 16
        Alignment = taLeftJustify
        Caption = '<B>Ultimile n.....</B>'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = []
        ParentFont = False
        TabOrder = 5
      end
    end
    object pnTimeDelaLa: TPanel
      Tag = 1
      Left = 241
      Top = 53
      Width = 245
      Height = 50
      Color = clWhite
      TabOrder = 5
      object Label2: TLabel
        Left = 5
        Top = 27
        Width = 25
        Height = 13
        Caption = 'De la'
      end
      object Label3: TLabel
        Left = 135
        Top = 27
        Width = 12
        Height = 13
        Caption = 'La'
      end
      object edDataDeLa: TdxDateEdit
        Left = 37
        Top = 22
        Width = 88
        TabOrder = 0
        StyleController = StyleController
        Date = 37987.000000000000000000
        UseEditMask = True
        StoredValues = 4
      end
      object edDataLa: TdxDateEdit
        Left = 152
        Top = 22
        Width = 88
        TabOrder = 1
        StyleController = StyleController
        Date = 37987.000000000000000000
        UseEditMask = True
        StoredValues = 4
      end
      object rbDelaLa: TRadioButton
        Left = 4
        Top = 2
        Width = 101
        Height = 17
        Alignment = taLeftJustify
        Caption = 'De la data la data'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 2
      end
    end
  end
  object StyleController: TdxEditStyleController
    BorderColor = 14065456
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 606
    Top = 92
  end
end
