object frmAdaugareColoana: TfrmAdaugareColoana
  Left = 306
  Top = 204
  AutoScroll = False
  Caption = 'Adaugare Coloana Noua'
  ClientHeight = 415
  ClientWidth = 610
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 24
    Top = 48
    Width = 78
    Height = 13
    Caption = 'Nume coloana : '
  end
  object Label2: TLabel
    Left = 8
    Top = 16
    Width = 189
    Height = 13
    Caption = 'Definire elemente de identificare '
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
  end
  object Label3: TLabel
    Left = 272
    Top = 48
    Width = 37
    Height = 13
    Caption = 'Captura'
  end
  object Label4: TLabel
    Left = 24
    Top = 80
    Width = 63
    Height = 13
    Caption = 'Tip de data : '
  end
  object Label5: TLabel
    Left = 272
    Top = 80
    Width = 76
    Height = 13
    Caption = 'Clasa de editare'
  end
  object Label6: TLabel
    Left = 8
    Top = 104
    Width = 160
    Height = 13
    Caption = 'Definire elemente de afisare'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
  end
  object Label7: TLabel
    Left = 24
    Top = 128
    Width = 91
    Height = 13
    Caption = 'Dimensiune Minima'
  end
  object Label8: TLabel
    Left = 272
    Top = 128
    Width = 94
    Height = 13
    Caption = 'Dimensiune Maxima'
  end
  object Label9: TLabel
    Left = 24
    Top = 160
    Width = 71
    Height = 13
    Caption = 'Culoare Fundal'
  end
  object Label10: TLabel
    Left = 272
    Top = 160
    Width = 60
    Height = 13
    Caption = 'Culoare Font'
  end
  object Label11: TLabel
    Left = 24
    Top = 192
    Width = 52
    Height = 13
    Caption = 'Nume Font'
  end
  object Label12: TLabel
    Left = 272
    Top = 192
    Width = 79
    Height = 13
    Caption = 'Dimensiune Font'
  end
  object Label13: TLabel
    Left = 8
    Top = 256
    Width = 167
    Height = 13
    Caption = 'Definire elemente de validare'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
  end
  object Label14: TLabel
    Left = 32
    Top = 280
    Width = 71
    Height = 13
    Caption = 'Valoare minima'
  end
  object Label15: TLabel
    Left = 272
    Top = 280
    Width = 74
    Height = 13
    Caption = 'Valoare maxima'
  end
  object Label16: TLabel
    Left = 32
    Top = 312
    Width = 76
    Height = 13
    Caption = 'Valoare implicita'
  end
  object Label19: TLabel
    Left = 32
    Top = 384
    Width = 83
    Height = 13
    Caption = 'Formula de calcul'
  end
  object Label20: TLabel
    Left = 24
    Top = 224
    Width = 78
    Height = 13
    Caption = 'Aliniament Camp'
  end
  object Label21: TLabel
    Left = 272
    Top = 224
    Width = 88
    Height = 13
    Caption = 'Aliniament Captura'
  end
  object Bevel1: TBevel
    Left = 200
    Top = 24
    Width = 313
    Height = 9
    Shape = bsTopLine
  end
  object Bevel2: TBevel
    Left = 176
    Top = 112
    Width = 337
    Height = 9
    Shape = bsTopLine
  end
  object Bevel3: TBevel
    Left = 176
    Top = 264
    Width = 337
    Height = 9
    Shape = bsTopLine
  end
  object edNumeColoana: TdxDBEdit
    Left = 104
    Top = 43
    Width = 161
    TabOrder = 0
    DataField = 'FIELD_NAME'
    DataSource = DTColoana
  end
  object edtCaption: TdxDBMaskEdit
    Left = 360
    Top = 43
    Width = 153
    TabOrder = 1
    DataField = 'CAPTION'
    DataSource = DTColoana
    IgnoreMaskBlank = False
  end
  object edtTipData: TdxImageEdit
    Left = 104
    Top = 72
    Width = 161
    TabOrder = 2
    OnChange = edtTipDataChange
  end
  object edtClasaEditare: TdxDBImageEdit
    Left = 360
    Top = 72
    Width = 153
    TabOrder = 3
    DataField = 'CLASS_NAME'
    DataSource = DTColoana
  end
  object AtsDBSpinEdit1: TdxDBSpinEdit
    Left = 128
    Top = 128
    Width = 121
    TabOrder = 4
    DataField = 'MIN_WIDTH'
    DataSource = DTColoana
  end
  object AtsDBSpinEdit2: TdxDBSpinEdit
    Left = 392
    Top = 128
    Width = 121
    TabOrder = 5
    DataField = 'MAX_WIDTH'
    DataSource = DTColoana
  end
  object AtsDBSpinEdit3: TdxDBSpinEdit
    Left = 392
    Top = 188
    Width = 121
    TabOrder = 6
    DataField = 'FONT_SIZE'
    DataSource = DTColoana
  end
  object AtsDBImageEdit3: TdxDBImageEdit
    Left = 120
    Top = 220
    Width = 121
    TabOrder = 7
    DataField = 'ALIGN'
    DataSource = DTColoana
  end
  object AtsDBImageEdit4: TdxDBImageEdit
    Left = 376
    Top = 220
    Width = 137
    TabOrder = 8
    DataField = 'CAPTION_ALIGN'
    DataSource = DTColoana
  end
  object AtsDBCurrencyEdit1: TdxDBCurrencyEdit
    Left = 128
    Top = 275
    Width = 121
    TabOrder = 9
    DataSource = DTColoana
    Nullable = False
  end
  object AtsDBCurrencyEdit2: TdxDBCurrencyEdit
    Left = 384
    Top = 275
    Width = 121
    TabOrder = 10
    DataSource = DTColoana
    Nullable = False
  end
  object AtsDBCurrencyEdit3: TdxDBCurrencyEdit
    Left = 128
    Top = 309
    Width = 121
    TabOrder = 11
    Nullable = False
  end
  object AtsDBCheckEdit1: TdxDBCheckEdit
    Left = 144
    Top = 344
    Width = 73
    TabOrder = 12
    Caption = 'ReadOnly'
    DataField = 'READONLY'
    DataSource = DTColoana
    ValueChecked = 'True'
    ValueUnchecked = 'False'
  end
  object AtsDBCheckEdit2: TdxDBCheckEdit
    Left = 32
    Top = 344
    Width = 81
    TabOrder = 13
    Caption = 'Obligatoriu'
    DataField = 'REQUIRED'
    DataSource = DTColoana
    ValueChecked = 'True'
    ValueUnchecked = 'False'
  end
  object AtsDBCheckEdit3: TdxDBCheckEdit
    Left = 240
    Top = 344
    Width = 81
    TabOrder = 14
    Caption = 'Vizibil'
    DataField = 'VISIBLE'
    DataSource = DTColoana
    ValueChecked = 'True'
    ValueUnchecked = 'False'
  end
  object AtsDBButtonEdit1: TdxDBButtonEdit
    Left = 128
    Top = 384
    Width = 385
    TabOrder = 15
    DataField = 'FORMULA_CALCUL'
    DataSource = DTColoana
    Buttons = <
      item
        Default = True
      end>
    ExistButtons = True
  end
  object Button1: TcxButton
    Left = 528
    Top = 24
    Width = 75
    Height = 25
    Caption = 'Salvare'
    TabOrder = 16
    OnClick = Button1Click
  end
  object Button2: TcxButton
    Left = 528
    Top = 56
    Width = 75
    Height = 25
    Caption = 'Button2'
    TabOrder = 17
    Visible = False
  end
  object Button3: TcxButton
    Left = 528
    Top = 88
    Width = 75
    Height = 25
    Caption = 'Button3'
    TabOrder = 18
    Visible = False
  end
  object DTColoana: TDataSource
    Left = 480
    Top = 16
  end
end
