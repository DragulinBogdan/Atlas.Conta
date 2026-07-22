object frmDefalcareDecontare: TfrmDefalcareDecontare
  Left = 209
  Top = 169
  Caption = 'Completare defalcar decontare'
  ClientHeight = 398
  ClientWidth = 658
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  DesignSize = (
    658
    398)
  PixelsPerInch = 96
  TextHeight = 13
  object Label2: TLabel
    Left = 12
    Top = 371
    Width = 80
    Height = 13
    Anchors = [akLeft, akBottom]
    Caption = 'Total de defalcat'
  end
  object Label3: TLabel
    Left = 204
    Top = 371
    Width = 71
    Height = 13
    Anchors = [akLeft, akBottom]
    Caption = 'Total defalcat :'
  end
  object Panel1: TPanel
    Left = 1
    Top = 2
    Width = 655
    Height = 41
    Anchors = [akLeft, akTop, akRight]
    BevelInner = bvLowered
    TabOrder = 5
    DesignSize = (
      655
      41)
    object Label1: TLabel
      Left = 6
      Top = 6
      Width = 202
      Height = 13
      Caption = 'Specificati defalcarea pentru documentul : '
    end
    object LbInfoDocum: TLabel
      Left = 6
      Top = 22
      Width = 639
      Height = 13
      Anchors = [akLeft, akTop, akRight]
      AutoSize = False
    end
  end
  object BtnCancel: TBitBtn
    Left = 572
    Top = 367
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Cancel = True
    Caption = 'Cancel'
    ModalResult = 2
    NumGlyphs = 2
    TabOrder = 0
  end
  object BtnOk: TBitBtn
    Left = 492
    Top = 367
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    TabOrder = 1
    OnClick = BtnOkClick
  end
  object GridDefalcare: TdxDBGrid
    Left = 8
    Top = 48
    Width = 642
    Height = 308
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'RecId'
    ShowSummaryFooter = True
    SummaryGroups = <>
    SummarySeparator = ', '
    TabOrder = 2
    DataSource = DTDefalcareDecontare
    Filter.Criteria = {00000000}
    LookAndFeel = lfFlat
    OptionsDB = [edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoUseBookmarks]
    OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
    Anchors = [akLeft, akTop, akRight, akBottom]
    object GridDefalcareTIPMAT: TdxDBGridMaskColumn
      Caption = 'Tip Mat.'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 71
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TIPMAT'
    end
    object GridDefalcareDENMAT: TdxDBGridMaskColumn
      Caption = 'Den Mat'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 117
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENMAT'
    end
    object GridDefalcareCANTITATE: TdxDBGridCurrencyColumn
      Caption = 'Cant.'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 40
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CANTITATE'
      Nullable = False
    end
    object GridDefalcarePRET_UNITAR: TdxDBGridCurrencyColumn
      Caption = 'Pret Unitar'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 83
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PRET_UNITAR'
      Nullable = False
    end
    object GridDefalcarePRET_LIVRARE_TVA: TdxDBGridCurrencyColumn
      Caption = 'Valoare'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 113
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PRET_LIVRARE_TVA'
      Nullable = False
    end
    object GridDefalcareTOTAL: TdxDBGridCurrencyColumn
      Caption = 'Decontat'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 103
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TOTAL'
      Nullable = False
    end
    object GridDefalcareCURENT: TdxDBGridCurrencyColumn
      Caption = 'Suma'
      HeaderAlignment = taCenter
      Width = 89
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CURENT'
      SummaryFooterType = cstSum
      SummaryFooterFormat = ',0.00;-,0.00'
      Nullable = False
      SummaryType = cstSum
      SummaryFormat = ',0.00;-,0.00'
    end
  end
  object edInainte: TdxCurrencyEdit
    Left = 99
    Top = 368
    Width = 100
    TabOrder = 3
    Anchors = [akLeft, akBottom]
    Alignment = taRightJustify
    ReadOnly = True
    DisplayFormat = ',0.00;-,0.00'
    StoredValues = 65
  end
  object edTotal: TdxCurrencyEdit
    Left = 278
    Top = 368
    Width = 100
    TabOrder = 4
    Anchors = [akLeft, akBottom]
    Alignment = taRightJustify
    ReadOnly = True
    DisplayFormat = ',0.00;-,0.00'
    StoredValues = 65
  end
  object DTDefalcareDecontare: TDataSource
    DataSet = DBDefalcareDecont
    Left = 16
    Top = 80
  end
  object DBDefalcareDecont: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 48
    Top = 80
  end
end
