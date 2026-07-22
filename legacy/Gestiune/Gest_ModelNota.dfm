object frmGestModelNota: TfrmGestModelNota
  Left = 343
  Top = 331
  AutoScroll = False
  Caption = 'Model Nota'
  ClientHeight = 302
  ClientWidth = 583
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object cxGridModel: TcxGrid
    Left = 0
    Top = 0
    Width = 583
    Height = 302
    Align = alClient
    TabOrder = 0
    object GridModel: TcxGridDBTableView
      NavigatorButtons.ConfirmDelete = False
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
    end
    object cxGridModelL: TcxGridLevel
      GridView = GridModel
    end
  end
end
