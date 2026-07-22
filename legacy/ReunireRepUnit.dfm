object frmReunireRep: TfrmReunireRep
  Left = 564
  Top = 105
  BorderStyle = bsDialog
  Caption = 'Reunire Repartitori'
  ClientHeight = 455
  ClientWidth = 591
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  DesignSize = (
    591
    455)
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 8
    Top = 3
    Width = 156
    Height = 13
    Caption = 'Repartitorul curent selectat'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
  end
  object Label2: TLabel
    Left = 8
    Top = 232
    Width = 295
    Height = 13
    Caption = 
      'Lista repartitorilor selectati vor fi tranformati in repartitoru' +
      'l curent'
  end
  object Label3: TLabel
    Left = 8
    Top = 24
    Width = 176
    Height = 13
    Caption = 'Nume / Cod Repartitor / Cod Fiscal : '
  end
  object Label4: TLabel
    Left = 8
    Top = 48
    Width = 91
    Height = 13
    Caption = 'Adresa Repartitor : '
  end
  object TreeRepartitori: TdxTreeList
    Left = 8
    Top = 256
    Width = 574
    Height = 161
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    TabOrder = 0
    LookAndFeel = lfFlat
    Options = [aoColumnSizing, aoColumnMoving, aoRowSelect, aoAutoWidth, aoAutoSort, aoCaseInsensitive]
    OptionsEx = [aoUseBitmap, aoBandHeaderWidth, aoAutoCalcPreviewLines, aoBandSizing, aoBandMoving, aoDragScroll, aoDragExpand, aoAnsiSort, aoAutoSearch]
    TreeLineColor = clGrayText
    ShowRoot = False
    OnDblClick = TreeRepartitoriDblClick
    Anchors = [akLeft, akTop, akRight]
    object TreeRepartitoriNume: TdxTreeListColumn
      Caption = 'Nume'
      HeaderAlignment = taCenter
      Width = 218
      BandIndex = 0
      RowIndex = 0
    end
    object TreeRepartitoriADRESA: TdxTreeListColumn
      Caption = 'Adresa'
      HeaderAlignment = taCenter
      Width = 249
      BandIndex = 0
      RowIndex = 0
    end
    object TreeRepartitoriCOD_FISCAL: TdxTreeListColumn
      Caption = 'Cod Fiscal'
      HeaderAlignment = taCenter
      BandIndex = 0
      RowIndex = 0
    end
  end
  object GridRepartitori: TdxDBTreeList
    Left = 8
    Top = 72
    Width = 574
    Height = 145
    SearchType = stContain
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    TabOrder = 1
    Anchors = [akLeft, akTop, akRight]
    OnDblClick = GridRepartitoriDblClick
    DataSource = DTRepartitori
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    OnChangeNode = GridRepartitoriChangeNode
    object GridRepartitoriNUME: TdxDBTreeListMaskColumn
      Caption = 'Nume'
      HeaderAlignment = taCenter
      Width = 227
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object GridRepartitoriCODSECTIE: TdxDBTreeListMaskColumn
      Caption = 'Cod Rep'
      HeaderAlignment = taCenter
      Width = 76
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODSECTIE'
    end
    object GridRepartitoriADRESA: TdxDBTreeListMaskColumn
      Caption = 'Adresa'
      HeaderAlignment = taCenter
      Width = 193
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object GridRepartitoriCOD_FISCAL: TdxDBTreeListMaskColumn
      Caption = 'Cod Fiscal'
      HeaderAlignment = taCenter
      Width = 75
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_FISCAL'
    end
  end
  object AtsDBEdit1: TdxDBEdit
    Left = 184
    Top = 21
    Width = 201
    TabOrder = 4
    DataField = 'NUME'
  end
  object AtsDBEdit2: TdxDBEdit
    Left = 394
    Top = 21
    Width = 90
    TabOrder = 5
    DataField = 'CODSECTIE'
  end
  object AtsDBEdit3: TdxDBEdit
    Left = 490
    Top = 21
    Width = 90
    TabOrder = 6
    DataField = 'COD_FISCAL'
  end
  object AtsDBEdit4: TdxDBEdit
    Left = 184
    Top = 45
    Width = 396
    TabOrder = 7
    DataField = 'ADRESA'
  end
  object BtnAdauga: TcxButton
    Left = 367
    Top = 227
    Width = 65
    Height = 22
    Caption = 'Adauga'
    TabOrder = 8
    OnClick = BtnAdaugaClick
  end
  object BtnRemove: TcxButton
    Left = 440
    Top = 227
    Width = 65
    Height = 22
    Caption = 'Elimina'
    Enabled = False
    TabOrder = 9
    OnClick = BtnRemoveClick
  end
  object BtnRemoveAll: TcxButton
    Left = 512
    Top = 227
    Width = 65
    Height = 22
    Caption = 'Elimina Tot'
    Enabled = False
    TabOrder = 10
    OnClick = BtnRemoveAllClick
  end
  object BtnCancel: TcxButton
    Left = 504
    Top = 424
    Width = 75
    Height = 25
    Caption = 'Abandon'
    ModalResult = 2
    TabOrder = 2
  end
  object BtnOk: TcxButton
    Left = 424
    Top = 424
    Width = 75
    Height = 25
    Caption = 'Ok'
    TabOrder = 3
    OnClick = BtnOkClick
  end
  object DTRepartitori: TDataSource
    DataSet = QryRepartitori
    Left = 16
    Top = 88
  end
  object QryRepartitori: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT '
      '  ID_REPARTITORI,'
      '  NUME,'
      '  ADRESA,'
      '  COD_FISCAL,'
      '  CODSECTIE, '
      '  ID_PARINTE'
      '  FROM REPARTITORI AS A')
    Params = <>
    Left = 48
    Top = 88
  end
end
