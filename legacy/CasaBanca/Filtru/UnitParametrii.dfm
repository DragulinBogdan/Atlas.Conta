object FrmParametrii: TFrmParametrii
  Left = 379
  Top = 179
  BorderStyle = bsDialog
  Caption = 'Parametrii Pentru Functia : '
  ClientHeight = 254
  ClientWidth = 431
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  PixelsPerInch = 96
  TextHeight = 13
  object LbInfo: TLabel
    Left = 8
    Top = 8
    Width = 225
    Height = 13
    Caption = 'Functia Aleasa %s Are Nevoie De %d Parametrii'
  end
  object LbParam1: TLabel
    Left = 62
    Top = 106
    Width = 65
    Height = 13
    Caption = 'Parametrul 1 :'
    Visible = False
  end
  object LbParam2: TLabel
    Left = 62
    Top = 134
    Width = 65
    Height = 13
    Caption = 'Parametrul 2 :'
    Visible = False
  end
  object LbParam3: TLabel
    Left = 62
    Top = 164
    Width = 65
    Height = 13
    Caption = 'Parametrul 3 :'
    Visible = False
  end
  object LbParam4: TLabel
    Left = 62
    Top = 192
    Width = 65
    Height = 13
    Caption = 'Parametrul 4 :'
    Visible = False
  end
  object BtnOk: TcxButton
    Left = 291
    Top = 223
    Width = 65
    Height = 26
    Caption = 'Ok'
    ModalResult = 1
    TabOrder = 5
    Visible = False
  end
  object RxSpeedButton1: TcxButton
    Left = 362
    Top = 223
    Width = 65
    Height = 26
    Caption = 'Cancel'
    ModalResult = 2
    TabOrder = 6
  end
  object Panel1: TPanel
    Left = 94
    Top = 24
    Width = 321
    Height = 57
    BevelOuter = bvNone
    TabOrder = 4
    object Label1: TLabel
      Left = 16
      Top = 8
      Width = 244
      Height = 26
      Caption = 
        'Introducerea parametrilor este o operatie obligatorie '#13#10'pentru b' +
        'una functionare a programului'
    end
  end
  object EditParam1: TcxButtonEdit
    Left = 132
    Top = 102
    Properties.Buttons = <
      item
        Default = True
        Kind = bkEllipsis
      end>
    Properties.OnButtonClick = EditParam1PropertiesButtonClick
    Properties.OnChange = EditParam1Change
    TabOrder = 0
    Visible = False
    Width = 289
  end
  object EditParam2: TcxButtonEdit
    Left = 132
    Top = 130
    Properties.Buttons = <
      item
        Default = True
        Kind = bkEllipsis
      end>
    Properties.OnButtonClick = EditParam1PropertiesButtonClick
    Properties.OnChange = EditParam1Change
    TabOrder = 1
    Visible = False
    Width = 289
  end
  object EditParam3: TcxButtonEdit
    Left = 132
    Top = 160
    Properties.Buttons = <
      item
        Default = True
        Kind = bkEllipsis
      end>
    Properties.OnButtonClick = EditParam1PropertiesButtonClick
    Properties.OnChange = EditParam1Change
    TabOrder = 2
    Visible = False
    Width = 289
  end
  object EditParam4: TcxButtonEdit
    Left = 132
    Top = 188
    Properties.Buttons = <
      item
        Default = True
        Kind = bkEllipsis
      end>
    Properties.OnButtonClick = EditParam1PropertiesButtonClick
    Properties.OnChange = EditParam1Change
    TabOrder = 3
    Visible = False
    Width = 289
  end
end
