object frmIntretinereJudete: TfrmIntretinereJudete
  Left = 0
  Top = 0
  Caption = 'Nomenclator Judete'
  ClientHeight = 667
  ClientWidth = 1137
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object cxGrid1: TcxGrid
    Left = 0
    Top = 0
    Width = 1137
    Height = 667
    Align = alClient
    TabOrder = 0
    object cxGrid1DBTableView1: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsView.GroupByBox = False
      object cxGrid1DBTableView1Column1: TcxGridDBColumn
        Caption = 'ID Judete'
        DataBinding.FieldName = 'id_judete'
        Width = 272
      end
      object cxGrid1DBTableView1Column2: TcxGridDBColumn
        Caption = 'Denumire'
        DataBinding.FieldName = 'denumire'
        Width = 763
      end
    end
    object cxGrid1TableView1: TcxGridTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
    end
    object cxGrid1Level1: TcxGridLevel
      GridView = cxGrid1DBTableView1
    end
  end
  object ZQuery1: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT id_judete, denumire FROM JUDETE')
    Params = <>
    Left = 240
    Top = 176
  end
  object DataSource1: TDataSource
    DataSet = ZQuery1
    Left = 416
    Top = 160
  end
end
