object frmCopyStock: TfrmCopyStock
  Left = 428
  Top = 274
  AutoScroll = False
  Caption = 'Copiere Stoc'
  ClientHeight = 185
  ClientWidth = 285
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 285
    Height = 39
    Hint = 'Validare Decont'
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
    Info = 'Copiere Stoc'
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
      285
      39)
  end
  object pnBottom: TPanel
    Left = 0
    Top = 153
    Width = 285
    Height = 32
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 1
    DesignSize = (
      285
      32)
    object edtPredator: TdxEdit
      Left = 16
      Top = 4
      Width = 33
      TabOrder = 0
      StyleController = StyleController
    end
    object btnAccept: TcxButton
      Tag = 1
      Left = 107
      Top = 4
      Width = 81
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'A&ccepta'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
      OnClick = btnAcceptClick
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000000000000000000000000
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
    end
    object btnCancel: TcxButton
      Left = 191
      Top = 4
      Width = 92
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = '&Abandon'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 2
      OnClick = btnCancelClick
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000000000000000000000000
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
    end
  end
  object pnRest: TPanel
    Left = 0
    Top = 39
    Width = 285
    Height = 114
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 2
    object Label1: TLabel
      Left = 22
      Top = 48
      Width = 58
      Height = 13
      Caption = 'Data Stoc'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 22
      Top = 16
      Width = 81
      Height = 13
      Caption = 'Gestiune Stoc'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object lbGestiuneStoc: TLabel
      Left = 125
      Top = 16
      Width = 81
      Height = 13
      Caption = 'Gestiune Stoc'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      Transparent = False
    end
    object Label4: TLabel
      Left = 22
      Top = 80
      Width = 57
      Height = 13
      Caption = 'Cont Stoc'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edtDataStock: TdxDateEdit
      Left = 125
      Top = 40
      Width = 121
      TabOrder = 0
      StyleController = StyleController
      Date = -700000.000000000000000000
      UseEditMask = True
      StoredValues = 4
    end
    object edtContStock: TdxImageEdit
      Left = 125
      Top = 72
      Width = 121
      TabOrder = 1
      StyleController = StyleController
    end
  end
  object StyleController: TdxEditStyleController
    BorderColor = 14065456
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 142
    Top = 4
  end
end
