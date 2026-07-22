object FrmListaCasa: TFrmListaCasa
  Tag = 2
  Left = 206
  Top = 354
  Width = 831
  Height = 501
  Caption = 'Lista Casa'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 823
    Height = 33
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Vizualizare %s'
    InfoFont.Charset = DEFAULT_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'MS Sans Serif'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 5
    ActAsCaption = False
    HideCaption = False
    DesignSize = (
      823
      33)
  end
  object pnContent: TPanel
    Left = 0
    Top = 33
    Width = 823
    Height = 437
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    object GridVisual: TdxDBGrid
      Left = 0
      Top = 0
      Width = 823
      Height = 437
      SearchType = stStart
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'ID_LISTA'
      ShowGroupPanel = True
      ShowSummaryFooter = True
      SummaryGroups = <
        item
          DefaultGroup = True
          SummaryItems = <
            item
              ColumnName = 'GridVisualDATA'
              SummaryField = 'PLATI'
              SummaryType = cstSum
            end>
          Name = 'GridVisualSummaryGroup2'
        end>
      SummarySeparator = ', '
      Align = alClient
      TabOrder = 0
      DataSource = DTVisual
      Filter.Active = True
      Filter.AutoDataSetFilter = True
      Filter.Criteria = {00000000}
      HeaderColor = clWindow
      LookAndFeel = lfUltraFlat
      OptionsBehavior = [edgoAnsiSort, edgoAutoSearch, edgoAutoSort, edgoCellMultiSelect, edgoDragScroll, edgoExtMultiSelect, edgoMultiSelect, edgoMultiSort, edgoTabThrough, edgoVertThrough]
      OptionsCustomize = [edgoBandMoving, edgoBandSizing, edgoColumnMoving, edgoColumnSizing, edgoExtCustomizing]
      OptionsDB = [edgoCancelOnExit, edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoSyncSelection, edgoUseBookmarks, edgoUseLocate]
      OptionsView = [edgoBandHeaderWidth, edgoIndicator, edgoInvertSelect, edgoUseBitmap]
      ShowRowFooter = True
      OnChangeNode = GridVisualChangeNode
      OnCustomDraw = GridVisualCustomDraw
      object GridVisualRecId: TdxDBGridColumn
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'RecId'
      end
      object GridVisualID_LISTA: TdxDBGridMaskColumn
        Visible = False
        Width = 526
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_LISTA'
      end
      object GridVisualID_PARINTE: TdxDBGridMaskColumn
        Visible = False
        Width = 526
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_PARINTE'
      end
      object GridVisualCOD_CB: TdxDBGridImageColumn
        Alignment = taRightJustify
        Caption = 'Casa/Banca'
        MinWidth = 16
        Width = 82
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_CB'
        ShowDescription = True
      end
      object GridVisualCOD: TdxDBGridMaskColumn
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD'
      end
      object GridVisualDATA: TdxDBGridDateColumn
        Caption = 'Data'
        Sorted = csUp
        Visible = False
        Width = 97
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA'
        GroupIndex = 0
      end
      object GridVisualTIPDOC: TdxDBGridMaskColumn
        Caption = 'TipDocument'
        Width = 115
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TIPDOC'
      end
      object GridVisualNRDOC: TdxDBGridMaskColumn
        Caption = 'NrDoc'
        Width = 108
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NRDOC'
      end
      object GridVisualPOZ: TdxDBGridMaskColumn
        Caption = 'Poz'
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'POZ'
      end
      object GridVisualEXPLICATIE: TdxDBGridMaskColumn
        Caption = 'Explicatie'
        Width = 161
        BandIndex = 0
        RowIndex = 0
        FieldName = 'EXPLICATIE'
      end
      object GridVisualINCASARI: TdxDBGridMaskColumn
        Caption = 'Incasare'
        Sorted = csUp
        Width = 160
        BandIndex = 0
        RowIndex = 0
        FieldName = 'INCASARI'
      end
      object GridVisualPLATI: TdxDBGridMaskColumn
        Caption = 'Plata'
        Width = 162
        BandIndex = 0
        RowIndex = 0
        FieldName = 'PLATI'
        SummaryFooterType = cstSum
        SummaryFooterField = 'PLATI'
        SummaryType = cstSum
        SummaryField = 'PLATI'
        SummaryGroupName = 'GridVisualSummaryGroup2'
      end
      object GridVisualCODGEST: TdxDBGridImageColumn
        Alignment = taLeftJustify
        Caption = 'Repartitor'
        MinWidth = 16
        Width = 134
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CODGEST'
        ShowDescription = True
      end
      object GridVisualSOLD: TdxDBGridMaskColumn
        Visible = False
        Width = 81
        BandIndex = 0
        RowIndex = 0
        FieldName = 'SOLD'
      end
      object GridVisualCONT_CSP: TdxDBGridMaskColumn
        Caption = 'Cont'
        Width = 87
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CONT_CSP'
      end
      object GridVisualVAL_CRSP: TdxDBGridMaskColumn
        Caption = 'Val Crsp'
        Visible = False
        Width = 81
        BandIndex = 0
        RowIndex = 0
        FieldName = 'VAL_CRSP'
      end
      object GridVisualACHITAT: TdxDBGridMaskColumn
        Caption = 'Achitat'
        Visible = False
        Width = 81
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ACHITAT'
      end
      object GridVisualDATAEM: TdxDBGridDateColumn
        Caption = 'DataEmit'
        Width = 115
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATAEM'
      end
      object GridVisualC_O: TdxDBGridMaskColumn
        Caption = 'Operator'
        Width = 91
        BandIndex = 0
        RowIndex = 0
        FieldName = 'C_O'
      end
      object GridVisualNR_LIST: TdxDBGridMaskColumn
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NR_LIST'
      end
      object GridVisualCURS_SCHIM: TdxDBGridMaskColumn
        Caption = 'Curs'
        Visible = False
        Width = 65
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CURS_SCHIM'
      end
      object GridVisualECL: TdxDBGridMaskColumn
        Caption = 'Ech'
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ECL'
      end
      object GridVisualON_SERVER: TdxDBGridMaskColumn
        Visible = False
        Width = 65
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ON_SERVER'
      end
      object GridVisualSOLD_NOU: TdxDBGridColumn
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'SOLD_NOU'
      end
      object GridVisualSORTFIELD: TdxDBGridColumn
        Visible = False
        Width = 270
        BandIndex = 0
        RowIndex = 0
        FieldName = 'SORTFIELD'
      end
      object GridVisualPEXPLIC: TdxDBGridMemoColumn
        DisableFilter = True
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'PEXPLIC'
      end
      object GridVisualMEXPLIC: TdxDBGridMemoColumn
        DisableFilter = True
        Visible = False
        Width = 56
        BandIndex = 0
        RowIndex = 0
        FieldName = 'MEXPLIC'
      end
      object GridVisualVALIDATA: TdxDBGridMaskColumn
        Caption = 'Este Validata'
        Width = 88
        BandIndex = 0
        RowIndex = 0
        FieldName = 'VALIDATA'
      end
      object GridVisualTRANSFER: TdxDBGridMaskColumn
        Caption = 'Transferata'
        Width = 144
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TRANSFER'
      end
      object GridVisualCOD_CBT: TdxDBGridMaskColumn
        Caption = 'Cod Casa Transfer'
        Visible = False
        Width = 81
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_CBT'
      end
      object GridVisualCOD_TRANSFER: TdxDBGridMaskColumn
        Visible = False
        Width = 81
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_TRANSFER'
      end
      object GridVisualDATA_ACCEPT: TdxDBGridDateColumn
        Visible = False
        Width = 98
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_ACCEPT'
      end
      object GridVisualPARENT_COD: TdxDBGridMaskColumn
        Visible = False
        Width = 70
        BandIndex = 0
        RowIndex = 0
        FieldName = 'PARENT_COD'
      end
      object GridVisualNR_DECONT: TdxDBGridMaskColumn
        Caption = 'Nr Decont'
        Width = 110
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NR_DECONT'
      end
      object GridVisualDATA_DECONT: TdxDBGridDateColumn
        Caption = 'Data Decont'
        Width = 85
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_DECONT'
      end
      object GridVisualID_PROIECT: TdxDBGridImageColumn
        Alignment = taLeftJustify
        Caption = 'Proiect'
        MinWidth = 16
        Width = 118
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_PROIECT'
        ShowDescription = True
      end
      object GridVisualID_TIPURI_CHELTVEN: TdxDBGridImageColumn
        Alignment = taRightJustify
        Caption = 'Tip Cheltuiala'
        MinWidth = 16
        Width = 105
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_TIPURI_CHELTVEN'
        ShowDescription = True
      end
      object GridVisualID_ORGANIGRAMA: TdxDBGridImageColumn
        Alignment = taRightJustify
        Caption = 'Organigrama'
        MinWidth = 16
        Width = 94
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_ORGANIGRAMA'
        ShowDescription = True
      end
      object GridVisualID_RESURSA: TdxDBGridImageColumn
        Alignment = taRightJustify
        Caption = 'Resursa'
        MinWidth = 16
        Width = 88
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_RESURSA'
        ShowDescription = True
      end
    end
  end
  object DTVisual: TDataSource
    DataSet = MemLista
    Left = 8
    Top = 71
  end
  object MemLista: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 42
    Top = 72
    object MemListaID_LISTA: TStringField
      FieldName = 'ID_LISTA'
      Size = 100
    end
    object MemListaID_PARINTE: TStringField
      FieldName = 'ID_PARINTE'
      Size = 100
    end
    object MemListaCOD_CB: TIntegerField
      FieldName = 'COD_CB'
    end
    object MemListaCOD: TIntegerField
      FieldName = 'COD'
    end
    object MemListaCODGEST: TStringField
      FieldName = 'CODGEST'
    end
    object MemListaDATA: TDateTimeField
      FieldName = 'DATA'
    end
    object MemListaTIPDOC: TStringField
      FieldName = 'TIPDOC'
      Size = 3
    end
    object MemListaNRDOC: TStringField
      FieldName = 'NRDOC'
      Size = 10
    end
    object MemListaPOZ: TIntegerField
      FieldName = 'POZ'
    end
    object MemListaEXPLICATIE: TStringField
      FieldName = 'EXPLICATIE'
      Size = 80
    end
    object MemListaINCASARI: TBCDField
      FieldName = 'INCASARI'
      Precision = 14
      Size = 2
    end
    object MemListaPLATI: TBCDField
      FieldName = 'PLATI'
      Precision = 14
      Size = 2
    end
    object MemListaSOLD: TBCDField
      FieldName = 'SOLD'
      Precision = 14
      Size = 2
    end
    object MemListaCONT_CSP: TStringField
      FieldName = 'CONT_CSP'
      Size = 11
    end
    object MemListaVAL_CRSP: TBCDField
      FieldName = 'VAL_CRSP'
      Precision = 14
      Size = 2
    end
    object MemListaACHITAT: TBCDField
      FieldName = 'ACHITAT'
      Precision = 14
      Size = 2
    end
    object MemListaDATAEM: TDateTimeField
      FieldName = 'DATAEM'
    end
    object MemListaC_O: TIntegerField
      FieldName = 'C_O'
    end
    object MemListaNR_LIST: TIntegerField
      FieldName = 'NR_LIST'
    end
    object MemListaCURS_SCHIM: TBCDField
      FieldName = 'CURS_SCHIM'
      Precision = 10
      Size = 2
    end
    object MemListaECL: TWordField
      FieldName = 'ECL'
    end
    object MemListaON_SERVER: TIntegerField
      FieldName = 'ON_SERVER'
    end
    object MemListaSOLD_NOU: TCurrencyField
      FieldKind = fkCalculated
      FieldName = 'SOLD_NOU'
      Calculated = True
    end
    object MemListaSORTFIELD: TStringField
      FieldKind = fkCalculated
      FieldName = 'SORTFIELD'
      Size = 50
      Calculated = True
    end
    object MemListaID_PROIECT: TStringField
      FieldName = 'ID_PROIECT'
    end
    object MemListaPEXPLIC: TMemoField
      FieldName = 'PEXPLIC'
      BlobType = ftMemo
    end
    object MemListaMEXPLIC: TMemoField
      FieldName = 'MEXPLIC'
      BlobType = ftMemo
    end
    object MemListaVALIDATA: TWordField
      FieldName = 'VALIDATA'
    end
    object MemListaTRANSFER: TIntegerField
      FieldName = 'TRANSFER'
    end
    object MemListaCOD_CBT: TIntegerField
      FieldName = 'COD_CBT'
    end
    object MemListaCOD_TRANSFER: TIntegerField
      FieldName = 'COD_TRANSFER'
    end
    object MemListaDATA_ACCEPT: TDateTimeField
      FieldName = 'DATA_ACCEPT'
    end
    object MemListaID_TIPURI_CHELTVEN: TIntegerField
      FieldName = 'ID_TIPURI_CHELTVEN'
    end
    object MemListaPARENT_COD: TIntegerField
      FieldName = 'PARENT_COD'
    end
    object MemListaNR_DECONT: TIntegerField
      FieldName = 'NR_DECONT'
    end
    object MemListaDATA_DECONT: TDateTimeField
      FieldName = 'DATA_DECONT'
    end
    object MemListaID_ORGANIGRAMA: TIntegerField
      FieldName = 'ID_ORGANIGRAMA'
    end
    object MemListaID_RESURSA: TIntegerField
      FieldName = 'ID_RESURSA'
    end
  end
end
