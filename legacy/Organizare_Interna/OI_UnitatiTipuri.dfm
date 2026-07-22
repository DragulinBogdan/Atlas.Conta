object frmOIUnitatiTipuri: TfrmOIUnitatiTipuri
  Left = 297
  Top = 224
  Caption = 'frmOIUnitatiTipuri'
  ClientHeight = 419
  ClientWidth = 395
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
  object pnOptions: TPanel
    Left = 0
    Top = 41
    Width = 395
    Height = 30
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    DesignSize = (
      395
      30)
    object btnAdd: TcxButton
      Left = 5
      Top = 2
      Width = 24
      Height = 24
      Hint = 'Inchide Ecranul'
      Anchors = [akLeft, akBottom]
      Caption = '+'
      ModalResult = 1
      TabOrder = 0
      OnClick = btnAddClick
    end
    object btnDel: TcxButton
      Left = 33
      Top = 2
      Width = 24
      Height = 24
      Hint = 'Inchide Ecranul'
      Anchors = [akLeft, akBottom]
      Caption = '-'
      ModalResult = 1
      TabOrder = 1
      OnClick = btnDelClick
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 382
    Width = 395
    Height = 37
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 1
    DesignSize = (
      395
      37)
    object cxButton1: TcxButton
      Left = 315
      Top = 9
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
    end
  end
  object cxOIUnitatiTipuri: TcxGrid
    Left = 0
    Top = 71
    Width = 395
    Height = 251
    Hint = 'sageata sus/sageata jos permite deplasarea prin nomenclator'
    Align = alClient
    TabOrder = 2
    object cxOIUnitatiTipuriDBTableView1: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = frmData.DTOIUnitatiTipuri
      DataController.KeyFieldNames = 'ID_OI_UNITATI_TIPURI'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      object cxOIUnitatiTipuriDBTableView1ID_OI_UNITATI_TIPURI: TcxGridDBColumn
        Caption = 'Id'
        DataBinding.FieldName = 'ID_OI_UNITATI_TIPURI'
        Options.Editing = False
        Width = 29
      end
      object cxOIUnitatiTipuriDBTableView1DENUMIRE1: TcxGridDBColumn
        Caption = 'Denumire'
        DataBinding.FieldName = 'DENUMIRE'
        Width = 199
      end
      object cxOIUnitatiTipuriDBTableView1ARE_CONTABILITATE: TcxGridDBColumn
        Caption = 'Are Contabilitate'
        DataBinding.FieldName = 'ARE_CONTABILITATE'
        Width = 55
      end
      object cxOIUnitatiTipuriDBTableView1ARE_CONT: TcxGridDBColumn
        Caption = 'Cont Trezorerie'
        DataBinding.FieldName = 'ARE_CONT'
        Width = 57
      end
      object cxOIUnitatiTipuriDBTableView1ESTE_PROIECT: TcxGridDBColumn
        Caption = 'Este Proiect'
        DataBinding.FieldName = 'ESTE_PROIECT'
        Width = 53
      end
    end
    object cxOIUnitatiTipuriLevel1: TcxGridLevel
      GridView = cxOIUnitatiTipuriDBTableView1
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 322
    Width = 395
    Height = 60
    Align = alBottom
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 3
    DesignSize = (
      395
      60)
    object Label1: TLabel
      Left = 8
      Top = 8
      Width = 66
      Height = 13
      Caption = 'Denumire : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object cxDBTextEdit1: TcxDBTextEdit
      Left = 75
      Top = 4
      Anchors = [akLeft, akTop, akRight]
      DataBinding.DataField = 'DENUMIRE'
      DataBinding.DataSource = frmData.DTOIUnitatiTipuri
      TabOrder = 0
      Width = 316
    end
    object cxDBCheckBox2: TcxDBCheckBox
      Left = 128
      Top = 32
      Caption = 'Are Contabilitate'
      DataBinding.DataField = 'ARE_CONTABILITATE'
      DataBinding.DataSource = frmData.DTOIUnitatiTipuri
      ParentFont = False
      Properties.NullStyle = nssUnchecked
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      TabOrder = 1
    end
    object cxDBCheckBox3: TcxDBCheckBox
      Left = 256
      Top = 32
      Caption = 'Cont Trezorerie'
      DataBinding.DataField = 'ARE_CONT'
      DataBinding.DataSource = frmData.DTOIUnitatiTipuri
      ParentFont = False
      Properties.NullStyle = nssUnchecked
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      TabOrder = 2
    end
    object chkEsteProiect: TcxDBCheckBox
      Left = 16
      Top = 32
      Caption = 'Este Proiect'
      DataBinding.DataField = 'ESTE_PROIECT'
      DataBinding.DataSource = frmData.DTOIUnitatiTipuri
      ParentFont = False
      Properties.NullStyle = nssUnchecked
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      TabOrder = 3
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 395
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Tipuri Unitati'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 4
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
end
