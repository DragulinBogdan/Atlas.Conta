object frmCBValidare: TfrmCBValidare
  Left = 323
  Top = 196
  Width = 766
  Height = 502
  Caption = 'frmCBValidare'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object PageControl: TcxTabControl
    Left = 0
    Top = 0
    Width = 758
    Height = 21
    Align = alTop
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    ParentFont = False
    TabIndex = 0
    TabOrder = 0
    Tabs.Strings = (
      'Toate'
      'Inregistrari Casa'
      'Inregistrari Banca'
      'Justificari')
    TabStop = False
    ClientRectBottom = 22
    ClientRectLeft = 2
    ClientRectRight = 756
    ClientRectTop = 22
  end
  object pnClient: TPanel
    Left = 0
    Top = 21
    Width = 758
    Height = 428
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 1
    TabStop = True
    object cxGrid1: TcxGrid
      Left = 0
      Top = 0
      Width = 758
      Height = 272
      Align = alClient
      BevelOuter = bvNone
      BorderStyle = cxcbsNone
      TabOrder = 0
      object cxGrid1DBTableView1: TcxGridDBTableView
        NavigatorButtons.ConfirmDelete = False
        DataController.DataSource = DTVisualizare
        DataController.DetailKeyFieldNames = 'COD_CB'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        object cxGrid1DBTableView1COD_CB: TcxGridDBColumn
          DataBinding.FieldName = 'COD_CB'
        end
        object cxGrid1DBTableView1DENUMIRE: TcxGridDBColumn
          DataBinding.FieldName = 'DENUMIRE'
        end
        object cxGrid1DBTableView1CRSP_LEI: TcxGridDBColumn
          DataBinding.FieldName = 'CRSP_LEI'
          Width = 50
        end
        object cxGrid1DBTableView1DENV: TcxGridDBColumn
          DataBinding.FieldName = 'DENV'
        end
        object cxGrid1DBTableView1C_O: TcxGridDBColumn
          DataBinding.FieldName = 'C_O'
        end
        object cxGrid1DBTableView1SOLDINI_D: TcxGridDBColumn
          DataBinding.FieldName = 'SOLDINI_D'
        end
        object cxGrid1DBTableView1SOLDINI_C: TcxGridDBColumn
          DataBinding.FieldName = 'SOLDINI_C'
        end
        object cxGrid1DBTableView1DATA_SOLD: TcxGridDBColumn
          DataBinding.FieldName = 'DATA_SOLD'
        end
        object cxGrid1DBTableView1CASIER: TcxGridDBColumn
          DataBinding.FieldName = 'CASIER'
        end
        object cxGrid1DBTableView1DEFALCATOR: TcxGridDBColumn
          DataBinding.FieldName = 'DEFALCATOR'
        end
        object cxGrid1DBTableView1ADMIN: TcxGridDBColumn
          DataBinding.FieldName = 'ADMIN'
        end
        object cxGrid1DBTableView1IS_BANCA: TcxGridDBColumn
          DataBinding.FieldName = 'IS_BANCA'
        end
        object cxGrid1DBTableView1IS_AVANS: TcxGridDBColumn
          DataBinding.FieldName = 'IS_AVANS'
        end
        object cxGrid1DBTableView1IS_TEMPOR: TcxGridDBColumn
          DataBinding.FieldName = 'IS_TEMPOR'
        end
        object cxGrid1DBTableView1ID_REPARTITORI: TcxGridDBColumn
          DataBinding.FieldName = 'ID_REPARTITORI'
        end
      end
      object cxGrid1DBTableView2: TcxGridDBTableView
        NavigatorButtons.ConfirmDelete = False
        DataController.DataSource = DataSource1
        DataController.DetailKeyFieldNames = 'COD_CB'
        DataController.KeyFieldNames = 'COD'
        DataController.MasterKeyFieldNames = 'COD_CB'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        object cxGrid1DBTableView2COD_CB: TcxGridDBColumn
          DataBinding.FieldName = 'COD_CB'
        end
        object cxGrid1DBTableView2COD: TcxGridDBColumn
          DataBinding.FieldName = 'COD'
        end
        object cxGrid1DBTableView2CODGEST: TcxGridDBColumn
          DataBinding.FieldName = 'CODGEST'
        end
        object cxGrid1DBTableView2DATA: TcxGridDBColumn
          DataBinding.FieldName = 'DATA'
        end
        object cxGrid1DBTableView2TIPDOC: TcxGridDBColumn
          DataBinding.FieldName = 'TIPDOC'
        end
        object cxGrid1DBTableView2NRDOC: TcxGridDBColumn
          DataBinding.FieldName = 'NRDOC'
        end
        object cxGrid1DBTableView2POZ: TcxGridDBColumn
          DataBinding.FieldName = 'POZ'
        end
        object cxGrid1DBTableView2EXPLICATIE: TcxGridDBColumn
          DataBinding.FieldName = 'EXPLICATIE'
        end
        object cxGrid1DBTableView2INCASARI: TcxGridDBColumn
          DataBinding.FieldName = 'INCASARI'
        end
        object cxGrid1DBTableView2PLATI: TcxGridDBColumn
          DataBinding.FieldName = 'PLATI'
        end
        object cxGrid1DBTableView2SOLD: TcxGridDBColumn
          DataBinding.FieldName = 'SOLD'
        end
        object cxGrid1DBTableView2CONT_CSP: TcxGridDBColumn
          DataBinding.FieldName = 'CONT_CSP'
        end
        object cxGrid1DBTableView2VAL_CRSP: TcxGridDBColumn
          DataBinding.FieldName = 'VAL_CRSP'
        end
        object cxGrid1DBTableView2ACHITAT: TcxGridDBColumn
          DataBinding.FieldName = 'ACHITAT'
        end
        object cxGrid1DBTableView2DATAEM: TcxGridDBColumn
          DataBinding.FieldName = 'DATAEM'
        end
        object cxGrid1DBTableView2C_O: TcxGridDBColumn
          DataBinding.FieldName = 'C_O'
        end
        object cxGrid1DBTableView2NR_LIST: TcxGridDBColumn
          DataBinding.FieldName = 'NR_LIST'
        end
        object cxGrid1DBTableView2MEXPLIC: TcxGridDBColumn
          DataBinding.FieldName = 'MEXPLIC'
        end
        object cxGrid1DBTableView2CURS_SCHIM: TcxGridDBColumn
          DataBinding.FieldName = 'CURS_SCHIM'
        end
        object cxGrid1DBTableView2SOLD_INITIAL: TcxGridDBColumn
          DataBinding.FieldName = 'SOLD_INITIAL'
        end
        object cxGrid1DBTableView2COD_ARHIVA: TcxGridDBColumn
          DataBinding.FieldName = 'COD_ARHIVA'
        end
        object cxGrid1DBTableView2ECL: TcxGridDBColumn
          DataBinding.FieldName = 'ECL'
        end
        object cxGrid1DBTableView2VALIDATA: TcxGridDBColumn
          DataBinding.FieldName = 'VALIDATA'
        end
        object cxGrid1DBTableView2TRANSFER: TcxGridDBColumn
          DataBinding.FieldName = 'TRANSFER'
        end
        object cxGrid1DBTableView2COD_CBT: TcxGridDBColumn
          DataBinding.FieldName = 'COD_CBT'
        end
        object cxGrid1DBTableView2COD_TRANSFER: TcxGridDBColumn
          DataBinding.FieldName = 'COD_TRANSFER'
        end
        object cxGrid1DBTableView2NR_DECONT: TcxGridDBColumn
          DataBinding.FieldName = 'NR_DECONT'
        end
        object cxGrid1DBTableView2DATA_DECONT: TcxGridDBColumn
          DataBinding.FieldName = 'DATA_DECONT'
        end
        object cxGrid1DBTableView2PARENT_COD: TcxGridDBColumn
          DataBinding.FieldName = 'PARENT_COD'
        end
        object cxGrid1DBTableView2V_O: TcxGridDBColumn
          DataBinding.FieldName = 'V_O'
        end
        object cxGrid1DBTableView2VALIDATION_HASH: TcxGridDBColumn
          DataBinding.FieldName = 'VALIDATION_HASH'
        end
      end
      object cxGrid1DBCardView1: TcxGridDBCardView
        NavigatorButtons.ConfirmDelete = False
        DataController.DataSource = DataSource2
        DataController.DetailKeyFieldNames = 'COD_CB'
        DataController.KeyFieldNames = 'COD_CB'
        DataController.MasterKeyFieldNames = 'COD_CB'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        LayoutDirection = ldVertical
        object cxGrid1DBCardView1COD_CB: TcxGridDBCardViewRow
          DataBinding.FieldName = 'COD_CB'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1DENUMIRE: TcxGridDBCardViewRow
          DataBinding.FieldName = 'DENUMIRE'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1CRSP_LEI: TcxGridDBCardViewRow
          DataBinding.FieldName = 'CRSP_LEI'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1DENV: TcxGridDBCardViewRow
          DataBinding.FieldName = 'DENV'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1C_O: TcxGridDBCardViewRow
          DataBinding.FieldName = 'C_O'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1SOLDINI_D: TcxGridDBCardViewRow
          DataBinding.FieldName = 'SOLDINI_D'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1SOLDINI_C: TcxGridDBCardViewRow
          DataBinding.FieldName = 'SOLDINI_C'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1DATA_SOLD: TcxGridDBCardViewRow
          DataBinding.FieldName = 'DATA_SOLD'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1CASIER: TcxGridDBCardViewRow
          DataBinding.FieldName = 'CASIER'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1DEFALCATOR: TcxGridDBCardViewRow
          DataBinding.FieldName = 'DEFALCATOR'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1ADMIN: TcxGridDBCardViewRow
          DataBinding.FieldName = 'ADMIN'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1IS_BANCA: TcxGridDBCardViewRow
          DataBinding.FieldName = 'IS_BANCA'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1IS_AVANS: TcxGridDBCardViewRow
          DataBinding.FieldName = 'IS_AVANS'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1IS_TEMPOR: TcxGridDBCardViewRow
          DataBinding.FieldName = 'IS_TEMPOR'
          Position.BeginsLayer = True
        end
        object cxGrid1DBCardView1ID_REPARTITORI: TcxGridDBCardViewRow
          DataBinding.FieldName = 'ID_REPARTITORI'
          Position.BeginsLayer = True
        end
      end
      object cxGrid1Level1: TcxGridLevel
        GridView = cxGrid1DBTableView1
        Options.DetailTabsPosition = dtpTop
        object cxGrid1Level2: TcxGridLevel
          Caption = 'TESTE'
          GridView = cxGrid1DBTableView2
        end
        object cxGrid1Level3: TcxGridLevel
          Caption = 'TTTT'
          GridView = cxGrid1DBCardView1
        end
      end
    end
    object pnExplicatie: TPanel
      Left = 0
      Top = 272
      Width = 758
      Height = 156
      Align = alBottom
      BevelOuter = bvNone
      Color = clGray
      TabOrder = 1
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 449
    Width = 758
    Height = 26
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 2
    DesignSize = (
      758
      26)
    object btnOk: TSpeedButton
      Left = 598
      Top = 4
      Width = 74
      Height = 20
      Anchors = [akRight, akBottom]
      Caption = 'Ok'
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
    end
    object btnCancel: TSpeedButton
      Left = 678
      Top = 4
      Width = 75
      Height = 20
      Anchors = [akRight, akBottom]
      Caption = 'Cancel'
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
    end
  end
  object DTVisualizare: TDataSource
    DataSet = QryVizualizare
    Left = 8
    Top = 24
  end
  object QryVizualizare: TZQuery
    Active = True
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    Params = <>
    SQL.Strings = (
      'SELECT * FROM CASIERIE')
    Left = 40
    Top = 24
  end
  object DataSource1: TDataSource
    DataSet = QryLevel2
    Left = 8
    Top = 64
  end
  object QryLevel2: TZQuery
    Active = True
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    Params = <>
    SQL.Strings = (
      'SELECT  * FROM BREGISTRU')
    Left = 40
    Top = 64
  end
  object DataSource2: TDataSource
    DataSet = ADOQuery1
    Left = 8
    Top = 96
  end
  object ADOQuery1: TZQuery
    Active = True
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    Params = <>
    SQL.Strings = (
      'SELECT  * FROM CASIERIE')
    Left = 40
    Top = 96
  end
end
