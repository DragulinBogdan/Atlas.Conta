object frmFisaDetaliu: TfrmFisaDetaliu
  Left = 283
  Top = 345
  AutoScroll = False
  Caption = 'Fisa detaliu'
  ClientHeight = 254
  ClientWidth = 719
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object cxGridFisaDetaliu: TcxGrid
    Left = 0
    Top = 20
    Width = 719
    Height = 234
    Align = alClient
    TabOrder = 1
    Visible = False
    LookAndFeel.Kind = lfOffice11
    LookAndFeel.NativeStyle = False
    object GridFisaDetaliu: TcxGridDBTableView
      NavigatorButtons.ConfirmDelete = False
      DataController.Filter.MaxValueListCount = 1000
      DataController.Filter.Options = [fcoCaseInsensitive]
      DataController.Options = [dcoAnsiSort, dcoCaseInsensitive, dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding]
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      Filtering.ColumnPopup.MaxDropDownItemCount = 12
      OptionsBehavior.ImmediateEditor = False
      OptionsBehavior.IncSearch = True
      OptionsBehavior.FocusCellOnCycle = True
      OptionsCustomize.ColumnsQuickCustomization = True
      OptionsData.CancelOnExit = False
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsSelection.HideFocusRectOnExit = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.Footer = True
      OptionsView.GroupByBox = False
      OptionsView.GroupFooters = gfVisibleWhenExpanded
      Preview.AutoHeight = False
      Preview.MaxLineCount = 2
    end
    object cxGridFisaDetaliuLevel1: TcxGridLevel
      GridView = GridFisaDetaliu
    end
  end
  object tabTipuriFise: TcxTabControl
    Left = 0
    Top = 0
    Width = 719
    Height = 20
    Align = alTop
    LookAndFeel.Kind = lfOffice11
    LookAndFeel.NativeStyle = False
    Style = 9
    TabOrder = 0
    TabSlants.Kind = skCutCorner
    TabSlants.Positions = [spLeft, spRight]
    OnChange = tabTipuriFiseChange
    ClientRectBottom = 20
    ClientRectRight = 719
    ClientRectTop = 0
  end
  object pnTree: TPanel
    Left = 0
    Top = 20
    Width = 719
    Height = 234
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 2
    object TreeDetaliu: TcxDBTreeList
      Left = 0
      Top = 28
      Width = 719
      Height = 206
      BorderStyle = cxcbsNone
      Align = alClient
      Bands = <>
      LookAndFeel.Kind = lfOffice11
      LookAndFeel.NativeStyle = False
      OptionsBehavior.IncSearch = True
      OptionsCustomizing.ColumnsQuickCustomization = True
      OptionsData.Editing = False
      OptionsData.AnsiSort = True
      OptionsData.Deleting = False
      OptionsView.Indicator = True
      RootValue = -1
      TabOrder = 0
    end
    object pnToolsTree: TPanel
      Left = 0
      Top = 0
      Width = 719
      Height = 28
      Align = alTop
      BevelInner = bvRaised
      BevelOuter = bvLowered
      TabOrder = 1
      object BtnExpandTree: TcxButton
        Left = 7
        Top = 3
        Width = 74
        Height = 22
        Caption = 'Extinde'
        TabOrder = 1
        OnClick = BtnExpandTreeClick
        Glyph.Data = {
          36040000424D3604000000000000360000002800000010000000100000000100
          2000000000000004000000000000000000000000000000000000FF00FF000000
          0000FF00FF00FF00FF00FF00FF0000000000FF00FF00FF00FF0000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00FF000000
          0000FF00FF00FF00FF00FF00FF0000000000000000000000000000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00FF000000
          0000FF00FF00FF00FF00FF00FF0000000000FF00FF00FF00FF0000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00FF000000
          0000FF00FF00FF00FF00FF00FF0000000000FF00FF00FF00FF00000000000000
          0000000000000000000000000000000000000000000000000000FF00FF000000
          0000FF00FF00FF00FF00FF00FF0000000000FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF0000000000000000000000000000000000000000000000
          000000000000000000000000000000000000FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF000000
          0000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF0000000000000000000000000000000000000000000000
          000000000000000000000000000000000000FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00000000000000
          0000000000000000000000000000000000000000000000000000000000000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF0000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF0000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF0000000000FFFF
          FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00000000000000
          0000000000000000000000000000000000000000000000000000000000000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
        LookAndFeel.Kind = lfOffice11
      end
      object BtnCollapseTree: TcxButton
        Left = 84
        Top = 3
        Width = 77
        Height = 22
        Caption = 'Restrange'
        TabOrder = 2
        OnClick = BtnCollapseTreeClick
        Glyph.Data = {
          36040000424D3604000000000000360000002800000010000000100000000100
          2000000000000004000000000000000000000000000000000000FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF000000000000000000000000000000000000000000000000000000
          0000000000000000000000000000FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          00000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF000000000000000000000000000000000000000000000000000000
          0000000000000000000000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF000000000000000000000000000000000000000000000000000000
          0000000000000000000000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          0000FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF000000
          00000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF0000000000FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF000000000000000000000000000000000000000000000000000000
          0000000000000000000000000000FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
          FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
        LookAndFeel.Kind = lfOffice11
      end
      object NiveleTree: TToolBar
        Left = 169
        Top = 4
        Width = 283
        Height = 20
        Align = alNone
        Caption = 'NiveleTree'
        EdgeBorders = [ebLeft]
        ShowCaptions = True
        TabOrder = 0
        Wrapable = False
      end
    end
  end
  object dsFisaMaterial: TDataSource
    DataSet = qryFisaDetaliu
    Left = 360
    Top = 112
  end
  object qryFisaDetaliu: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      ''
      
        ':id_gest_produs, :codmat, :PeCodMat, :IdGestTipStoc, :IdGestiune' +
        ', :LstTipMaterial, :Data')
    Params = <
      item
        DataType = ftUnknown
        Name = 'id_gest_produs'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'codmat'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'PeCodMat'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'IdGestTipStoc'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'IdGestiune'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'LstTipMaterial'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'Data'
        ParamType = ptUnknown
      end>
    Left = 392
    Top = 112
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'id_gest_produs'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'codmat'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'PeCodMat'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'IdGestTipStoc'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'IdGestiune'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'LstTipMaterial'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'Data'
        ParamType = ptUnknown
      end>
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = cxGridFisaDetaliu
    PopupMenus = <>
    Left = 256
    Top = 96
  end
end
