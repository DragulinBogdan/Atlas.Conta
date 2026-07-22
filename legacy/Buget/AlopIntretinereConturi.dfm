object frmAlopIntretinereCont: TfrmAlopIntretinereCont
  Left = 271
  Top = 141
  Caption = 'Intretinere Conturi ALOP'
  ClientHeight = 456
  ClientWidth = 702
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnClose = FormClose
  OnCreate = FormCreate
  DesignSize = (
    702
    456)
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 702
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Intretinere Conturi ALOP'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 0
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object pnContent: TPanel
    Left = 0
    Top = 41
    Width = 702
    Height = 381
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 1
    object TabControl: TcxTabControl
      Left = 0
      Top = 0
      Width = 702
      Height = 21
      Align = alTop
      Color = clSkyBlue
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentColor = False
      ParentFont = False
      TabOrder = 0
      Properties.CustomButtons.Buttons = <>
      Properties.Style = 9
      Properties.TabIndex = 0
      Properties.Tabs.Strings = (
        'Conturi Lichidare(furnizori, salarii... )'
        'Conturi Executie Plati'
        'Conturi Executie Cheltuieli'
        'Conturi Executie Venituri')
      Properties.TabSlants.Kind = skCutCorner
      Properties.TabSlants.Positions = [spLeft, spRight]
      LookAndFeel.Kind = lfOffice11
      TabSlants.Kind = skCutCorner
      TabSlants.Positions = [spLeft, spRight]
      OnChange = TabControlChange
      ClientRectRight = 0
      ClientRectTop = 0
    end
    object cxGridConturi: TcxGrid
      Left = 0
      Top = 21
      Width = 702
      Height = 360
      Align = alClient
      TabOrder = 1
      LookAndFeel.Kind = lfOffice11
      ExplicitHeight = 292
      object GridConturi: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        DataController.DataSource = DTConturi
        DataController.KeyFieldNames = 'id'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsData.Deleting = False
        OptionsData.DeletingConfirmation = False
        OptionsData.Editing = False
        OptionsData.Inserting = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        Styles.Header = frmData.cxStyle7
        object GridConturiid: TcxGridDBColumn
          Caption = 'Id'
          DataBinding.FieldName = 'id'
          Visible = False
        end
        object GridConturicont: TcxGridDBColumn
          Caption = 'Cont'
          DataBinding.FieldName = 'cont'
          HeaderAlignmentHorz = taCenter
          Width = 142
        end
        object GridConturiromana: TcxGridDBColumn
          Caption = 'Denumire Cont'
          DataBinding.FieldName = 'romana'
          HeaderAlignmentHorz = taCenter
          Width = 443
        end
      end
      object cxGridConturiL: TcxGridLevel
        GridView = GridConturi
      end
    end
  end
  object btnAdd: TcxButton
    Left = 6
    Top = 426
    Width = 75
    Height = 25
    Anchors = [akLeft, akBottom]
    Caption = 'Adauga'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 2
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
    OnClick = btnAddClick
  end
  object btnDel: TcxButton
    Left = 86
    Top = 426
    Width = 75
    Height = 25
    Anchors = [akLeft, akBottom]
    Caption = 'Sterge'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 3
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
    OnClick = btnDelClick
  end
  object DTConturi: TDataSource
    DataSet = qryConturi
    Left = 24
    Top = 88
  end
  object qryConturi: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spAlopConturi :tip')
    Params = <
      item
        DataType = ftUnknown
        Name = 'tip'
        ParamType = ptUnknown
      end>
    Left = 56
    Top = 88
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'tip'
        ParamType = ptUnknown
      end>
  end
end
