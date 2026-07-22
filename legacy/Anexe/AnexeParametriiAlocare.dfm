object frmAsocParam: TfrmAsocParam
  Left = 502
  Top = 240
  BorderStyle = bsDialog
  Caption = 'Asociere parametrii'
  ClientHeight = 435
  ClientWidth = 273
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
  PixelsPerInch = 96
  TextHeight = 13
  object pnOptions: TPanel
    Left = 0
    Top = 41
    Width = 273
    Height = 30
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    DesignSize = (
      273
      30)
    object btnAdd: TcxButton
      Left = 44
      Top = 2
      Width = 24
      Height = 24
      Hint = 'Adauga tip document'
      Anchors = [akRight, akBottom]
      Caption = '+'
      TabOrder = 0
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -27
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnAddClick
    end
    object btnDel: TcxButton
      Left = 72
      Top = 2
      Width = 24
      Height = 24
      Hint = 'Sterge tip document'
      Anchors = [akRight, akBottom]
      Caption = '-'
      TabOrder = 1
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -27
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnDelClick
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 294
    Width = 273
    Height = 81
    Align = alBottom
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 1
    ExplicitTop = 301
    DesignSize = (
      273
      81)
    object Label1: TLabel
      Left = 7
      Top = 35
      Width = 50
      Height = 13
      Caption = 'Ordine : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 7
      Top = 10
      Width = 70
      Height = 13
      Caption = 'Parametru : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edParam: TcxDBImageComboBox
      Left = 73
      Top = 5
      Anchors = [akLeft, akTop, akRight]
      DataBinding.DataField = 'ID_ANEXE_PARAMETRII'
      DataBinding.DataSource = DTParam
      ParentFont = False
      Properties.Items = <>
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      TabOrder = 1
      Width = 194
    end
    object cxDBTextEdit1: TcxDBSpinEdit
      Left = 74
      Top = 31
      Anchors = [akLeft, akTop, akRight]
      DataBinding.DataField = 'ORDINE'
      DataBinding.DataSource = DTParam
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      TabOrder = 0
      Width = 194
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 375
    Width = 273
    Height = 60
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 2
    DesignSize = (
      273
      60)
    object cxButton1: TcxButton
      Left = 192
      Top = 0
      Width = 75
      Height = 25
      Hint = 'Inchide Ecranul'
      Anchors = [akRight, akBottom]
      Caption = '&Inchide'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F4F4F4FFD9DDD9FFA9BDA9FF7BA57BFF639C64FF639C64FF7EA87EFFABC0
        ABFFD9DED9FFF5F5F5FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EDEE
        EDFFB8C5B8FF58A459FF27B429FF11D015FF06E00DFF06E00CFF10D016FF24B8
        27FF55AB57FFB9C8B9FFEEEFEEFFF8F8F800F8F8F800F8F8F800F0F0F0FFACBC
        ACFF43A846FF0BD711FF03EA0BFF16E11DFF06F20EFF00F708FF00F608FF00F2
        08FF08DD0FFF3DB040FFAAC3AAFFF0F1F0FFF8F8F800F7F7F7FFC6CFC6FF4BA7
        4CFF0AD410FF01F009FF36DF3BFF8FC590FF2EDC34FF00F808FF00F808FF00F8
        08FF00F408FF07DB0EFF42B045FFC4D2C4FFF7F7F7FFE8EAE8FF75AE76FF0EC3
        14FF06E50DFF46DA4BFFC1DAC2FFE9EAE9FFA9CAAAFF12E819FF00F708FF00F8
        08FF00F808FF00ED08FF0ACB10FF69B26BFFE5EAE5FFCFDACFFF3CA43EFF21CF
        27FF5ED662FFCCDBCCFFDFECDFFFC5E9C5FFE0E7E0FF77CB79FF12EA19FF00F8
        08FF00F808FF00F108FF00DB08FF2EAB30FFC9D9C9FFB4CAB4FF1FA622FF31CC
        37FFB0DCB0FFF3F4F3FFA9E7ABFF44DF48FFC7EEC8FFD4DFD4FF67D069FF0EEC
        14FF00F808FF00F108FF00DE08FF13B118FFA4C5A4FFA9C5A9FF16A81AFF06D3
        0EFF25E12DFF86E288FF5BE760FF04F30CFF6EE571FFF4F4F4FFCEDCCFFF64CD
        67FF0BED12FF00EE08FF00DA08FF08B40FFF8DBA8EFFADC9AEFF17A11BFF01CA
        09FF00E308FF00F408FF00F708FF00F808FF05F00DFFB9E9BAFFF5F5F5FFE2E2
        E2FF92C593FF12D519FF00D009FF09AB0EFF91BC91FFC4D6C4FF259928FF04BD
        0BFF02D60AFF00EB08FF00F308FF00F708FF00F808FF3CE942FFCEEFD0FFF6F6
        F6FFD9D9D9FF19C120FF04C20CFF169D1AFFADCAADFFDFE7DFFF53A455FF10AC
        14FF0BC613FF00D908FF00E708FF00EE08FF00F108FF08EE0FFF5BE25FFFD7EE
        D8FFD9D9D9FF0CB612FF0FB214FF399C3BFFD2DFD2FFF2F4F2FF9ABF9BFF229B
        25FF28B92BFF09C410FF00D108FF00DC08FF00E008FF00DF08FF0AD811FF59CF
        5DFFCDDFCEFF2FAC31FF1E9F21FF81B481FFEDF0EDFFF8F8F800E4E9E4FF76AC
        76FF38A33AFF44BE48FF20BD25FF0BBE12FF05C10CFF05C10BFF0BBE10FF1EBB
        21FF57BA59FF43A445FF5FA55FFFD9E2D9FFF8F8F800F8F8F800F8F8F800DAE2
        DAFF78AE78FF53A955FF63BF66FF60C563FF54C457FF54C457FF60C562FF65C0
        67FF54AC56FF66A867FFCBD9CCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8
        F800E4EAE4FFA0C3A0FF7AB07BFF70AC71FF74B274FF74B275FF71AD71FF77AD
        77FF96BD96FFDCE4DCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F2F4F2FFE0E8E0FFCADACAFFB8CFB8FFB7CEB7FFC9D9C9FFDEE7
        DEFFF0F2F0FFF8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 0
      OnClick = btnCloseClick
      ExplicitTop = -23
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 273
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Parametrii'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 3
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object cxGrid: TcxGrid
    Left = 0
    Top = 71
    Width = 273
    Height = 223
    Align = alClient
    TabOrder = 4
    ExplicitHeight = 268
    object Grid: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = DTParam
      DataController.KeyFieldNames = 'ID_CTIP_DOCUMENTE'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      object GridID_ANEXE_BILANT_PARAMETRII: TcxGridDBColumn
        DataBinding.FieldName = 'ID_ANEXE_BILANT_PARAMETRII'
        Visible = False
      end
      object GridID_ANEXE_PARAMETRII: TcxGridDBColumn
        Caption = 'Parametru'
        DataBinding.FieldName = 'ID_ANEXE_PARAMETRII'
        PropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Items = <>
        Width = 158
      end
      object GridID_ANEXE_BILANT: TcxGridDBColumn
        DataBinding.FieldName = 'ID_ANEXE_BILANT'
        Visible = False
      end
      object GridORDINE: TcxGridDBColumn
        Caption = 'Ordine'
        DataBinding.FieldName = 'ORDINE'
        SortIndex = 0
        SortOrder = soAscending
        Width = 113
      end
    end
    object GridL: TcxGridLevel
      GridView = Grid
    end
  end
  object DTParam: TDataSource
    DataSet = qryParam
    Left = 20
    Top = 111
  end
  object qryParam: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'SELECT * FROM ANEXE_BILANT_PARAMETRII WHERE ID_ANEXE_BILANT = :I' +
        'D_ANEXE_BILANT')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANEXE_BILANT'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 53
    Top = 112
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANEXE_BILANT'
        ParamType = ptUnknown
        Size = 4
      end>
  end
end
