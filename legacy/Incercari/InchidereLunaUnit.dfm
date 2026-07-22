object Form1: TForm1
  Left = 214
  Top = 161
  Width = 845
  Height = 638
  Caption = 'Inchidere Autormata'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    Left = 0
    Top = 570
    Width = 837
    Height = 41
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
  end
  object AtsDBGrid1: TdxDBGrid
    Left = 0
    Top = 0
    Width = 652
    Height = 570
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'key_field'
    ShowGroupPanel = True
    SummaryGroups = <>
    SummarySeparator = ', '
    Align = alClient
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
    TabOrder = 1
    BandFont.Charset = DEFAULT_CHARSET
    BandFont.Color = clWindowText
    BandFont.Height = -11
    BandFont.Name = 'MS Sans Serif'
    BandFont.Style = []
    DataSource = DataSource1
    Filter.Criteria = {00000000}
    HeaderFont.Charset = DEFAULT_CHARSET
    HeaderFont.Color = clWindowText
    HeaderFont.Height = -11
    HeaderFont.Name = 'MS Sans Serif'
    HeaderFont.Style = []
    LookAndFeel = lfFlat
    OptionsDB = [edgoCancelOnExit, edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoUseBookmarks]
    OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
    PreviewFont.Charset = DEFAULT_CHARSET
    PreviewFont.Color = clBlue
    PreviewFont.Height = -11
    PreviewFont.Name = 'MS Sans Serif'
    PreviewFont.Style = []
    object AtsDBGrid1key_field: TdxDBGridMaskColumn
      Visible = False
      Width = 1017
      BandIndex = 0
      RowIndex = 0
      FieldName = 'key_field'
    end
    object AtsDBGrid1Cont_Debit: TdxDBGridMaskColumn
      Width = 117
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Cont_Debit'
    end
    object AtsDBGrid1Cont_Credit: TdxDBGridMaskColumn
      Width = 84
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Cont_Credit'
    end
    object AtsDBGrid1Denumire_Credit: TdxDBGridMaskColumn
      Width = 348
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Denumire_Credit'
    end
    object AtsDBGrid1DENUMIRE_DEBIT: TdxDBGridMaskColumn
      Visible = False
      Width = 5115
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE_DEBIT'
    end
    object AtsDBGrid1grup: TdxDBGridMaskColumn
      Caption = 'Cont de Inchidere'
      Sorted = csUp
      Visible = False
      Width = 5471
      BandIndex = 0
      RowIndex = 0
      FieldName = 'grup'
      GroupIndex = 0
    end
    object AtsDBGrid1Formula: TdxDBGridMaskColumn
      Width = 69
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Formula'
    end
    object AtsDBGrid1Grupa: TdxDBGridMaskColumn
      Caption = 'Mod Inchidere'
      Sorted = csUp
      Visible = False
      Width = 408
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Grupa'
      GroupIndex = 1
    end
  end
  object Panel2: TPanel
    Left = 652
    Top = 0
    Width = 185
    Height = 570
    Align = alRight
    BevelOuter = bvNone
    TabOrder = 2
    object Label1: TLabel
      Left = 40
      Top = 80
      Width = 72
      Height = 20
      Caption = 'Legenda'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -16
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Memo1: TMemo
      Left = 8
      Top = 120
      Width = 169
      Height = 209
      Color = clMoneyGreen
      Ctl3D = False
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Lines.Strings = (
        'SID - sold initial debitor'
        ''
        'SIC - sold initial creditor'
        ''
        'SPD - sold perioada debitor'
        ''
        'SPC - sold perioada creditor'
        ''
        'RD - rulaj debitor'
        ''
        'RC - rulaj creditor'
        ''
        'SD - sold total debitor'
        ''
        'SC - sold total creditor')
      ParentCtl3D = False
      ParentFont = False
      TabOrder = 0
    end
  end
  object DataSource1: TDataSource
    DataSet = ADOQuery1
    Left = 144
    Top = 328
  end
  object ADOQuery1: TZQuery
    Active = True
    Connection = ADOConnection1
    CursorType = ctStatic
    Params = <>
    SQL.Strings = (
      'select '
      '  cont as key_field,'
      '  cont as Cont_Credit, '
      '  romana as Denumire_Credit, '
      
        '  (select romana from cplan where cont like '#39'121.10'#39') AS DENUMIR' +
        'E_DEBIT,'
      '  '#39'121.10'#39' as Cont_Debit, '
      
        '  '#39'121.10 - '#39' + (select romana from cplan where cont like '#39'121.1' +
        '0'#39') as grup,'
      '  '#39'SC'#39' as Formula,'
      '  '#39'[CREDIT] % - 121.10'#39' as Grupa'
      'from cplan '
      'where '
      '  cont like '#39'6%'#39' and cont_level =4 '
      'union all'
      'select '
      '  cont as key_field,'
      '  '#39'121.10'#39' as Cont_Credit, '
      
        '  (select romana from cplan where cont like '#39'121.10'#39') AS Denumir' +
        'e_Credit,'
      '  romana as DENUMIRE_DEBIT, '
      '  cont as Cont_Debit, '
      
        '  '#39'121.10 - '#39' + (select romana from cplan where cont like '#39'121.1' +
        '0'#39') as grup,'
      '  '#39'SD'#39' as Formula,'
      '  '#39'[DEBIT] 121.10 - %'#39' as Grupa'
      'from cplan '
      'where '
      '  cont like '#39'7%'#39' and cont_level =4 ')
    Left = 176
    Top = 328
  end
  object ADOConnection1: TADOConnection
    Connected = True
    ConnectionString = 
      'Provider=SQLOLEDB.1;Password=qazwsx;Persist Security Info=True;U' +
      'ser ID=sa;Initial Catalog=CONTA_ATS;Data Source=(local)'
    LoginPrompt = False
    Provider = 'SQLOLEDB.1'
    Left = 208
    Top = 328
  end
end
