object frmDecontPick: TfrmDecontPick
  Left = 235
  Top = 141
  ActiveControl = PickGrid
  BorderStyle = bsSingle
  Caption = 'Transfer intre Case'
  ClientHeight = 470
  ClientWidth = 636
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnKeyDown = FormKeyDown
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 636
    Height = 34
    Hint = 'Validare Decont'
    Align = alTop
    Alignment = taLeftJustify
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Alegere Decont'
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
      636
      34)
  end
  object pnRest: TPanel
    Left = 0
    Top = 34
    Width = 636
    Height = 366
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 407
    object PickGrid: TdxDBGrid
      Left = 2
      Top = 2
      Width = 632
      Height = 362
      SearchType = stContain
      Bands = <
        item
          Caption = 'Decont'
          Fixed = bfLeft
          Width = 200
        end
        item
          Caption = 'Detalii'
          Width = 419
        end>
      DefaultLayout = False
      HeaderPanelRowCount = 1
      KeyField = 'COD'
      ShowGroupPanel = True
      SummaryGroups = <>
      SummarySeparator = ', '
      Align = alClient
      BorderStyle = bsNone
      TabOrder = 0
      OnDblClick = PickGridDblClick
      OnKeyDown = FormKeyDown
      BandColor = clWindow
      DataSource = DTPickDecont
      Filter.Active = True
      Filter.Criteria = {00000000}
      HeaderColor = clWindow
      LookAndFeel = lfUltraFlat
      OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoDragScroll, edgoMultiSort]
      OptionsCustomize = [edgoBandMoving, edgoBandSizing, edgoColumnMoving, edgoColumnSizing, edgoFullSizing]
      OptionsDB = [edgoCancelOnExit, edgoCanNavigation, edgoLoadAllRecords]
      OptionsView = [edgoHotTrack, edgoIndicator, edgoInvertSelect, edgoUseBitmap]
      ShowBands = True
      ExplicitHeight = 403
      object PickGridCOD: TdxDBGridMaskColumn
        Caption = 'Cod'
        Visible = False
        Width = 42
        BandIndex = 1
        RowIndex = 0
        FieldName = 'COD'
      end
      object PickGridNR_DECONT: TdxDBGridMaskColumn
        Caption = 'Nr Decont'
        Width = 47
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NR_DECONT'
      end
      object PickGridNUME: TdxDBGridMaskColumn
        Caption = 'Nume Repartitor'
        Width = 88
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUME'
      end
      object PickGridDATA_DECONT: TdxDBGridDateColumn
        Caption = 'Data Decont'
        Width = 73
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_DECONT'
      end
      object PickGridAN: TdxDBGridMaskColumn
        Caption = 'An Decont'
        Width = 39
        BandIndex = 1
        RowIndex = 0
        FieldName = 'AN'
      end
      object PickGridLUNA: TdxDBGridMaskColumn
        Caption = 'Luna Decont'
        Width = 26
        BandIndex = 1
        RowIndex = 0
        FieldName = 'LUNA'
      end
      object PickGridCOD_CB: TdxDBGridMaskColumn
        Caption = 'Cod Casa Sosire'
        Visible = False
        Width = 50
        BandIndex = 1
        RowIndex = 0
        FieldName = 'COD_CB'
      end
      object PickGridDENUMIRE_COD_CB: TdxDBGridMaskColumn
        Caption = 'Casa Sosire'
        Width = 101
        BandIndex = 1
        RowIndex = 0
        FieldName = 'DENUMIRE_COD_CB'
      end
      object PickGridCHEIE: TdxDBGridMaskColumn
        Caption = 'Cheie Indexare'
        Visible = False
        Width = 254
        BandIndex = 1
        RowIndex = 0
        FieldName = 'CHEIE'
      end
      object PickGridCODGEST: TdxDBGridMaskColumn
        Caption = 'Id Repartitor'
        Visible = False
        Width = 179
        BandIndex = 1
        RowIndex = 0
        FieldName = 'CODGEST'
      end
      object PickGridCOD_CBT: TdxDBGridMaskColumn
        Caption = 'Cod Casa Plecare'
        Visible = False
        Width = 55
        BandIndex = 1
        RowIndex = 0
        FieldName = 'COD_CBT'
      end
      object PickGridDENUMIRE_COD_CBT: TdxDBGridMaskColumn
        Caption = 'Casa Plecare'
        Width = 93
        BandIndex = 1
        RowIndex = 0
        FieldName = 'DENUMIRE_COD_CBT'
      end
      object PickGridSUMA_DECONT: TdxDBGridMaskColumn
        Caption = 'Suma decont'
        BandIndex = 1
        RowIndex = 0
        FieldName = 'SUMA_DECONT'
      end
      object PickGridCODSECTIE: TdxDBGridMaskColumn
        Caption = 'Cod Scriptic'
        Visible = False
        Width = 42
        BandIndex = 1
        RowIndex = 0
        FieldName = 'CODSECTIE'
      end
      object PickGridJUSTIFICAT: TdxDBGridMaskColumn
        Caption = 'Justificat'
        Width = 57
        BandIndex = 1
        RowIndex = 0
        FieldName = 'JUSTIFICAT'
      end
      object PickGridPROCENT: TdxDBGridMaskColumn
        Caption = 'Procent '
        Width = 39
        BandIndex = 1
        RowIndex = 0
        FieldName = 'PROCENT'
      end
      object PickGridNR_INTRARI: TdxDBGridMaskColumn
        Caption = 'Nr Intrari'
        Visible = False
        Width = 42
        BandIndex = 1
        RowIndex = 0
        FieldName = 'NR_INTRARI'
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 400
    Width = 636
    Height = 70
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = 16776176
    TabOrder = 2
    DesignSize = (
      636
      70)
    object btnOk: TSpeedButton
      Left = 471
      Top = 6
      Width = 75
      Height = 22
      Anchors = [akRight, akBottom]
      Caption = '&OK'
      Flat = True
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000010000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        3333333333333333333333330000333333333333333333333333F33333333333
        00003333344333333333333333388F3333333333000033334224333333333333
        338338F3333333330000333422224333333333333833338F3333333300003342
        222224333333333383333338F3333333000034222A22224333333338F338F333
        8F33333300003222A3A2224333333338F3838F338F33333300003A2A333A2224
        33333338F83338F338F33333000033A33333A222433333338333338F338F3333
        0000333333333A222433333333333338F338F33300003333333333A222433333
        333333338F338F33000033333333333A222433333333333338F338F300003333
        33333333A222433333333333338F338F00003333333333333A22433333333333
        3338F38F000033333333333333A223333333333333338F830000333333333333
        333A333333333333333338330000333333333333333333333333333333333333
        0000}
      NumGlyphs = 2
      ParentFont = False
      OnClick = btnOkClick
    end
    object btnCancel: TSpeedButton
      Left = 552
      Top = 6
      Width = 75
      Height = 22
      Anchors = [akRight, akBottom]
      Caption = '&Cancel'
      Flat = True
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000010000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333333333000033338833333333333333333F333333333333
        0000333911833333983333333388F333333F3333000033391118333911833333
        38F38F333F88F33300003339111183911118333338F338F3F8338F3300003333
        911118111118333338F3338F833338F3000033333911111111833333338F3338
        3333F8330000333333911111183333333338F333333F83330000333333311111
        8333333333338F3333383333000033333339111183333333333338F333833333
        00003333339111118333333333333833338F3333000033333911181118333333
        33338333338F333300003333911183911183333333383338F338F33300003333
        9118333911183333338F33838F338F33000033333913333391113333338FF833
        38F338F300003333333333333919333333388333338FFF830000333333333333
        3333333333333333333888330000333333333333333333333333333333333333
        0000}
      NumGlyphs = 2
      ParentFont = False
      OnClick = btnCancelClick
    end
    object Label1: TLabel
      Left = 13
      Top = 8
      Width = 71
      Height = 13
      Caption = 'Nr Decont : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label2: TLabel
      Left = 152
      Top = 8
      Width = 85
      Height = 13
      Caption = 'Data Decont : '
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edtNrDecont: TdxDBEdit
      Left = 83
      Top = 4
      Width = 62
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 0
      DataField = 'NR_DECONT'
      DataSource = DTPickDecont
      ReadOnly = True
      StyleController = StyleController
      StoredValues = 64
    end
    object edtDataDecont: TdxDBDateEdit
      Left = 236
      Top = 4
      Width = 86
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
      DataField = 'DATA_DECONT'
      DataSource = DTPickDecont
      ReadOnly = True
      StyleController = StyleController
      PopupBorder = pbSingle
      DateButtons = []
      SaveTime = False
      UseEditMask = True
      StoredValues = 68
    end
  end
  object StyleController: TdxEditStyleController
    BorderColor = 14065456
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 358
    Top = 4
  end
  object DTPickDecont: TDataSource
    DataSet = QryPickDecont
    Left = 88
    Top = 106
  end
  object QryPickDecont: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_JUSTIFICARI_DETALIAT :CASA_SOS, :CASA_PLEC, :COD_REP')
    Params = <
      item
        DataType = ftInteger
        Name = 'CASA_SOS'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end
      item
        DataType = ftInteger
        Name = 'CASA_PLEC'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end
      item
        DataType = ftInteger
        Name = 'COD_REP'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end>
    Left = 120
    Top = 104
    ParamData = <
      item
        DataType = ftInteger
        Name = 'CASA_SOS'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end
      item
        DataType = ftInteger
        Name = 'CASA_PLEC'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end
      item
        DataType = ftInteger
        Name = 'COD_REP'
        ParamType = ptUnknown
        Size = -1
        Value = 0
      end>
  end
end
