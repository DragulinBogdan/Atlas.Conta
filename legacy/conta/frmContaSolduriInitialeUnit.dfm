object frmContaSolduriInitiale: TfrmContaSolduriInitiale
  Left = 218
  Top = 225
  AutoScroll = False
  Caption = 'Solduri initiale'
  ClientHeight = 528
  ClientWidth = 650
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
  object gridLstSolduri: TcxGrid
    Left = 0
    Top = 41
    Width = 650
    Height = 258
    Align = alClient
    TabOrder = 0
    LookAndFeel.Kind = lfOffice11
    object gridViewSolduri: TcxGridDBTableView
      NavigatorButtons.ConfirmDelete = False
      DataController.DataSource = DTSolduriComplet
      DataController.Filter.Options = [fcoCaseInsensitive]
      DataController.KeyFieldNames = 'RecId'
      DataController.Options = [dcoAnsiSort, dcoCaseInsensitive, dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding]
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <
        item
          Kind = skSum
          FieldName = 'sold_creditor'
          Column = gridViewSoldurisold_creditor
        end
        item
          Kind = skSum
          FieldName = 'sold_debitor'
          Column = gridViewSoldurisold_debitor
        end>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.IncSearch = True
      OptionsCustomize.ColumnsQuickCustomization = True
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.Footer = True
      OptionsView.GroupByBox = False
      object gridViewSolduricont: TcxGridDBColumn
        Caption = 'Cont'
        DataBinding.FieldName = 'cont'
        HeaderAlignmentHorz = taCenter
        Width = 152
      end
      object gridViewSoldurinume: TcxGridDBColumn
        Caption = 'Nume'
        DataBinding.FieldName = 'nume'
        HeaderAlignmentHorz = taCenter
        Options.Editing = False
        Width = 208
      end
      object gridViewSoldurian: TcxGridDBColumn
        Caption = 'An'
        DataBinding.FieldName = 'an'
        Visible = False
        HeaderAlignmentHorz = taCenter
        Width = 75
      end
      object gridViewSoldurisold_debitor: TcxGridDBColumn
        Caption = 'Sold Debitor'
        DataBinding.FieldName = 'sold_debitor'
        HeaderAlignmentHorz = taCenter
        Width = 131
      end
      object gridViewSoldurisold_creditor: TcxGridDBColumn
        Caption = 'Sold Creditor'
        DataBinding.FieldName = 'sold_creditor'
        HeaderAlignmentHorz = taCenter
        Width = 137
      end
    end
    object gridSolduriLevel: TcxGridLevel
      GridView = gridViewSolduri
    end
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 650
    Height = 41
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 1
    DesignSize = (
      650
      41)
    object Label1: TLabel
      Left = 8
      Top = 16
      Width = 23
      Height = 13
      Caption = 'Cont'
    end
    object Label2: TLabel
      Left = 448
      Top = 16
      Width = 43
      Height = 13
      Anchors = [akTop, akRight]
      Caption = 'An fiscal '
      Visible = False
    end
    object edtCont: TcxImageComboBox
      Left = 46
      Top = 14
      Hint = 'Cont'
      Anchors = [akTop, akRight]
      Properties.Items = <>
      Style.Color = 12910591
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clBtnShadow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clWhite
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      Width = 307
    end
    object edtAnFiscal: TcxImageComboBox
      Left = 497
      Top = 12
      Hint = 'Cont'
      Anchors = [akTop, akRight]
      Properties.Items = <>
      Style.Color = 12910591
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clBtnShadow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clWhite
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 1
      Visible = False
      Width = 111
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 307
    Width = 650
    Height = 221
    Align = alBottom
    BevelOuter = bvNone
    Color = 16505534
    ParentBackground = False
    TabOrder = 2
    object lcBasic: TdxLayoutControl
      Left = 0
      Top = 0
      Width = 512
      Height = 221
      Align = alClient
      TabOrder = 0
      TabStop = False
      LayoutLookAndFeel = frmData.Office
      object dxLayoutGroup1: TdxLayoutGroup
        AlignHorz = ahParentManaged
        AlignVert = avTop
        ButtonOptions.Buttons = <>
        Hidden = True
        ShowBorder = False
      end
    end
    object Panel1: TPanel
      Left = 512
      Top = 0
      Width = 138
      Height = 221
      Align = alRight
      BevelOuter = bvNone
      TabOrder = 1
      object cxButton1: TcxButton
        Left = 16
        Top = 6
        Width = 123
        Height = 25
        Action = actAdauga
        TabOrder = 0
        LookAndFeel.Kind = lfOffice11
      end
      object cxButton2: TcxButton
        Left = 16
        Top = 37
        Width = 123
        Height = 25
        Action = actModifica
        TabOrder = 1
        LookAndFeel.Kind = lfOffice11
      end
      object cxButton3: TcxButton
        Left = 16
        Top = 68
        Width = 123
        Height = 25
        Action = actSterge
        TabOrder = 2
        LookAndFeel.Kind = lfOffice11
      end
    end
  end
  object Split: TcxSplitter
    Left = 0
    Top = 299
    Width = 650
    Height = 8
    HotZoneClassName = 'TcxMediaPlayer9Style'
    AlignSplitter = salBottom
    Control = pnBottom
    Color = 16505534
    ParentColor = False
  end
  object DTSolduriComplet: TDataSource
    Left = 21
    Top = 80
  end
  object qrySolduri: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from solduri_repartitori'
      'where cont = :cont')
    Params = <
      item
        DataType = ftUnknown
        Name = 'cont'
        ParamType = ptUnknown
      end>
    DataSource = frmData.DTRepartitori
    Left = 51
    Top = 80
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'cont'
        ParamType = ptUnknown
      end>
  end
  object ActiuniSI: TActionList
    Left = 120
    Top = 176
    object actAdauga: TAction
      Caption = 'Adauga sold initial'
    end
    object actSterge: TAction
      Caption = 'Sterge sold initial'
    end
    object actModifica: TAction
      Caption = 'Modifica sold initial'
    end
  end
end
