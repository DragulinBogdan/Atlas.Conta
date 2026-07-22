object frmAnexeCentralizare: TfrmAnexeCentralizare
  Left = 273
  Top = 138
  Caption = 'Centralizare Anexe'
  ClientHeight = 613
  ClientWidth = 862
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object cxGridAnexa: TcxGrid
    Left = 0
    Top = 73
    Width = 862
    Height = 540
    Align = alClient
    TabOrder = 1
    object cxGridAnexaDBTableView1: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      object cxGridAnexaDBTableView1Column1: TcxGridDBColumn
      end
    end
    object GridAnexa: TcxGridDBBandedTableView
      Navigator.Buttons.CustomButtons = <>
      OnCustomDrawCell = GridAnexaCustomDrawCell
      DataController.DataSource = DTAnexe
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.IncSearch = True
      OptionsView.CellAutoHeight = True
      OptionsView.GroupByBox = False
      Bands = <
        item
          Width = 306
        end>
      object GridAnexaColumn1: TcxGridDBBandedColumn
        Caption = '123'
        PropertiesClassName = 'TcxCheckBoxProperties'
        Properties.Alignment = taCenter
        HeaderAlignmentHorz = taCenter
        Styles.Header = frmData.cxStyle46
        Position.BandIndex = 0
        Position.ColIndex = 0
        Position.RowIndex = 0
      end
    end
    object GridAnexaL: TcxGridLevel
      GridView = GridAnexa
    end
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 862
    Height = 73
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    object Label1: TLabel
      Left = 16
      Top = 8
      Width = 98
      Height = 13
      Caption = 'Selectie Anexa : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 423
      Top = 8
      Width = 104
      Height = 13
      Caption = 'Perioada fiscala : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label3: TLabel
      Left = 423
      Top = 31
      Width = 102
      Height = 13
      Caption = 'Mod Introducere :'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edtAnexa: TcxImageComboBox
      Tag = -1
      Left = 115
      Top = 3
      Properties.Items = <>
      Properties.OnChange = edtAnexaPropertiesChange
      TabOrder = 0
      Width = 217
    end
    object edtPerioada: TcxImageComboBox
      Tag = -1
      Left = 528
      Top = 3
      Properties.Items = <>
      Properties.OnChange = edtPerioadaPropertiesChange
      TabOrder = 1
      Width = 303
    end
    object edNrZerouri: TcxImageComboBox
      Left = 528
      Top = 26
      EditValue = '1'
      Properties.Items = <
        item
          Description = 'Leu'
          ImageIndex = 0
          Value = '1'
        end
        item
          Description = 'Zeci de lei'
          ImageIndex = 1
          Value = '10'
        end
        item
          Description = 'Sute de lei'
          ImageIndex = 2
          Value = '100'
        end
        item
          Description = 'Mii lei'
          ImageIndex = 3
          Value = '1000'
        end
        item
          Description = 'Zeci de mii'
          ImageIndex = 4
          Value = '10000'
        end
        item
          Description = 'Sute de mii'
          ImageIndex = 5
          Value = '100000'
        end
        item
          Description = 'Milioane'
          ImageIndex = 6
          Value = '1000000'
        end>
      Properties.OnChange = edNrZerouriPropertiesChange
      TabOrder = 2
      Width = 97
    end
    object edZerouri: TcxSpinEdit
      Left = 628
      Top = 26
      Properties.OnChange = edZerouriPropertiesChange
      TabOrder = 3
      Value = 1
      Width = 68
    end
  end
  object DTAnexe: TDataSource
    DataSet = QryAnexe
    Left = 600
    Top = 576
  end
  object MemAnexe: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 632
    Top = 576
  end
  object QryAnexe: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spLstDateAnexeCumulare :idAnexa, :idPerioadeFiscale, :multi' +
        'plicator'
      '')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idAnexa'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idPerioadeFiscale'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'multiplicator'
        ParamType = ptUnknown
      end>
    Left = 664
    Top = 576
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idAnexa'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idPerioadeFiscale'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'multiplicator'
        ParamType = ptUnknown
      end>
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = cxGridAnexa
    PopupMenus = <>
    Left = 504
    Top = 496
  end
end
