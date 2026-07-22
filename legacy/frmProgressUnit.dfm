object frmProgressRap: TfrmProgressRap
  Left = 408
  Top = 284
  BorderStyle = bsDialog
  Caption = 'Progres Situatie'
  ClientHeight = 108
  ClientWidth = 380
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = False
  Position = poScreenCenter
  OnShow = FormShow
  DesignSize = (
    380
    108)
  PixelsPerInch = 96
  TextHeight = 13
  object lbInfo: TcxLabel
    Left = 11
    Top = 8
    Anchors = [akLeft, akTop, akRight]
    AutoSize = False
    Properties.WordWrap = True
    Transparent = True
    Height = 33
    Width = 361
  end
  object progressBar: TcxProgressBar
    Left = 11
    Top = 47
    Anchors = [akLeft, akTop, akRight]
    TabOrder = 0
    Width = 361
  end
  object btnCancel: TcxButton
    Left = 156
    Top = 74
    Width = 75
    Height = 25
    Anchors = [akTop]
    Caption = 'Abandon'
    TabOrder = 2
    OnClick = btnCancelClick
  end
end
