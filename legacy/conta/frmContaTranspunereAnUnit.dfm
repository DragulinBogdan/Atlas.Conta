object frmContaTranspunereAn: TfrmContaTranspunereAn
  Left = 355
  Top = 156
  Caption = 'Transpunere an fiscal'
  ClientHeight = 473
  ClientWidth = 572
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
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object cxGridTranspunere: TcxGrid
    Left = 0
    Top = 34
    Width = 572
    Height = 343
    Align = alClient
    TabOrder = 0
    LookAndFeel.Kind = lfOffice11
    LookAndFeel.NativeStyle = False
    ExplicitWidth = 580
    ExplicitHeight = 351
    object gridTranspunere: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      OnFocusedRecordChanged = gridTranspunereFocusedRecordChanged
      DataController.DataSource = DSTranspunere
      DataController.KeyFieldNames = 'id_cplan_transpunere'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.Footer = True
      OptionsView.GroupByBox = False
      OptionsView.Indicator = True
      object gridTranspunereid_cplan_transpunere: TcxGridDBColumn
        Caption = 'Id'
        DataBinding.FieldName = 'id_cplan_transpunere'
        Visible = False
      end
      object gridTranspunerecont: TcxGridDBColumn
        Caption = 'Cont nou'
        DataBinding.FieldName = 'cont'
        PropertiesClassName = 'TcxLookupComboBoxProperties'
        Properties.KeyFieldNames = 'cont'
        Properties.ListColumns = <
          item
            FieldName = 'descriere'
          end>
        Properties.ListSource = dsPlanNou
        Width = 249
      end
      object gridTranspunerecont_vechi: TcxGridDBColumn
        Caption = 'Cont vechi'
        DataBinding.FieldName = 'cont_vechi'
        PropertiesClassName = 'TcxLookupComboBoxProperties'
        Properties.KeyFieldNames = 'cont'
        Properties.ListColumns = <
          item
            FieldName = 'descriere'
          end>
        Properties.ListSource = dsPlanVechi
        Width = 304
      end
    end
    object cxGridTranspunereLevel1: TcxGridLevel
      GridView = gridTranspunere
    end
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 572
    Height = 34
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 1
    ExplicitWidth = 580
    DesignSize = (
      572
      34)
    object Label1: TLabel
      Left = 13
      Top = 10
      Width = 148
      Height = 13
      Caption = 'Modalitate definire transpunere '
    end
    object edModalitateSelectie: TcxImageComboBox
      Left = 168
      Top = 8
      Anchors = [akLeft, akTop, akRight]
      EditValue = 0
      Properties.Items = <
        item
          Description = 'Plan conturi an anterior'
          ImageIndex = 0
          Value = 0
        end
        item
          Description = 'Conturi din balanta anul anterior cu sold'
          Value = 1
        end>
      Properties.OnEditValueChanged = edModalitateSelectiePropertiesEditValueChanged
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      Width = 393
    end
  end
  object Split: TcxSplitter
    Left = 0
    Top = 377
    Width = 572
    Height = 8
    HotZoneClassName = 'TcxMediaPlayer9Style'
    AlignSplitter = salBottom
    Control = pnBottom
    Color = 16505534
    ParentColor = False
    ExplicitTop = 385
    ExplicitWidth = 8
  end
  object pnBottom: TPanel
    Left = 0
    Top = 385
    Width = 572
    Height = 88
    Align = alBottom
    BevelOuter = bvNone
    Color = 16505534
    ParentBackground = False
    TabOrder = 3
    ExplicitTop = 393
    ExplicitWidth = 580
    object lcBasic: TdxLayoutControl
      Left = 0
      Top = 0
      Width = 442
      Height = 88
      Align = alClient
      TabOrder = 0
      LayoutLookAndFeel = frmData.Office
      object edContNou: TcxDBLookupComboBox
        Left = 63
        Top = 0
        DataBinding.DataField = 'cont'
        DataBinding.DataSource = DSTranspunere
        Properties.KeyFieldNames = 'cont'
        Properties.ListColumns = <
          item
            FieldName = 'descriere'
          end>
        Properties.ListSource = dsPlanNou
        Style.HotTrack = False
        TabOrder = 0
        Width = 364
      end
      object edContVechi: TcxDBLookupComboBox
        Left = 63
        Top = 27
        DataBinding.DataField = 'cont_vechi'
        DataBinding.DataSource = DSTranspunere
        Properties.KeyFieldNames = 'cont'
        Properties.ListColumns = <
          item
            FieldName = 'descriere'
          end>
        Properties.ListSource = dsPlanVechi
        Style.HotTrack = False
        TabOrder = 1
        Width = 296
      end
      object dxLayoutGroup1: TdxLayoutGroup
        AlignHorz = ahParentManaged
        AlignVert = avTop
        ButtonOptions.Buttons = <>
        Hidden = True
        ShowBorder = False
        Index = -1
      end
      object lcBasicItem3: TdxLayoutItem
        Parent = dxLayoutGroup1
        CaptionOptions.Text = 'Cont nou'
        Control = edContNou
        ControlOptions.OriginalHeight = 21
        ControlOptions.OriginalWidth = 364
        ControlOptions.ShowBorder = False
        Index = 0
      end
      object lcBasicItem1: TdxLayoutItem
        Parent = dxLayoutGroup1
        CaptionOptions.Text = 'Cont vechi'
        Control = edContVechi
        ControlOptions.OriginalHeight = 21
        ControlOptions.OriginalWidth = 296
        ControlOptions.ShowBorder = False
        Index = 1
      end
    end
    object Panel1: TPanel
      Left = 434
      Top = 0
      Width = 138
      Height = 88
      Align = alRight
      BevelOuter = bvNone
      TabOrder = 1
      ExplicitLeft = 442
      object cxButton1: TcxButton
        Left = 11
        Top = 5
        Width = 123
        Height = 25
        Action = actAdauga
        Caption = 'Adauga'
        LookAndFeel.Kind = lfOffice11
        TabOrder = 0
      end
      object cxButton2: TcxButton
        Left = 11
        Top = 32
        Width = 123
        Height = 25
        Action = actModifica
        Caption = 'Modifica'
        LookAndFeel.Kind = lfOffice11
        TabOrder = 1
      end
      object cxButton3: TcxButton
        Left = 11
        Top = 59
        Width = 123
        Height = 25
        Action = actSterge
        Caption = 'Sterge'
        LookAndFeel.Kind = lfOffice11
        TabOrder = 2
      end
    end
  end
  object DSTranspunere: TDataSource
    DataSet = qryTranspunere
    Left = 200
    Top = 104
  end
  object qryTranspunere: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryTranspunereAfterOpen
    UpdateObject = updTranspunere
    SQL.Strings = (
      'exec spContaTranspunere')
    Params = <>
    Left = 232
    Top = 104
    object qryTranspunereid_cplan_transpunere: TFloatField
      FieldName = 'id_cplan_transpunere'
      ReadOnly = True
    end
    object qryTranspunerecont: TStringField
      FieldName = 'cont'
      ReadOnly = True
      Size = 100
    end
    object qryTranspunerecont_vechi: TStringField
      FieldName = 'cont_vechi'
      ReadOnly = True
      Size = 100
    end
    object qryTranspunerean_fiscal: TIntegerField
      FieldName = 'an_fiscal'
      ReadOnly = True
    end
  end
  object ActiuniSI: TActionList
    Left = 120
    Top = 176
    object actAdauga: TAction
      Caption = 'Adauga sold initial'
      OnExecute = actAdaugaExecute
    end
    object actSterge: TAction
      Caption = 'Sterge sold initial'
      OnExecute = actStergeExecute
    end
    object actModifica: TAction
      Caption = 'Modifica sold initial'
      OnExecute = actModificaExecute
    end
  end
  object updTranspunere: TZUpdateSQL
    DeleteSQL.Strings = (
      'exec spContaTranspunereDelete :id_cplan_transpunere')
    InsertSQL.Strings = (
      'exec spContaTranspunereInsert :cont, :cont_vechi')
    ModifySQL.Strings = (
      
        'exec spContaTranspunereUpdate :id_cplan_transpunere, :cont, :con' +
        't_vechi')
    UseSequenceFieldForRefreshSQL = False
    Left = 264
    Top = 104
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'id_cplan_transpunere'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'cont'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'cont_vechi'
        ParamType = ptUnknown
      end>
  end
  object dsPlanVechi: TDataSource
    DataSet = qryPlanVechi
    Left = 216
    Top = 216
  end
  object qryPlanVechi: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spContaPlanVechiTranspunere :modalitate')
    Params = <
      item
        DataType = ftUnknown
        Name = 'modalitate'
        ParamType = ptUnknown
      end>
    Left = 248
    Top = 216
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'modalitate'
        ParamType = ptUnknown
      end>
  end
  object dsPlanNou: TDataSource
    DataSet = qryPlanNou
    Left = 216
    Top = 248
  end
  object qryPlanNou: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spContaPlanNouTranspunere')
    Params = <>
    Left = 248
    Top = 248
  end
end
