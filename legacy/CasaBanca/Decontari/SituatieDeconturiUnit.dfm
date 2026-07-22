object FrmSituatieDeconturi: TFrmSituatieDeconturi
  Left = 277
  Top = 136
  ActiveControl = edtCasa
  Caption = 'Deconturi'
  ClientHeight = 492
  ClientWidth = 857
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  WindowState = wsMaximized
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 857
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Situatie Deconturi '
    InfoFont.Charset = ANSI_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'Arial Black'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 5
    ActAsCaption = False
    HideCaption = False
    DesignSize = (
      857
      41)
  end
  object pnContent: TPanel
    Left = 0
    Top = 68
    Width = 857
    Height = 424
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    object GridDeconturi: TcxDBTreeList
      Left = 0
      Top = 0
      Width = 857
      Height = 424
      Align = alClient
      Bands = <
        item
        end>
      DataController.DataSource = DTDecont
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID'
      Navigator.Buttons.CustomButtons = <>
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      ScrollbarAnnotations.CustomAnnotations = <>
      TabOrder = 0
      ExplicitTop = 1
      object GridDeconturiNIVEL: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'NIVEL'
        Width = 122
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiCOD_CASA: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'COD_CASA'
        Width = 100
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiDENUMIRE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Denumire'
        DataBinding.FieldName = 'DENUMIRE'
        Width = 218
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiEXPLICATIE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Explicatie'
        DataBinding.FieldName = 'EXPLICATIE'
        Width = 58
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiNR_DECONT: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Nr Decont'
        DataBinding.FieldName = 'NR_DECONT'
        Width = 49
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiDATA_DECONT: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Data Decont'
        DataBinding.FieldName = 'DATA_DECONT'
        Width = 109
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiDATA_OPERATIE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Data Operatie'
        DataBinding.FieldName = 'DATA_OPERATIE'
        Width = 69
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiSUMA_RIDICATA: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Suma Ridicata'
        DataBinding.FieldName = 'SUMA_RIDICATA'
        Width = 77
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiSUMA_JUSTIFICATA: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Suma Justificata'
        DataBinding.FieldName = 'SUMA_JUSTIFICATA'
        Width = 77
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiDIFERENTA: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Diferenta'
        DataBinding.FieldName = 'DIFERENTA'
        Width = 83
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiCODGEST: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'CODGEST'
        Width = 122
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiCOD: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'COD'
        Width = 60
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiID: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'ID'
        Width = 122
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object GridDeconturiID_PARINTE: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'ID_PARINTE'
        Width = 133
        Position.ColIndex = 13
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object TreeRepartitori: TdxDBTreeList
      Left = 232
      Top = 144
      Width = 409
      Height = 217
      SearchType = stStart
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'ID_REPARTITORI'
      ParentField = 'ID_PARINTE'
      TabOrder = 1
      Visible = False
      OnDblClick = TreeRepartitoriDblClick
      OnKeyDown = TreeRepartitoriKeyDown
      DataSource = frmData.DTRepartitori
      LookAndFeel = lfUltraFlat
      OptionsBehavior = [etoAutoSearch, etoAutoSort]
      OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
      OptionsView = [etoAutoWidth, etoIndicator, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
      TreeLineColor = clGrayText
      object TreeRepartitoriCONT: TdxDBTreeListMaskColumn
        Caption = 'Cont'
        DisableEditor = True
        HeaderAlignment = taCenter
        Sorted = csUp
        Width = 49
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_REPARTITORI'
      end
      object TreeRepartitoriNUME: TdxDBTreeListMaskColumn
        Caption = 'Denumire'
        DisableEditor = True
        HeaderAlignment = taCenter
        Width = 117
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUME'
      end
      object TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn
        Caption = 'Cod'
        DisableEditor = True
        HeaderAlignment = taCenter
        Width = 30
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CODSECTIE'
      end
      object TreeRepartitoriADRESA: TdxDBTreeListMaskColumn
        Caption = 'Adresa'
        DisableEditor = True
        HeaderAlignment = taCenter
        Width = 102
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ADRESA'
      end
      object TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn
        Caption = 'Interna'
        DisableEditor = True
        HeaderAlignment = taCenter
        Width = 38
        BandIndex = 0
        RowIndex = 0
        FieldName = 'GESTINT'
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object TreeRepartitoriTIPGEST: TdxDBTreeListMaskColumn
        Caption = 'Tip Gestiune'
        DisableEditor = True
        HeaderAlignment = taCenter
        Visible = False
        Width = 59
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TIP_GESTIUNE'
      end
    end
  end
  object pnDef: TPanel
    Left = 0
    Top = 41
    Width = 857
    Height = 27
    Align = alTop
    BevelOuter = bvNone
    Color = 15788249
    TabOrder = 2
    DesignSize = (
      857
      27)
    object Label1: TLabel
      Left = 3
      Top = 0
      Width = 19
      Height = 26
      Caption = 'De'#13#10' la:'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 113
      Top = -1
      Width = 30
      Height = 26
      Caption = 'Pana'#13#10'   la:'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      WordWrap = True
    end
    object Label3: TLabel
      Left = 451
      Top = 5
      Width = 69
      Height = 13
      Caption = 'Repartitor : '
      Constraints.MaxWidth = 69
      Constraints.MinWidth = 69
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label4: TLabel
      Left = 226
      Top = -1
      Width = 42
      Height = 26
      Caption = 'Casa Decont'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      WordWrap = True
    end
    object edtDeLa: TcxDateEdit
      Left = 34
      Top = 2
      Properties.OnChange = edtDeLaDateChange
      TabOrder = 0
      Width = 76
    end
    object edtPanaLa: TcxDateEdit
      Left = 146
      Top = 3
      Properties.OnChange = edtDeLaDateChange
      TabOrder = 1
      Width = 77
    end
    object edtCasa: TcxImageComboBox
      Left = 272
      Top = 2
      Properties.Items = <>
      Properties.OnChange = edtDeLaDateChange
      TabOrder = 2
      Width = 168
    end
    object edtRepartitor: TcxPopupEdit
      Tag = -1
      Left = 517
      Top = 1
      Anchors = [akLeft, akTop, akRight]
      Properties.PopupAutoSize = False
      Properties.PopupClientEdge = True
      Properties.PopupControl = TreeRepartitori
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = edtRepartitorPropertiesCloseUp
      Properties.OnInitPopup = edtRepartitorPropertiesInitPopup
      TabOrder = 3
      Width = 332
    end
  end
  object DTDecont: TDataSource
    DataSet = QryDecont
    Left = 8
    Top = 441
  end
  object QryDecont: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_JUSTIFICARI_DECONTARI :DELA, :PANALA, :COD_CASA, :ID_REP' +
        'ARTITOR')
    Params = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DELA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'PANALA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CASA'
        ParamType = ptUnknown
        Size = 4
        Value = 5
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 40
    Top = 441
    ParamData = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DELA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'PANALA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CASA'
        ParamType = ptUnknown
        Size = 4
        Value = 5
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptUnknown
        Size = 4
      end>
  end
end
