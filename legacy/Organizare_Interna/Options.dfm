object OptionsForm: TOptionsForm
  Left = 351
  Top = 244
  BorderStyle = bsDialog
  Caption = 'Express OrgChart Options'
  ClientHeight = 208
  ClientWidth = 316
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  Position = poScreenCenter
  OnActivate = FormActivate
  PixelsPerInch = 96
  TextHeight = 13
  object Bevel1: TBevel
    Left = 0
    Top = 0
    Width = 313
    Height = 173
    Shape = bsFrame
  end
  object Label1: TLabel
    Left = 192
    Top = 16
    Width = 40
    Height = 13
    Caption = 'Indent X'
  end
  object Label2: TLabel
    Left = 192
    Top = 44
    Width = 40
    Height = 13
    Caption = 'Indent Y'
  end
  object Label3: TLabel
    Left = 192
    Top = 72
    Width = 48
    Height = 13
    Caption = 'Line width'
  end
  object GroupBox1: TGroupBox
    Left = 8
    Top = 8
    Width = 177
    Height = 85
    Caption = ' Edit Mode '
    TabOrder = 0
    object cbLeft: TCheckBox
      Left = 8
      Top = 16
      Width = 49
      Height = 17
      Caption = 'Left'
      TabOrder = 0
    end
    object cbCenter: TCheckBox
      Left = 8
      Top = 32
      Width = 53
      Height = 17
      Caption = 'Center'
      TabOrder = 1
    end
    object cbRight: TCheckBox
      Left = 8
      Top = 48
      Width = 53
      Height = 17
      Caption = 'Right'
      TabOrder = 2
    end
    object cbVCenter: TCheckBox
      Left = 8
      Top = 64
      Width = 77
      Height = 17
      Caption = 'Vert Center'
      TabOrder = 3
    end
    object cbWrap: TCheckBox
      Left = 100
      Top = 16
      Width = 53
      Height = 17
      Caption = 'Wrap'
      TabOrder = 4
    end
    object cbUpper: TCheckBox
      Left = 100
      Top = 32
      Width = 53
      Height = 17
      Caption = 'Upper'
      TabOrder = 5
    end
    object cbLower: TCheckBox
      Left = 100
      Top = 48
      Width = 53
      Height = 17
      Caption = 'Lower'
      TabOrder = 6
    end
    object cbGrow: TCheckBox
      Left = 100
      Top = 64
      Width = 49
      Height = 17
      Caption = 'Grow'
      TabOrder = 7
    end
  end
  object seX: TSpinEdit
    Left = 244
    Top = 12
    Width = 61
    Height = 22
    MaxValue = 0
    MinValue = 0
    TabOrder = 1
    Value = 0
    OnChange = seLineWidthChange
  end
  object seY: TSpinEdit
    Left = 244
    Top = 40
    Width = 61
    Height = 22
    MaxValue = 0
    MinValue = 0
    TabOrder = 2
    Value = 0
    OnChange = seLineWidthChange
  end
  object seLineWidth: TSpinEdit
    Left = 244
    Top = 68
    Width = 61
    Height = 22
    MaxValue = 0
    MinValue = 0
    TabOrder = 3
    Value = 0
    OnChange = seLineWidthChange
  end
  object BitBtn1: TBitBtn
    Left = 240
    Top = 176
    Width = 75
    Height = 25
    TabOrder = 4
    Kind = bkCancel
  end
  object BitBtn2: TBitBtn
    Left = 160
    Top = 176
    Width = 75
    Height = 25
    TabOrder = 5
    OnClick = BitBtn2Click
    Kind = bkOK
  end
  object cbSelect: TCheckBox
    Left = 16
    Top = 104
    Width = 89
    Height = 17
    Caption = 'Show Select'
    TabOrder = 6
  end
  object cbFocus: TCheckBox
    Left = 16
    Top = 120
    Width = 89
    Height = 17
    Caption = 'Show Focus'
    TabOrder = 7
  end
  object cbButtons: TCheckBox
    Left = 16
    Top = 136
    Width = 89
    Height = 17
    Caption = 'Show Buttons'
    TabOrder = 8
  end
  object cbCanDrag: TCheckBox
    Left = 108
    Top = 104
    Width = 77
    Height = 17
    Caption = 'Can Drag'
    TabOrder = 9
  end
  object cbShowDrag: TCheckBox
    Left = 108
    Top = 120
    Width = 81
    Height = 17
    Caption = 'Show Drag'
    TabOrder = 10
  end
  object cbInsDel: TCheckBox
    Left = 108
    Top = 136
    Width = 85
    Height = 17
    Caption = 'Insert, Delete'
    TabOrder = 11
  end
  object cbEdit: TCheckBox
    Left = 16
    Top = 152
    Width = 81
    Height = 17
    Caption = 'Edit'
    TabOrder = 12
  end
  object cbShowImages: TCheckBox
    Left = 108
    Top = 152
    Width = 89
    Height = 17
    Caption = 'Show Images'
    TabOrder = 13
  end
end
