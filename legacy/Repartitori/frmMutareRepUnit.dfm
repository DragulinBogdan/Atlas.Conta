object frmMutareRep: TfrmMutareRep
  Left = 281
  Top = 156
  Caption = 'Mutare repartitori'
  ClientHeight = 550
  ClientWidth = 812
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 509
    Width = 812
    Height = 41
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    ExplicitTop = 517
    ExplicitWidth = 820
  end
  object cxGridDate: TcxGrid
    Left = 0
    Top = 0
    Width = 812
    Height = 509
    Align = alClient
    TabOrder = 1
    LookAndFeel.Kind = lfOffice11
    ExplicitWidth = 820
    ExplicitHeight = 517
    object GridDate: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      DataController.DataSource = DSDate
      DataController.KeyFieldNames = 'RecID'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.IncSearch = True
      OptionsCustomize.ColumnsQuickCustomization = True
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GridLineColor = clSilver
      OptionsView.GroupByBox = False
      Preview.Column = GridDateexplicatie
      Preview.Visible = True
      object GridDateidRepartitor: TcxGridDBColumn
        Caption = 'Repartitor'
        DataBinding.FieldName = 'idRepartitor'
        Width = 99
      end
      object GridDateModul: TcxGridDBColumn
        DataBinding.FieldName = 'Modul'
        Width = 69
      end
      object GridDateDataTable: TcxGridDBColumn
        Caption = 'Tabela'
        DataBinding.FieldName = 'DataTable'
        Width = 122
      end
      object GridDateFieldName: TcxGridDBColumn
        Caption = 'Camp'
        DataBinding.FieldName = 'FieldName'
        Width = 63
      end
      object GridDateKeyField: TcxGridDBColumn
        Caption = 'Camp Keye primara'
        DataBinding.FieldName = 'KeyField'
        Width = 63
      end
      object GridDateKeyValue: TcxGridDBColumn
        Caption = 'Valoare cheie'
        DataBinding.FieldName = 'KeyValue'
        Width = 63
      end
      object GridDatedocument: TcxGridDBColumn
        Caption = 'Document'
        DataBinding.FieldName = 'document'
        Width = 159
      end
      object GridDateexplicatie: TcxGridDBColumn
        Caption = 'Explicatie'
        DataBinding.FieldName = 'explicatie'
        Width = 160
      end
    end
    object GridDateLevel1: TcxGridLevel
      GridView = GridDate
    end
  end
  object DSDate: TDataSource
    DataSet = qryDate
    Left = 72
    Top = 176
  end
  object MemDate: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 112
    Top = 176
  end
  object qryDate: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryDateAfterOpen
    SQL.Strings = (
      'exec spRepMutareDate :idRep')
    Params = <
      item
        DataType = ftInteger
        Name = 'idRep'
        ParamType = ptInput
        Value = '21695'
      end>
    Left = 144
    Top = 176
    ParamData = <
      item
        DataType = ftInteger
        Name = 'idRep'
        ParamType = ptInput
        Value = '21695'
      end>
  end
end
