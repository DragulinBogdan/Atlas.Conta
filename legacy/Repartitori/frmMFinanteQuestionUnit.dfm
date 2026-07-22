object frmMFinanteQuestion: TfrmMFinanteQuestion
  Left = 446
  Top = 271
  BorderStyle = bsDialog
  Caption = 'Captcha'
  ClientHeight = 172
  ClientWidth = 315
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
  object btnOk: TcxButton
    Left = 224
    Top = 125
    Width = 75
    Height = 40
    Caption = 'OK'
    ModalResult = 1
    TabOrder = 0
    LookAndFeel.Kind = lfOffice11
  end
  object edCaptcha: TcxTextEdit
    Left = 16
    Top = 136
    Style.LookAndFeel.Kind = lfOffice11
    StyleDisabled.LookAndFeel.Kind = lfOffice11
    StyleFocused.LookAndFeel.Kind = lfOffice11
    StyleHot.LookAndFeel.Kind = lfOffice11
    TabOrder = 1
    Width = 185
  end
  object edImage: TcxImage
    Left = 8
    Top = 8
    Properties.GraphicClassName = 'TJPEGImage'
    Properties.ReadOnly = True
    Style.LookAndFeel.Kind = lfOffice11
    StyleDisabled.LookAndFeel.Kind = lfOffice11
    StyleFocused.LookAndFeel.Kind = lfOffice11
    StyleHot.LookAndFeel.Kind = lfOffice11
    TabOrder = 2
    Height = 100
    Width = 297
  end
end
