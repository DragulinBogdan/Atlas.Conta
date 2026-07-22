object frmAnexeCulegere: TfrmAnexeCulegere
  Left = 271
  Top = 142
  Caption = 'Anexa Culegere'
  ClientHeight = 613
  ClientWidth = 862
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
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 862
    Height = 92
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    object Label4: TLabel
      Left = 464
      Top = 69
      Width = 102
      Height = 13
      Caption = 'Mod Introducere :'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edNrZerouri: TcxImageComboBox
      Left = 569
      Top = 64
      EditValue = '1'
      Properties.Items = <
        item
          Description = 'Leu'
          ImageIndex = 0
          Value = '1'
        end
        item
          Description = 'Zeci de lei'
          ImageIndex = 1
          Value = '10'
        end
        item
          Description = 'Sute de lei'
          ImageIndex = 2
          Value = '100'
        end
        item
          Description = 'Mii lei'
          ImageIndex = 3
          Value = '1000'
        end
        item
          Description = 'Zeci de mii'
          ImageIndex = 4
          Value = '10000'
        end
        item
          Description = 'Sute de mii'
          ImageIndex = 5
          Value = '100000'
        end
        item
          Description = 'Milioane'
          ImageIndex = 6
          Value = '1000000'
        end>
      Properties.OnChange = edNrZerouriPropertiesChange
      TabOrder = 0
      Width = 97
    end
    object edZerouri: TcxSpinEdit
      Left = 669
      Top = 64
      Properties.OnChange = edZerouriPropertiesChange
      TabOrder = 1
      Value = 1
      Width = 68
    end
    object btnDelDate: TcxButton
      Left = 125
      Top = 64
      Width = 174
      Height = 24
      Caption = 'Sterge Date Introduse'
      TabOrder = 2
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnDelDateClick
    end
    object btnRefreshDate: TcxButton
      Left = 307
      Top = 64
      Width = 120
      Height = 24
      Caption = 'Refresh Date'
      TabOrder = 3
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnRefreshDateClick
    end
    object pnParam: TPanel
      Left = 0
      Top = 0
      Width = 862
      Height = 60
      Align = alTop
      BevelOuter = bvNone
      Color = 15788262
      TabOrder = 4
      object Label2: TLabel
        Left = 6
        Top = 10
        Width = 104
        Height = 13
        Caption = 'Selectie Unitate : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label3: TLabel
        Left = 426
        Top = 10
        Width = 104
        Height = 13
        Caption = 'Perioada fiscala : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label1: TLabel
        Left = 4
        Top = 37
        Width = 98
        Height = 13
        Caption = 'Selectie Anexa : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edtUnitate: TcxPopupEdit
        Tag = -1
        Left = 108
        Top = 5
        Properties.PopupAutoSize = False
        Properties.PopupControl = cxTreeUnitati
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseQuery = edtUnitatePropertiesCloseQuery
        Properties.OnInitPopup = edtUnitatePropertiesInitPopup
        Properties.OnPopup = edtUnitatePropertiesPopup
        TabOrder = 0
        Width = 303
      end
      object edtPerioada: TcxImageComboBox
        Tag = -1
        Left = 531
        Top = 5
        Properties.Items = <>
        Properties.OnChange = edtPerioadaPropertiesChange
        TabOrder = 1
        Width = 303
      end
      object edtAnexa: TcxImageComboBox
        Tag = -1
        Left = 108
        Top = 32
        Properties.Items = <>
        Properties.OnChange = edtAnexaPropertiesChange
        TabOrder = 2
        Width = 432
      end
      object btnEditParams: TcxButton
        Left = 626
        Top = 30
        Width = 100
        Height = 24
        Caption = 'Parametrii'
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7F7F7FF949494FF7B7373FF84847BFF84847BFFBDB5
          B5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00E7E7E7FF6B6363FF848C84FF949494FF636363FF9494
          94FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00EFEFEFFF8C847BFFEFDEDEFFFFFFFF008C8473FF9494
          94FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0084847BFF63636BFF7B8494FF423939FFA59C
          9CFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0073D6EFFF00A5D6FF00ADDEFF00A5D6FF9CC6
          C6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7EFE7FF39DEFFFF10DEFFFF29D6FFFF10DEFFFF6BB5
          CEFFFFE7D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFF7EFFF84D6E7FF39D6FFFF42C6FFFF42C6FFFF42DEFFFF39CE
          EFFFBDB5ADFFFFF7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFF7F7FFADDEE7FF4ADEFFFF63D6FFFF73DEFFFF84DEFFFF6BD6FFFF63E7
          FFFF5ABDD6FFD6BDB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00DEEFEFFF7BDEF7FF7BD6FFFF8CD6FFFFADDEFFFFADE7FFFF94D6FFFF94DE
          FFFF73E7FFFF84B5C6FFF7E7DEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00BDE7F7FF94DEFFFF9CDEFFFFC6E7FFFFD6EFFFFFD6EFFFFFC6E7FFFFA5DE
          FFFFB5F7FFFF84D6E7FFCED6D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00B5E7FFFF94DEFFFFBDE7FFFFE7EFFFFFE7EFFFFFDEEFFFFFE7EFFFFFC6E7
          FFFFBDE7FFFFA5E7F7FFBDD6D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00A5DEFFFF94D6FFFFD6E7FFFFDEEFFFFFE7EFFFFFDEEFFFFFE7EFFFFFCEE7
          FFFFADDEFFFFB5E7EFFFDEDEDEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00ADDEF7FF73D6FFFFDEF7FFFFD6F7FFFFC6EFFFFFCEEFFFFFD6EFFFFFCEEF
          FFFF8CDEFFFFADDEEFFFFFEFE7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00EFF7F7FF6BCEFFFF7BDEFFFFE7FFFFFFEFFFFFFFCEF7FFFFC6EFFFFF94DE
          FFFF5ACEFFFFCEE7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00D6EFFFFF5ACEFFFF73DEFFFFB5EFFFFF94E7FFFF63DEFFFF4AC6
          F7FFB5DEEFFFFFFFF7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7FFFFFFA5E7FFFF84DEFFFF84DEFFFF94DEF7FFDEEF
          F7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
        TabOrder = 3
        Visible = False
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        OnClick = btnEditParamsClick
      end
      object edIdParam: TcxDBTextEdit
        Left = 735
        Top = 32
        DataBinding.DataField = 'id_anexe_centralizare_param'
        DataBinding.DataSource = DTHead
        ParentFont = False
        Properties.ReadOnly = True
        Style.Color = clSilver
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        TabOrder = 4
        Width = 121
      end
    end
  end
  object cxGridAnexa: TcxGrid
    Left = 0
    Top = 92
    Width = 862
    Height = 521
    Align = alClient
    TabOrder = 1
    object cxGridAnexaDBTableView1: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      object cxGridAnexaDBTableView1Column1: TcxGridDBColumn
      end
    end
    object GridAnexa: TcxGridDBBandedTableView
      Navigator.Buttons.CustomButtons = <>
      OnCustomDrawCell = GridAnexaCustomDrawCell
      OnFocusedRecordChanged = GridAnexaFocusedRecordChanged
      DataController.DataSource = DTAnexe
      DataController.KeyFieldNames = 'Id'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsView.CellAutoHeight = True
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      OptionsView.HeaderAutoHeight = True
      Bands = <
        item
          Width = 306
        end>
      object GridAnexaColumn1: TcxGridDBBandedColumn
        Caption = '123'
        PropertiesClassName = 'TcxCheckBoxProperties'
        Properties.Alignment = taCenter
        HeaderAlignmentHorz = taCenter
        Styles.Header = frmData.cxStyle46
        Position.BandIndex = 0
        Position.ColIndex = 0
        Position.RowIndex = 0
      end
    end
    object GridAnexaL: TcxGridLevel
      GridView = GridAnexa
    end
  end
  object cxTreeUnitati: TcxDBTreeList
    Left = 473
    Top = 348
    Width = 305
    Height = 184
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTOIUnitati
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_OI_UNITATI'
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeUnitatiDESCRIERE
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 2
    Visible = False
    OnDblClick = cxTreeUnitatiDblClick
    OnKeyDown = cxTreeUnitatiKeyDown
    object cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI_TIPURI'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.GlyphAlignHorz = taCenter
      Caption.Text = 'Institutie/Unitate'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATATEA_URMARITA'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'NUME_ORDONANTATOR'
      Width = 100
      Position.ColIndex = 10
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_UTILIZATORI'
      Width = 100
      Position.ColIndex = 13
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiSTARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'STARE'
      Width = 100
      Position.ColIndex = 12
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATEA_CENTRALIZATOARE'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA'
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_COD'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_CONT'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object QryAnexe: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spLstAnexeData :idParam, :idAnexa, :idUnitate, :idPerioadeF' +
        'iscale, :multiplicator')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idParam'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idAnexa'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idUnitate'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idPerioadeFiscale'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'multiplicator'
        ParamType = ptUnknown
      end>
    Left = 66
    Top = 103
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idParam'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idAnexa'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idUnitate'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'idPerioadeFiscale'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'multiplicator'
        ParamType = ptUnknown
      end>
  end
  object MemAnexe: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 37
    Top = 104
  end
  object DTAnexe: TDataSource
    DataSet = MemAnexe
    Left = 8
    Top = 104
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = cxGridAnexa
    PopupMenus = <>
    Left = 664
    Top = 552
  end
  object qryHead: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from anexe_centralizare_param')
    Params = <>
    Left = 590
    Top = 32
  end
  object DTHead: TDataSource
    DataSet = qryHead
    Left = 555
    Top = 31
  end
end
