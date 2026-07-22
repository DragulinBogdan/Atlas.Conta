object frmAlopOrdVizualizare: TfrmAlopOrdVizualizare
  Left = 275
  Top = 131
  Caption = 'Ordonantari'
  ClientHeight = 482
  ClientWidth = 873
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object gbBox: TcxGroupBox
    Left = 0
    Top = 0
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    Caption = 'Lista Documente Lichidare'
    TabOrder = 0
    Height = 436
    Width = 873
    object gridDocumenteLichidare: TcxGrid
      Left = 2
      Top = 18
      Width = 869
      Height = 416
      Align = alClient
      TabOrder = 0
      object viewDocumenteLichidare: TcxGridDBBandedTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        Bands = <
          item
          end>
      end
      object nivelDocumenteLichidare: TcxGridLevel
        GridView = viewDocumenteLichidare
      end
    end
  end
  object qryListaOrd: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec sp')
    Params = <>
    Left = 80
    Top = 112
  end
  object DTListaOrd: TDataSource
    DataSet = qryListaOrd
    Left = 48
    Top = 112
  end
end
