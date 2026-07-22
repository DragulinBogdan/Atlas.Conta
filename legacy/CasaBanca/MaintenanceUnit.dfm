object FrmSettings: TFrmSettings
  Left = 472
  Top = 287
  Caption = 'Setari Caracteristici Grid'
  ClientHeight = 398
  ClientWidth = 410
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Label5: TLabel
    Left = 40
    Top = 181
    Width = 55
    Height = 13
    Caption = 'Primul Nivel'
  end
  object Label8: TLabel
    Left = 16
    Top = 8
    Width = 101
    Height = 13
    Caption = 'Culoari fara focus'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
  end
  object Bevel2: TBevel
    Left = 120
    Top = 16
    Width = 266
    Height = 3
    Shape = bsTopLine
  end
  object SettingsControl: TPageControl
    Left = 0
    Top = 0
    Width = 410
    Height = 328
    ActivePage = tabSettings
    Align = alClient
    TabOrder = 0
    ExplicitHeight = 366
    object tabSettings: TTabSheet
      Caption = 'Setari'
      ExplicitHeight = 338
      object Label1: TLabel
        Left = 27
        Top = 34
        Width = 55
        Height = 13
        Caption = 'Primul Nivel'
      end
      object Label2: TLabel
        Left = 27
        Top = 59
        Width = 60
        Height = 13
        Caption = 'Al 2 lea nivel'
      end
      object Label3: TLabel
        Left = 27
        Top = 83
        Width = 117
        Height = 13
        Caption = 'Stergere pe al 2 lea nivel'
      end
      object Label4: TLabel
        Left = 43
        Top = 121
        Width = 59
        Height = 13
        Caption = 'Nivel parinte'
      end
      object Label6: TLabel
        Left = 59
        Top = 148
        Width = 50
        Height = 13
        Caption = 'Nivel Copil'
      end
      object Label14: TLabel
        Left = 27
        Top = 170
        Width = 23
        Height = 13
        Caption = 'Data'
      end
      object Label20: TLabel
        Left = 27
        Top = 193
        Width = 66
        Height = 13
        Caption = 'CelulaCurenta'
      end
      object Label21: TLabel
        Left = 8
        Top = 236
        Width = 84
        Height = 13
        Caption = 'Mod de cautare : '
      end
      object Label7: TLabel
        Left = 16
        Top = 14
        Width = 94
        Height = 13
        Caption = 'Culori fara focus'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 115
        Top = 21
        Width = 279
        Height = 3
        Shape = bsTopLine
      end
      object Label9: TLabel
        Left = 16
        Top = 104
        Width = 86
        Height = 13
        Caption = 'Culori cu focus'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel3: TBevel
        Left = 106
        Top = 112
        Width = 290
        Height = 3
        Shape = bsTopLine
      end
      object Label25: TLabel
        Left = 8
        Top = 260
        Width = 99
        Height = 13
        Caption = 'Numar de zecimale : '
      end
      object pn_FirstLevelColor: TPanel
        Tag = 1
        Left = 94
        Top = 31
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Primul Nivel'
        TabOrder = 0
      end
      object pn_SecondLevelColor: TPanel
        Tag = 3
        Left = 94
        Top = 56
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Al 2 lea nivel'
        TabOrder = 1
      end
      object pn_DeletedSecondLevelColor: TPanel
        Tag = 5
        Left = 94
        Top = 80
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Stergere pe al 2 lea nivel'
        TabOrder = 2
      end
      object pn_ParentColor: TPanel
        Tag = 7
        Left = 107
        Top = 118
        Width = 286
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Primul Nivel'
        TabOrder = 3
      end
      object pn_ChildColor: TPanel
        Tag = 9
        Left = 120
        Top = 143
        Width = 273
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel Copil'
        TabOrder = 4
      end
      object pn_DataColor: TPanel
        Tag = 1
        Left = 94
        Top = 167
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = '01/01/2004'
        TabOrder = 5
      end
      object pn_FocusedColor: TPanel
        Tag = 1
        Left = 94
        Top = 190
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'CelulaCurenta'
        TabOrder = 6
      end
      object chkAutomat: TCheckBox
        Left = 8
        Top = 212
        Width = 289
        Height = 17
        Caption = 'Mod de Culegere Automat ()  / cu "?" (X)'
        TabOrder = 7
        OnClick = chkAutomatClick
      end
      object edtSearchType: TdxImageEdit
        Left = 107
        Top = 231
        Width = 281
        TabOrder = 8
        OnChange = edtSearchTypeChange
        Descriptions.Strings = (
          'Incepand exact cu .....'
          'Continand textul ...'
          'Se termina in textul ..')
        ImageIndexes.Strings = (
          '0'
          '1'
          '2')
        Values.Strings = (
          '0'
          '1'
          '2')
      end
      object edtNrDecimal: TdxSpinEdit
        Left = 106
        Top = 256
        Width = 110
        TabOrder = 9
        OnChange = edtNrDecimalChange
        MaxValue = 8.000000000000000000
        StoredValues = 48
      end
    end
    object tabTransfer: TTabSheet
      Caption = 'Transfer'
      ImageIndex = 1
      ExplicitHeight = 338
      object Label10: TLabel
        Left = 16
        Top = -1
        Width = 85
        Height = 13
        Caption = 'Etape Transfer'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel4: TBevel
        Left = 120
        Top = 6
        Width = 266
        Height = 3
        Shape = bsTopLine
      end
    end
    object tabSetariGrid: TTabSheet
      Caption = 'Setari Deconturi'
      ImageIndex = 2
      ExplicitHeight = 338
      object Label26: TLabel
        Left = 30
        Top = 135
        Width = 33
        Height = 13
        Caption = 'Nivel 6'
      end
      object Label27: TLabel
        Left = 30
        Top = 113
        Width = 33
        Height = 13
        Caption = 'Nivel 5'
      end
      object Label28: TLabel
        Left = 30
        Top = 91
        Width = 33
        Height = 13
        Caption = 'Nivel 4'
      end
      object Label29: TLabel
        Left = 30
        Top = 69
        Width = 33
        Height = 13
        Caption = 'Nivel 3'
      end
      object Label30: TLabel
        Left = 30
        Top = 50
        Width = 33
        Height = 13
        Caption = 'Nivel 2'
      end
      object Label31: TLabel
        Left = 30
        Top = 28
        Width = 33
        Height = 13
        Caption = 'Nivel 1'
      end
      object Label32: TLabel
        Left = 16
        Top = 12
        Width = 103
        Height = 13
        Caption = 'Situatie Deconturi'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label33: TLabel
        Left = 15
        Top = 151
        Width = 84
        Height = 13
        Caption = 'Setari Validare'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label34: TLabel
        Left = 30
        Top = 168
        Width = 38
        Height = 13
        Caption = 'Validare'
      end
      object Label35: TLabel
        Left = 15
        Top = 190
        Width = 83
        Height = 13
        Caption = 'Camp Transfer'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label36: TLabel
        Left = 30
        Top = 210
        Width = 74
        Height = 13
        Caption = 'Mod Vizualizare'
      end
      object Label37: TLabel
        Left = 16
        Top = 234
        Width = 149
        Height = 13
        Caption = 'Raport Dispozitie de Plata'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label38: TLabel
        Left = 30
        Top = 256
        Width = 32
        Height = 13
        Caption = 'Raport'
      end
      object pnDLevel4B: TPanel
        Tag = 1
        Left = 94
        Top = 130
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel 4B'
        TabOrder = 0
      end
      object pnDLevel3B: TPanel
        Tag = 1
        Left = 94
        Top = 109
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel 3B'
        TabOrder = 1
      end
      object pnDLevel4A: TPanel
        Tag = 1
        Left = 94
        Top = 88
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel 4A'
        TabOrder = 2
      end
      object pnDLevel3A: TPanel
        Tag = 1
        Left = 94
        Top = 67
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel 3A'
        TabOrder = 3
      end
      object pnDLevel2: TPanel
        Tag = 1
        Left = 94
        Top = 46
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel2'
        TabOrder = 4
      end
      object pnDLevel1: TPanel
        Tag = 1
        Left = 94
        Top = 25
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Nivel 1'
        TabOrder = 5
      end
      object pn_Validare: TPanel
        Tag = 1
        Left = 94
        Top = 165
        Width = 299
        Height = 20
        BevelInner = bvLowered
        BevelOuter = bvNone
        Caption = 'Validare'
        TabOrder = 6
      end
      object edtModVizTransfer: TdxImageEdit
        Left = 108
        Top = 204
        Width = 280
        TabOrder = 7
        OnChange = edtModVizTransferChange
        Descriptions.Strings = (
          'Icon si Text'
          'Icon si hint Text'
          'Text')
        ImageIndexes.Strings = (
          '0'
          '1'
          '2')
        Values.Strings = (
          '0'
          '1'
          '2')
      end
      object edtDispRaport: TdxButtonEdit
        Left = 108
        Top = 251
        Width = 280
        TabOrder = 8
        ReadOnly = True
        Buttons = <
          item
            Default = True
          end>
        OnButtonClick = edtDispRaportButtonClick
        StoredValues = 64
        ExistButtons = True
      end
      object chkDisplayStr: TCheckBox
        Left = 16
        Top = 276
        Width = 289
        Height = 17
        Caption = 'Mod de Afisare Case Arbore ()  / Simplu (X)'
        TabOrder = 9
        OnClick = chkDisplayStrClick
      end
    end
    object tabSaveSheet: TTabSheet
      Caption = 'Salvari'
      ImageIndex = 3
      ExplicitHeight = 338
      object Label11: TLabel
        Left = 16
        Top = 6
        Width = 121
        Height = 13
        Caption = 'Setari pentru Salvare'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel5: TBevel
        Left = 144
        Top = 13
        Width = 242
        Height = 3
        Shape = bsTopLine
      end
      object chkSaveCasaDefault: TCheckBox
        Left = 24
        Top = 28
        Width = 137
        Height = 17
        Caption = 'Salvare Casa Implicta'
        TabOrder = 0
        OnClick = chkSaveCasaDefaultClick
      end
      object chkSavePeZi: TCheckBox
        Left = 24
        Top = 52
        Width = 161
        Height = 17
        Caption = 'Salvare optiune "Pe Zi"'
        TabOrder = 1
        OnClick = chkSaveCasaDefaultClick
      end
      object chkSaveZileAnt: TCheckBox
        Left = 24
        Top = 76
        Width = 217
        Height = 17
        Caption = 'Salvarea numar de zile anterioare'
        TabOrder = 2
        OnClick = chkSaveCasaDefaultClick
      end
      object chkSaveTransfImg: TCheckBox
        Left = 24
        Top = 147
        Width = 137
        Height = 17
        Caption = 'Salvare imagini transfer'
        TabOrder = 3
        OnClick = chkSaveCasaDefaultClick
      end
      object chkSaveDataStart: TCheckBox
        Left = 24
        Top = 100
        Width = 137
        Height = 17
        Caption = 'Salvare data inceput'
        TabOrder = 4
        OnClick = chkSaveCasaDefaultClick
      end
      object chkSaveTipDefalcare: TCheckBox
        Left = 24
        Top = 124
        Width = 121
        Height = 17
        Caption = 'Salvare tip defalcare'
        TabOrder = 5
        OnClick = chkSaveCasaDefaultClick
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 328
    Width = 410
    Height = 70
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 1
    DesignSize = (
      410
      70)
    object btnOk: TBitBtn
      Left = 250
      Top = 6
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Kind = bkOK
      NumGlyphs = 2
      TabOrder = 0
    end
    object btnCancel: TBitBtn
      Left = 325
      Top = 6
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Kind = bkCancel
      NumGlyphs = 2
      TabOrder = 1
    end
    object btnDefault: TBitBtn
      Left = 3
      Top = 5
      Width = 131
      Height = 25
      Caption = 'Incarca Culori Default'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        0400000000000001000000000000000000001000000010000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333330000000
        00003333377777777777333330FFFFFFFFF03FF3F7FFFF33FFF7003000000FF0
        00F077F7777773F77737E00FBFBFB0FFFFF07773333FF7FF33F7E0FBFB00000F
        F0F077F333777773F737E0BFBFBFBFB0FFF077F3333FFFF733F7E0FBFB00000F
        F0F077F333777773F737E0BFBFBFBFB0FFF077F33FFFFFF733F7E0FB0000000F
        F0F077FF777777733737000FB0FFFFFFFFF07773F7F333333337333000FFFFFF
        FFF0333777F3FFF33FF7333330F000FF0000333337F777337777333330FFFFFF
        0FF0333337FFFFFF7F37333330CCCCCC0F033333377777777F73333330FFFFFF
        0033333337FFFFFF773333333000000003333333377777777333}
      NumGlyphs = 2
      TabOrder = 2
      OnClick = btnDefaultClick
    end
  end
  object rbColor: TRadioButton
    Left = 296
    Top = 2
    Width = 57
    Height = 17
    Caption = '&Culoare'
    Checked = True
    TabOrder = 2
    TabStop = True
  end
  object rbFont: TRadioButton
    Left = 360
    Top = 2
    Width = 49
    Height = 17
    Caption = '&Font'
    TabOrder = 3
  end
  object ColorDialog: TColorDialog
    Options = [cdAnyColor]
    Left = 180
    Top = 332
  end
  object FontDialog: TFontDialog
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    Left = 212
    Top = 332
  end
end
