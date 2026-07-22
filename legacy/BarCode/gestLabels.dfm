object frmGestLabels: TfrmGestLabels
  Left = 273
  Top = 108
  Caption = 'Label Coduri Bara'
  ClientHeight = 613
  ClientWidth = 862
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnForm: TPanel
    Left = 0
    Top = 0
    Width = 862
    Height = 564
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    BevelOuter = bvNone
    TabOrder = 0
    object cxGrid: TcxGrid
      Left = 0
      Top = 0
      Width = 862
      Height = 564
      Align = alClient
      TabOrder = 0
      object GridLabel: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        OnGetCellHeight = GridLabelGetCellHeight
        DataController.DataSource = DTLabels
        DataController.KeyFieldNames = 'id_gest_labels'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        object GridLabelid_gest_labels: TcxGridDBColumn
          Caption = 'Id'
          DataBinding.FieldName = 'id_gest_labels'
          Visible = False
        end
        object GridLabelthumb: TcxGridDBColumn
          DataBinding.FieldName = 'thumb'
          PropertiesClassName = 'TcxImageProperties'
          Width = 145
        end
        object GridLabeldenumire: TcxGridDBColumn
          Caption = 'Denumire'
          DataBinding.FieldName = 'denumire'
          Width = 427
        end
        object GridLabelsizeWidth: TcxGridDBColumn
          DataBinding.FieldName = 'sizeWidth'
          Visible = False
          Width = 93
        end
        object GridLabelsizeHeight: TcxGridDBColumn
          DataBinding.FieldName = 'sizeHeight'
          Visible = False
          Width = 127
        end
        object GridLabelreport: TcxGridDBColumn
          DataBinding.FieldName = 'report'
          PropertiesClassName = 'TcxButtonEditProperties'
          Properties.Buttons = <
            item
              Default = True
              Kind = bkEllipsis
            end>
          Width = 68
        end
      end
      object GridLabelV: TcxGridLevel
        GridView = GridLabel
      end
    end
  end
  object DTLabels: TDataSource
    DataSet = qryLabels
    Left = 254
    Top = 340
  end
  object qryLabels: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'set textsize 2560000'
      'select * from gest_labels')
    Params = <>
    Left = 284
    Top = 341
  end
end
