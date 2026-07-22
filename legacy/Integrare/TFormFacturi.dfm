object Form2: TForm2
  Left = 0
  Top = 0
  Caption = 'Facturi'
  ClientHeight = 787
  ClientWidth = 979
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object cxGrid1: TcxGrid
    Left = 16
    Top = 8
    Width = 937
    Height = 657
    Align = alCustom
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 0
    object cxGrid1DBTableView1: TcxGridDBTableView
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
  object btnValidareXMLClick: TButton
    Left = 16
    Top = 746
    Width = 113
    Height = 41
    Caption = 'Validare XML'
    TabOrder = 1
    OnClick = btnValidareXMLClickClick
  end
  object btnTransformaInPDF: TcxButton
    Left = 160
    Top = 746
    Width = 89
    Height = 41
    Caption = 'XML IN PDF'
    TabOrder = 2
    OnClick = btnTransformaInPDFClick
  end
  object btnGenereazaXML: TButton
    Left = 280
    Top = 746
    Width = 97
    Height = 41
    Caption = 'Genereaza XML'
    TabOrder = 3
  end
  object ZQuery1: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT '
      '  d.id_gest_docum,'
      '  d.nr_docum AS [Num'#259'r Factur'#259'],'
      '  d.data_docum AS [Dat'#259'],'
      '  f.NUME AS [Client],'
      '  d.totaldoc AS [Total RON]'
      'FROM gest_docum d'
      
        'JOIN gest_defa_docum c ON c.id_gest_defa_docum = d.id_gest_defa_' +
        'docum'
      
        'JOIN gest_tip_docum t ON t.id_gest_tip_docum = c.id_gest_tip_doc' +
        'um'
      'JOIN repartitori f ON f.id_repartitori = d.id_primitor'
      'WHERE d.stare = 1 AND t.cod_docum = '#39'FCT'#39
      'ORDER BY d.data_docum DESC')
    Params = <>
    Left = 24
    Top = 512
  end
  object DataSource1: TDataSource
    DataSet = ZQuery1
    Left = 64
    Top = 512
  end
  object OpenDialogXML: TOpenDialog
    Left = 56
    Top = 704
  end
  object SaveDialogPDF: TSaveDialog
    DefaultExt = 'pdf'
    Filter = 'PDF files (*.pdf)|*.pdf'
    Left = 192
    Top = 704
  end
  object SaveDialog1: TSaveDialog
    DefaultExt = 'xml'
    Filter = 'xml|XML Files (*.xml)|*.xml'
    Title = 'Salveaza XML Factura'
    Left = 312
    Top = 688
  end
end
