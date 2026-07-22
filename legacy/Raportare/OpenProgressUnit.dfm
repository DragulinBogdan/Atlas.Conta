object FrmOpenProgress: TFrmOpenProgress
  Left = 361
  Top = 155
  BorderStyle = bsDialog
  Caption = 'Optinere Set de Date'
  ClientHeight = 160
  ClientWidth = 304
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  DesignSize = (
    304
    160)
  PixelsPerInch = 96
  TextHeight = 13
  object LbCurrent: TLabel
    Left = 11
    Top = 68
    Width = 3
    Height = 13
  end
  object LbTotal: TLabel
    Left = 13
    Top = 101
    Width = 3
    Height = 13
  end
  object ProgressTotal: TProgressBar
    Left = 10
    Top = 83
    Width = 281
    Height = 17
    TabOrder = 0
  end
  object Animatie: TAnimate
    Left = 15
    Top = 5
    Width = 272
    Height = 60
    CommonAVI = aviCopyFile
    StopFrame = 20
  end
  object ProgressPartial: TProgressBar
    Left = 11
    Top = 115
    Width = 281
    Height = 17
    TabOrder = 3
  end
  object BtnCancel: TcxButton
    Left = 115
    Top = 134
    Width = 75
    Height = 25
    Anchors = [akBottom]
    Caption = 'Abandon'
    TabOrder = 1
    OnClick = BtnCancelClick
  end
  object RefreshTime: TTimer
    OnTimer = RefreshTimeTimer
    Left = 8
    Top = 96
  end
end
