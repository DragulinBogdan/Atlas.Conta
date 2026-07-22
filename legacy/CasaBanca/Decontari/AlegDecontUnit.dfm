object frmAlegDecont: TfrmAlegDecont
  Left = 498
  Top = 219
  BorderStyle = bsSingle
  Caption = 'Detalii Decont'
  ClientHeight = 178
  ClientWidth = 321
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 321
    Height = 34
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
    Info = 'Detalii Decont'
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
      321
      34)
  end
  object pnRest: TPanel
    Left = 0
    Top = 34
    Width = 321
    Height = 94
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 38
    DesignSize = (
      321
      94)
    object lbNrDec: TLabel
      Left = 17
      Top = 8
      Width = 63
      Height = 13
      Caption = 'Nr. Decont'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object lbDataDec: TLabel
      Left = 142
      Top = 8
      Width = 73
      Height = 13
      Caption = 'Data Decont'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object btnDecont: TSpeedButton
      Left = 297
      Top = 5
      Width = 15
      Height = 17
      Caption = '...'
      Flat = True
      Transparent = False
      OnClick = btnDecontClick
    end
    object Label1: TLabel
      Left = 17
      Top = 29
      Width = 53
      Height = 13
      Caption = 'Reparitor'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edtDataDec: TdxDateEdit
      Left = 218
      Top = 3
      Width = 79
      TabOrder = 1
      ReadOnly = False
      StyleController = StyleController
      Date = -700000.000000000000000000
      SaveTime = False
      UseEditMask = True
      StoredValues = 68
    end
    object edtNrDec: TdxSpinEdit
      Left = 83
      Top = 3
      Width = 54
      TabOrder = 0
      StyleController = StyleController
      OnChange = edtNrDecChange
    end
    object edtRep: TdxPopupEdit
      Left = 73
      Top = 26
      Width = 238
      TabOrder = 2
      OnEnter = edtRepEnter
      OnKeyDown = edtRepKeyDown
      Anchors = [akLeft, akTop, akRight]
      ReadOnly = False
      StyleController = StyleController
      OnChange = edtNrDecChange
      HideEditCursor = True
      PopupFormBorderStyle = pbsSysPanel
      PopupWidth = 400
      OnCloseUp = edtRepCloseUp
      StoredValues = 64
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 128
    Width = 321
    Height = 50
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 2
    DesignSize = (
      321
      50)
    object btnOk: TSpeedButton
      Left = 163
      Top = 6
      Width = 75
      Height = 22
      Anchors = [akRight, akBottom]
      Caption = '&OK'
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
      OnClick = btnOkClick
    end
    object btnCancel: TSpeedButton
      Left = 244
      Top = 6
      Width = 75
      Height = 22
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
  end
  object StyleController: TdxEditStyleController
    BorderColor = 14065456
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 262
    Top = 4
  end
end
