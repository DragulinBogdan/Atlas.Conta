object frmInchiderePerioada: TfrmInchiderePerioada
  Left = 313
  Top = 101
  BorderStyle = bsDialog
  ClientHeight = 414
  ClientWidth = 708
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 339
    Width = 708
    Height = 75
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    DesignSize = (
      708
      75)
    object btnOk: TcxButton
      Left = 614
      Top = 6
      Width = 75
      Height = 25
      Hint = 'Inchide Ecranul'
      Anchors = [akRight, akBottom]
      Caption = '&Inchide'
      LookAndFeel.Kind = lfOffice11
      ModalResult = 1
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
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 708
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Inchidere Perioada Fiscala'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 1
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object cxGridInchidere: TcxGrid
    Left = 0
    Top = 41
    Width = 708
    Height = 181
    Align = alClient
    TabOrder = 2
    LookAndFeel.Kind = lfOffice11
    ExplicitHeight = 219
    object GridInchidere: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      Navigator.Visible = True
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = DTPerioadeFiscale
      DataController.KeyFieldNames = 'id_perioade_fiscale'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.IncSearch = True
      OptionsBehavior.ImmediateEditor = False
      OptionsCustomize.ColumnsQuickCustomization = True
      OptionsData.CancelOnExit = False
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      OptionsView.Indicator = True
      object GridInchidereAnFiscal: TcxGridDBColumn
        Caption = 'An Fiscal'
        DataBinding.FieldName = 'AnFiscal'
        Width = 38
      end
      object GridInchidereDataStart: TcxGridDBColumn
        Caption = 'Data Start'
        DataBinding.FieldName = 'DataStart'
        Width = 53
      end
      object GridInchidereDataStop: TcxGridDBColumn
        Caption = 'Data Stop'
        DataBinding.FieldName = 'DataStop'
        Width = 81
      end
      object GridInchidereInchisa: TcxGridDBColumn
        DataBinding.FieldName = 'Inchisa'
        Width = 59
      end
      object GridInchidereDataInchidere: TcxGridDBColumn
        Caption = 'Data Inchidere'
        DataBinding.FieldName = 'DataInchidere'
        Width = 103
      end
      object GridInchidereUtilizatorInchidere: TcxGridDBColumn
        Caption = 'Utilizator Inchidere'
        DataBinding.FieldName = 'UtilizatorInchidere'
        Width = 143
      end
      object GridInchidereluna: TcxGridDBColumn
        Caption = 'Luna'
        DataBinding.FieldName = 'luna'
        Visible = False
      end
      object GridInchideretrim: TcxGridDBColumn
        Caption = 'Trimestru'
        DataBinding.FieldName = 'trim'
        Visible = False
      end
      object GridInchidereblocata: TcxGridDBColumn
        Caption = 'Blocata'
        DataBinding.FieldName = 'blocata'
        Width = 47
      end
      object GridInchidereDataBlocare: TcxGridDBColumn
        Caption = 'Data Blocare'
        DataBinding.FieldName = 'DataBlocare'
        Width = 45
      end
      object GridInchidereUtilizatorBlocare: TcxGridDBColumn
        Caption = 'Utilizator Blocare'
        DataBinding.FieldName = 'UtilizatorBlocare'
        Width = 125
      end
      object GridInchidereID: TcxGridDBColumn
        DataBinding.FieldName = 'ID'
        Visible = False
      end
    end
    object GridInchidereLevel1: TcxGridLevel
      GridView = GridInchidere
    end
  end
  object pnContext: TPanel
    Left = 0
    Top = 222
    Width = 708
    Height = 117
    Align = alBottom
    TabOrder = 3
    ExplicitTop = 260
    DesignSize = (
      708
      117)
    object Bevel: TBevel
      Left = 8
      Top = 8
      Width = 688
      Height = 93
      Anchors = [akLeft, akTop, akRight]
    end
    object Label1: TLabel
      Left = 21
      Top = 15
      Width = 129
      Height = 13
      Caption = 'Data Inceput Perioada'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 274
      Top = 15
      Width = 122
      Height = 13
      Caption = 'Data Sfarsit Perioada'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object lbDataInchidere: TLabel
      Left = 212
      Top = 63
      Width = 85
      Height = 13
      Caption = 'Data Inchidere'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object lbDataBlocare: TLabel
      Left = 212
      Top = 86
      Width = 75
      Height = 13
      Caption = 'Data Blocare'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object cxDBDateEdit1: TcxDBDateEdit
      Left = 29
      Top = 31
      DataBinding.DataField = 'DataStart'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentFont = False
      Properties.DateButtons = []
      Properties.InputKind = ikMask
      Properties.ReadOnly = True
      Properties.SaveTime = False
      Properties.ShowTime = False
      Style.Color = 12910591
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.LookAndFeel.Kind = lfOffice11
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      Width = 137
    end
    object cxDBDateEdit2: TcxDBDateEdit
      Left = 282
      Top = 31
      DataBinding.DataField = 'DataStop'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentFont = False
      Properties.DateButtons = []
      Properties.InputKind = ikMask
      Properties.ReadOnly = True
      Properties.SaveTime = False
      Properties.ShowTime = False
      Style.Color = 12910591
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.LookAndFeel.Kind = lfOffice11
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 1
      Width = 137
    end
    object chkInchidere: TcxDBCheckBox
      Tag = 1
      Left = 12
      Top = 60
      Caption = 'Inchidere DA(X)/NU( )'
      DataBinding.DataField = 'Inchisa'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentFont = False
      Properties.ImmediatePost = True
      Properties.NullStyle = nssUnchecked
      Properties.OnEditValueChanged = chkInchiderePropertiesChange
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.LookAndFeel.Kind = lfOffice11
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 2
    end
    object edDataInchidere: TcxDBDateEdit
      Left = 310
      Top = 60
      DataBinding.DataField = 'DataInchidere'
      DataBinding.DataSource = DTPerioadeFiscale
      Properties.InputKind = ikMask
      Properties.ReadOnly = True
      Properties.SaveTime = False
      Properties.ShowTime = False
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 3
      Width = 135
    end
    object cxDBTextEdit1: TcxDBTextEdit
      Left = 204
      Top = 18
      DataBinding.DataField = 'trim'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentFont = False
      Properties.Alignment.Horz = taCenter
      Properties.ReadOnly = True
      Style.Color = clSkyBlue
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.LookAndFeel.Kind = lfOffice11
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 4
      Width = 41
    end
    object lbID: TcxDBLabel
      Left = 204
      Top = 40
      DataBinding.DataField = 'ID'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentColor = False
      ParentFont = False
      Properties.Alignment.Horz = taCenter
      Properties.Alignment.Vert = taVCenter
      Style.BorderColor = clWindowFrame
      Style.Color = clBtnFace
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clGray
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = []
      Style.LookAndFeel.Kind = lfOffice11
      Style.TextColor = clGray
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      Height = 21
      Width = 41
      AnchorX = 225
      AnchorY = 51
    end
    object lbUtilizatorInchidere: TcxDBLabel
      Left = 457
      Top = 58
      DataBinding.DataField = 'UtilizatorInchidere'
      DataBinding.DataSource = DTPerioadeFiscale
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      Height = 21
      Width = 232
    end
    object chkBlocare: TcxDBCheckBox
      Tag = 1
      Left = 12
      Top = 83
      Caption = 'Blocare definitiva DA(X)/NU( )'
      DataBinding.DataField = 'blocata'
      DataBinding.DataSource = DTPerioadeFiscale
      ParentFont = False
      Properties.ImmediatePost = True
      Properties.NullStyle = nssUnchecked
      Properties.OnEditValueChanged = chkBlocarePropertiesChange
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.LookAndFeel.Kind = lfOffice11
      Style.IsFontAssigned = True
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 7
    end
    object edDataBlocare: TcxDBDateEdit
      Left = 310
      Top = 83
      DataBinding.DataField = 'DataBlocare'
      DataBinding.DataSource = DTPerioadeFiscale
      Properties.InputKind = ikMask
      Properties.ReadOnly = True
      Properties.SaveTime = False
      Properties.ShowTime = False
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 8
      Width = 135
    end
    object lbUtilizatorBlocare: TcxDBLabel
      Left = 457
      Top = 81
      DataBinding.DataField = 'UtilizatorBlocare'
      DataBinding.DataSource = DTPerioadeFiscale
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      Height = 21
      Width = 232
    end
  end
  object DTPerioadeFiscale: TDataSource
    DataSet = QryPerioadeFiscale
    Left = 352
    Top = 8
  end
  object QryPerioadeFiscale: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spContPerioadeFiscale :DATA_REF')
    Params = <
      item
        DataType = ftDateTime
        Name = 'DATA_REF'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 384
    Top = 8
    ParamData = <
      item
        DataType = ftDateTime
        Name = 'DATA_REF'
        ParamType = ptUnknown
        Size = -1
      end>
  end
end
