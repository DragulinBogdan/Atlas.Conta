object FrmDefalcareCont: TFrmDefalcareCont
  Left = 220
  Top = 159
  Width = 579
  Height = 388
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object HeadPanel1: THeadPanel
    Left = 0
    Top = 0
    Width = 571
    Height = 34
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'HeadPanel1'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Defalcare Cont'
    InfoFont.Charset = DEFAULT_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'MS Sans Serif'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 8
  end
  object pnClient: TPanel
    Left = 0
    Top = 34
    Width = 571
    Height = 327
    Align = alClient
    BevelOuter = bvLowered
    TabOrder = 1
    object GridDefalcare: TdxDBGrid
      Left = 1
      Top = 1
      Width = 569
      Height = 325
      SearchType = stStart
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'ID_DEFALC'
      SummaryGroups = <>
      SummarySeparator = ', '
      Align = alClient
      TabOrder = 0
      DataSource = DTDefalcCont
      Filter.Active = True
      Filter.AutoDataSetFilter = True
      LookAndFeel = lfUltraFlat
      OptionsDB = [edgoCancelOnExit, edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoUseBookmarks]
      OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
      object GridDefalcareRecId: TdxDBGridColumn
        Visible = False
        Width = 44
        BandIndex = 0
        RowIndex = 0
        FieldName = 'RecId'
      end
      object GridDefalcareID_DEFALC: TdxDBGridMaskColumn
        Visible = False
        Width = 44
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_DEFALC'
      end
      object GridDefalcareBREG_COD: TdxDBGridMaskColumn
        Visible = False
        Width = 45
        BandIndex = 0
        RowIndex = 0
        FieldName = 'BREG_COD'
      end
      object GridDefalcareCONT_CSP: TdxDBGridPopupColumn
        Caption = 'Cont Coresp.'
        Width = 140
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CONT_CSP'
      end
      object GridDefalcareEXPLICATIE: TdxDBGridMaskColumn
        Caption = 'Explicatie'
        Width = 328
        BandIndex = 0
        RowIndex = 0
        FieldName = 'EXPLICATIE'
      end
      object GridDefalcareC_O: TdxDBGridMaskColumn
        Visible = False
        Width = 44
        BandIndex = 0
        RowIndex = 0
        FieldName = 'C_O'
      end
      object GridDefalcareVALOARE: TdxDBGridCurrencyColumn
        Caption = 'Valoare'
        Width = 99
        BandIndex = 0
        RowIndex = 0
        FieldName = 'VALOARE'
      end
    end
  end
  object DTDefalcCont: TDataSource
    DataSet = MemDefalcCont
    Left = 32
    Top = 87
  end
  object MemDefalcCont: TdxMemData
    Active = True
    Indexes = <>
    SortOptions = []
    Left = 64
    Top = 88
    object MemDefalcContID_DEFALC: TAutoIncField
      FieldName = 'ID_DEFALC'
    end
    object MemDefalcContBREG_COD: TIntegerField
      FieldName = 'BREG_COD'
    end
    object MemDefalcContCONT_CSP: TStringField
      FieldName = 'CONT_CSP'
    end
    object MemDefalcContVALOARE: TCurrencyField
      FieldName = 'VALOARE'
    end
    object MemDefalcContEXPLICATIE: TStringField
      FieldName = 'EXPLICATIE'
      Size = 255
    end
    object MemDefalcContC_O: TIntegerField
      FieldName = 'C_O'
    end
  end
end
