object frmIntretinAnexeParametrii: TfrmIntretinAnexeParametrii
  Left = 429
  Top = 176
  Caption = 'Intretinere parametri'
  ClientHeight = 440
  ClientWidth = 717
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnClose = FormClose
  OnShow = FormShow
  DesignSize = (
    717
    440)
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 725
    Height = 411
    Anchors = [akLeft, akTop, akRight, akBottom]
    BevelOuter = bvNone
    TabOrder = 0
    object Panel2: TPanel
      Left = 0
      Top = 208
      Width = 725
      Height = 203
      Align = alBottom
      BevelOuter = bvNone
      TabOrder = 0
      object vgrid: TcxDBVerticalGrid
        Left = 0
        Top = 0
        Width = 420
        Height = 203
        Align = alClient
        LookAndFeel.Kind = lfOffice11
        OptionsView.ScrollBars = ssVertical
        OptionsView.RowHeaderWidth = 102
        OptionsData.Appending = False
        OptionsData.Deleting = False
        OptionsData.Inserting = False
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        DataController.DataSource = DTParam
        Version = 1
        object vgridSQLText: TcxDBEditorRow
          Properties.DataBinding.FieldName = 'SQLText'
          ID = 0
          ParentID = -1
          Index = 0
          Version = 1
        end
        object vgridValueList: TcxDBEditorRow
          Properties.Caption = 'Lista valori'
          Properties.DataBinding.FieldName = 'ValueList'
          ID = 1
          ParentID = -1
          Index = 1
          Version = 1
        end
        object vgridDescriptionList: TcxDBEditorRow
          Properties.Caption = 'Lista descrieri'
          Properties.DataBinding.FieldName = 'DescriptionList'
          ID = 2
          ParentID = -1
          Index = 2
          Version = 1
        end
        object vgridSourceTable: TcxDBEditorRow
          Properties.Caption = 'Tabela sursa'
          Properties.DataBinding.FieldName = 'SourceTable'
          ID = 3
          ParentID = -1
          Index = 3
          Version = 1
        end
        object vgridKeyField: TcxDBEditorRow
          Properties.Caption = 'Camp cheie'
          Properties.DataBinding.FieldName = 'KeyField'
          ID = 4
          ParentID = -1
          Index = 4
          Version = 1
        end
        object vgridIDField: TcxDBEditorRow
          Properties.Caption = 'Camp ID'
          Properties.DataBinding.FieldName = 'IDField'
          ID = 5
          ParentID = -1
          Index = 5
          Version = 1
        end
        object vgridParentField: TcxDBEditorRow
          Properties.Caption = 'Camp parinte'
          Properties.DataBinding.FieldName = 'ParentField'
          ID = 6
          ParentID = -1
          Index = 6
          Version = 1
        end
        object vgridDisplayField: TcxDBEditorRow
          Properties.Caption = 'Camp afisare'
          Properties.DataBinding.FieldName = 'DisplayField'
          ID = 7
          ParentID = -1
          Index = 7
          Version = 1
        end
        object vgridFieldList: TcxDBEditorRow
          Properties.Caption = 'Lista coloane'
          Properties.DataBinding.FieldName = 'FieldList'
          ID = 8
          ParentID = -1
          Index = 8
          Version = 1
        end
        object vgridColumnAutoWidth: TcxDBEditorRow
          Properties.Caption = 'Latime automata pentru coloane'
          Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
          Properties.DataBinding.FieldName = 'ColumnAutoWidth'
          ID = 9
          ParentID = -1
          Index = 9
          Version = 1
        end
        object vgridConltrolWidth: TcxDBEditorRow
          Properties.Caption = 'Latime popup'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.DisplayFormat = '#0'
          Properties.DataBinding.FieldName = 'ControlWidth'
          ID = 10
          ParentID = -1
          Index = 10
          Version = 1
        end
      end
      object grdCols: TcxGrid
        Left = 428
        Top = 0
        Width = 297
        Height = 203
        Align = alRight
        TabOrder = 1
        LookAndFeel.Kind = lfOffice11
        object cxGridDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = DTPopupCols
          DataController.KeyFieldNames = 'ID_ANEXE_PARAMETRII_COLOANE'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          object cxGridDBTableView1ColName: TcxGridDBColumn
            Caption = 'Nume coloana'
            DataBinding.FieldName = 'ColName'
            Width = 182
          end
          object cxGridDBTableView1ColWidth: TcxGridDBColumn
            Caption = 'Latime'
            DataBinding.FieldName = 'ColWidth'
            Width = 113
          end
        end
        object cxGridLevel1: TcxGridLevel
          GridView = cxGridDBTableView1
        end
      end
      object spCols: TcxSplitter
        Left = 420
        Top = 0
        Width = 8
        Height = 203
        HotZoneClassName = 'TcxMediaPlayer9Style'
        AlignSplitter = salRight
        Control = grdCols
      end
    end
    object grd: TcxGrid
      Left = 0
      Top = 29
      Width = 725
      Height = 171
      Align = alClient
      TabOrder = 1
      LookAndFeel.Kind = lfOffice11
      object tvParam: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        OnFocusedRecordChanged = tvParamFocusedRecordChanged
        DataController.DataSource = DTParam
        DataController.KeyFieldNames = 'ID_ANEXE_PARAMETRII'
        DataController.Options = [dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding, dcoImmediatePost]
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        object tvParamParamName: TcxGridDBColumn
          Caption = 'Nume parametru'
          DataBinding.FieldName = 'ParamName'
          Width = 119
        end
        object tvParamCaption: TcxGridDBColumn
          Caption = 'Alias parametru'
          DataBinding.FieldName = 'Caption'
          Width = 155
        end
        object tvParamParamType: TcxGridDBColumn
          Caption = 'Tip parametru'
          DataBinding.FieldName = 'ParamType'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.ImmediatePost = True
          Properties.Items = <
            item
              Description = '[Default]'
              ImageIndex = 0
              Value = '0'
            end
            item
              Description = 'TextEdit'
              Value = '1'
            end
            item
              Description = 'ImageComboBox'
              Value = '2'
            end
            item
              Description = 'ComboBox'
              Value = '3'
            end
            item
              Description = 'PopupEdit'
              Value = '4'
            end
            item
              Description = 'CurrencyEdit'
              Value = '5'
            end
            item
              Description = 'SpinEdit'
              Value = '6'
            end
            item
              Description = 'DateEdit'
              Value = '7'
            end
            item
              Description = 'TimeEdit'
              Value = '8'
            end
            item
              Description = 'CheckBox'
              Value = '9'
            end
            item
              Description = 'LookupComboBox'
              Value = '10'
            end
            item
              Description = 'AnLuna'
              Value = '11'
            end>
          Properties.OnCloseUp = tvParamParamTypePropertiesCloseUp
          Width = 130
        end
        object tvParamDescription: TcxGridDBColumn
          Caption = 'Descriere parametru'
          DataBinding.FieldName = 'Description'
          Width = 232
        end
      end
      object grdLevel1: TcxGridLevel
        GridView = tvParam
      end
    end
    object cxSplitter1: TcxSplitter
      Left = 0
      Top = 200
      Width = 725
      Height = 8
      HotZoneClassName = 'TcxMediaPlayer9Style'
      AlignSplitter = salBottom
      Control = Panel2
    end
    object Panel3: TPanel
      Left = 0
      Top = 0
      Width = 725
      Height = 29
      Align = alTop
      BevelOuter = bvNone
      TabOrder = 3
      object SpeedButton1: TSpeedButton
        Left = 2
        Top = 3
        Width = 120
        Height = 22
        Caption = 'Adauga parametru'
        Flat = True
        Glyph.Data = {
          F6000000424DF600000000000000360000002800000008000000080000000100
          180000000000C000000000000000000000000000000000000000FFFFFFFFFFFF
          FFFFFFFF0000FF0000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000FF00
          00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000FF0000FFFFFFFFFFFFFF
          FFFFFF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000
          FF0000FF0000FF0000FF0000FF0000FF0000FFFFFFFFFFFFFFFFFFFF0000FF00
          00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000FF0000FFFFFFFFFFFFFF
          FFFFFFFFFFFFFFFFFFFFFFFF0000FF0000FFFFFFFFFFFFFFFFFF}
        OnClick = SpeedButton1Click
      end
      object SpeedButton2: TSpeedButton
        Left = 125
        Top = 3
        Width = 120
        Height = 22
        Caption = 'Sterge parametru'
        Flat = True
        Glyph.Data = {
          F6000000424DF600000000000000360000002800000008000000080000000100
          180000000000C000000000000000000000000000000000000000FFFFFFFFFFFF
          FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
          FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
          FFFF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF
          0000FF0000FF0000FF0000FF0000FF0000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
          FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
          FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF}
        OnClick = SpeedButton2Click
      end
    end
  end
  object cxButton1: TcxButton
    Left = 622
    Top = 413
    Width = 90
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'OK'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D560A00000000000036000000280000002400000012000000010020000000
      000000000000C40E0000C40E0000000000000000000000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000808000FFFFFFFF00808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000800000FF800000FF0080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000808000008080000080800000808000808080FF808080FFFFFFFFFF0080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000008080000080800000808000800000FF008000FF0080
      00FF800000FF0080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080008080
      80FF0080800000808000808080FFFFFFFFFF0080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000800000FF008000FF008000FF008000FF008000FF800000FF008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000808080FF008080000080800000808000008080008080
      80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
      8000008080000080800000808000800000FF008000FF008000FF008000FF0080
      00FF008000FF008000FF800000FF008080000080800000808000008080000080
      80000080800000808000008080000080800000808000808080FF008080000080
      800000808000008080000080800000808000808080FFFFFFFFFF008080000080
      8000008080000080800000808000008080000080800000808000800000FF0080
      00FF008000FF008000FF00FF00FF008000FF008000FF008000FF008000FF8000
      00FF008080000080800000808000008080000080800000808000008080000080
      8000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF008080000080
      800000808000808080FFFFFFFFFF008080000080800000808000008080000080
      80000080800000808000008000FF008000FF008000FF00FF00FF0080800000FF
      00FF008000FF008000FF008000FF800000FF0080800000808000008080000080
      800000808000008080000080800000808000808080FFFFFFFFFF008080008080
      80FF00808000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF0080
      800000808000008080000080800000808000008080000080800000FF00FF0080
      00FF00FF00FF00808000008080000080800000FF00FF008000FF008000FF0080
      00FF800000FF0080800000808000008080000080800000808000008080000080
      8000808080FFFFFFFFFF808080FF008080000080800000808000808080FFFFFF
      FFFF0080800000808000808080FFFFFFFFFF0080800000808000008080000080
      800000808000008080000080800000FF00FF0080800000808000008080000080
      80000080800000FF00FF008000FF008000FF008000FF800000FF008080000080
      80000080800000808000008080000080800000808000808080FF008080000080
      8000008080000080800000808000808080FFFFFFFFFF00808000008080008080
      80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
      800000808000008080000080800000808000008080000080800000FF00FF0080
      00FF008000FF008000FF800000FF008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000808000008080000080800000FF00FF008000FF008000FF008000FF8000
      00FF008080000080800000808000008080000080800000808000008080000080
      80000080800000808000008080000080800000808000808080FFFFFFFFFF0080
      800000808000808080FFFFFFFFFF008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000FF00FF008000FF008000FF008000FF800000FF00808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000808080FFFFFFFFFF0080800000808000808080FFFFFF
      FFFF008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000FF00FF008000FF0080
      00FF008000FF800000FF00808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080008080
      80FFFFFFFFFF0080800000808000808080FFFFFFFFFF00808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000FF00FF008000FF008000FF800000FF008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000808000008080000080800000808000808080FFFFFFFFFF008080008080
      80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
      80000080800000808000008080000080800000808000008080000080800000FF
      00FF008000FF008000FF00808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      800000808000808080FFFFFFFFFF808080FF0080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000008080000080800000FF00FF00808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000808080FF0080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      8000008080000080800000808000008080000080800000808000008080000080
      80000080800000808000008080000080800000808000}
    OptionsImage.NumGlyphs = 2
    TabOrder = 1
    OnClick = cxButton1Click
  end
  object qryParam: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from ANEXE_PARAMETRII')
    Params = <>
    Left = 8
    Top = 72
  end
  object DTParam: TDataSource
    DataSet = qryParam
    Left = 40
    Top = 72
  end
  object qryPopupCols: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = qryPopupColsNewRecord
    SQL.Strings = (
      
        'select * from ANEXE_PARAMETRII_COLOANE where ID_ANEXE_PARAMETRII' +
        ' = :ID_ANEXE_PARAMETRII')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANEXE_PARAMETRII'
        ParamType = ptUnknown
        Value = 1
      end>
    DataSource = DTParam
    Left = 8
    Top = 104
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANEXE_PARAMETRII'
        ParamType = ptUnknown
        Value = 1
      end>
  end
  object DTPopupCols: TDataSource
    DataSet = qryPopupCols
    Left = 40
    Top = 104
  end
end
