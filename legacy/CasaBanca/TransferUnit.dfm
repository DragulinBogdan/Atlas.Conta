object frmTransfer: TfrmTransfer
  Left = 279
  Top = 235
  BorderStyle = bsSingle
  Caption = 'Transfer intre Case'
  ClientHeight = 254
  ClientWidth = 392
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
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 392
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
    Info = 'Transfer intre Case / Banci / Deconturi'
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
      392
      34)
  end
  object pnRest: TPanel
    Left = 0
    Top = 34
    Width = 392
    Height = 177
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 172
    object Label1: TLabel
      Left = 25
      Top = 35
      Width = 88
      Height = 13
      Caption = 'Casa destinatie'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 27
      Top = 60
      Width = 77
      Height = 13
      Caption = 'Suma Destinatie'
    end
    object Label3: TLabel
      Left = 26
      Top = 14
      Width = 63
      Height = 13
      Caption = 'Casa Plecare'
    end
    object lbDataIesire: TLabel
      Left = 6
      Top = 83
      Width = 50
      Height = 13
      Caption = 'Data iesire'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      ParentFont = False
    end
    object lbDataIntrare: TLabel
      Left = 195
      Top = 83
      Width = 68
      Height = 13
      Caption = 'Data intrare'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object lbNrDec: TLabel
      Left = 1
      Top = 113
      Width = 52
      Height = 13
      Caption = 'Nr. Decont'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      ParentFont = False
    end
    object lbDataDec: TLabel
      Left = 123
      Top = 113
      Width = 61
      Height = 13
      Caption = 'Data Decont'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      ParentFont = False
    end
    object btnDecont: TSpeedButton
      Left = 321
      Top = 108
      Width = 64
      Height = 21
      Caption = 'Sel Decont'
      Flat = True
      Transparent = False
      OnClick = btnDecontClick
    end
    object edTransferHouse: TdxPopupEdit
      Left = 116
      Top = 29
      Width = 269
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      ParentFont = False
      Style.HotTrack = True
      TabOrder = 1
      OnKeyDown = edTransferHouseKeyDown
      StyleController = StyleController
      OnChange = edTransferHouseChange
      HideEditCursor = True
      PopupFormBorderStyle = pbsSysPanel
      OnCloseUp = edTransferHouseCloseUp
    end
    object chkConfirm: TdxCheckEdit
      Left = 236
      Top = 140
      Width = 149
      TabOrder = 7
      OnClick = chkConfirmClick
      Caption = 'Cu Confirmare de primire'
      StyleController = StyleController
    end
    object edtSuma: TdxCurrencyEdit
      Left = 116
      Top = 52
      Width = 269
      TabOrder = 2
      ReadOnly = True
      StyleController = StyleController
      DisplayFormat = ',0.00;,0.00'
      StoredValues = 64
    end
    object edtCasa: TdxEdit
      Left = 116
      Top = 6
      Width = 269
      TabOrder = 0
      ReadOnly = True
      StyleController = StyleController
      StoredValues = 64
    end
    object edtDataPlec: TdxDateEdit
      Left = 65
      Top = 80
      Width = 121
      TabOrder = 3
      ReadOnly = True
      StyleController = StyleController
      Date = -700000.000000000000000000
      SaveTime = False
      UseEditMask = True
      StoredValues = 68
    end
    object edtDataDest: TdxDateEdit
      Left = 265
      Top = 80
      Width = 120
      TabOrder = 4
      StyleController = StyleController
      Date = -700000.000000000000000000
      SaveTime = False
      UseEditMask = True
      StoredValues = 4
    end
    object ledDataDec: TdxDateEdit
      Left = 189
      Top = 108
      Width = 120
      TabOrder = 6
      Visible = False
      ReadOnly = False
      StyleController = StyleController
      Date = -700000.000000000000000000
      SaveTime = False
      UseEditMask = True
      StoredValues = 68
    end
    object edtNrDec: TdxSpinEdit
      Left = 56
      Top = 108
      Width = 54
      TabOrder = 5
      StyleController = StyleController
      OnChange = edtNrDecChange
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 211
    Width = 392
    Height = 43
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 2
    ExplicitTop = 192
    DesignSize = (
      392
      43)
    object btnOk: TSpeedButton
      Left = 240
      Top = 6
      Width = 69
      Height = 22
      Anchors = [akRight, akBottom]
      Caption = '&OK'
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
      OnClick = btnOkClick
    end
    object btnCancel: TSpeedButton
      Left = 315
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
    Left = 358
    Top = 4
  end
end
