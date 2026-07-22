object FrmTipCheltuiala: TFrmTipCheltuiala
  Left = 200
  Top = 112
  Width = 763
  Height = 544
  Caption = 'Tipuri Cheltuieli/Venituri'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsMDIChild
  OldCreateOrder = False
  Position = poDefault
  Visible = True
  WindowState = wsMaximized
  OnClose = FormClose
  PixelsPerInch = 96
  TextHeight = 13
  object GrTipCheltuieli: TGroupBox
    Left = 0
    Top = 40
    Width = 755
    Height = 442
    Align = alClient
    Caption = 'Tipuri de Cheltuieli / Venituri'
    TabOrder = 0
    object Splitter1: TSplitter
      Left = 622
      Top = 15
      Width = 3
      Height = 425
      Cursor = crHSplit
      Align = alRight
    end
    object TreeCheltuieli: TdxDBTreeList
      Left = 2
      Top = 15
      Width = 620
      Height = 425
      SearchType = stStart
      Bands = <
        item
        end>
      DefaultLayout = False
      HeaderPanelRowCount = 1
      KeyField = 'ID_TIPURI_CHELTVEN'
      ParentField = 'ID_PARINTE'
      Align = alClient
      DragMode = dmAutomatic
      TabOrder = 0
      OnKeyDown = TreeCheltuieliKeyDown
      DataSource = FrmData.DTTipCheltVen
      LookAndFeel = lfFlat
      OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoMultiSelect]
      OptionsDB = [etoCancelOnExit, etoCanDelete, etoCanInsert, etoCanNavigation, etoCheckHasChildren, etoConfirmDelete, etoLoadAllRecords, etoSyncSelection]
      OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoIndicator, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
      ShowGrid = True
      TreeLineColor = clGrayText
      object TreeCheltuieliID_TIPURI_CHELTVEN: TdxDBTreeListMaskColumn
        DisableEditor = True
        Visible = False
        Width = 151
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_TIPURI_CHELTVEN'
      end
      object TreeCheltuieliID_PARINTE: TdxDBTreeListMaskColumn
        DisableEditor = True
        Visible = False
        Width = 53
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_PARINTE'
      end
      object TreeCheltuieliCOD: TdxDBTreeListMaskColumn
        Caption = 'Cod'
        DisableEditor = True
        Width = 59
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD'
        OnGetText = TreeCheltuieliCODGetText
      end
      object TreeCheltuieliDENUMIRE: TdxDBTreeListMaskColumn
        Caption = 'Denumire'
        Width = 242
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DENUMIRE'
      end
      object TreeCheltuieliTIP: TdxDBTreeListImageColumn
        Alignment = taLeftJustify
        Caption = 'Tip'
        MinWidth = 16
        Width = 101
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TIP'
        Descriptions.Strings = (
          'Cheltuiala'
          'Venit')
        ImageIndexes.Strings = (
          '0'
          '1')
        ShowDescription = True
        Values.Strings = (
          '0'
          '1')
      end
      object TreeCheltuieliREALCOD: TdxDBTreeListMaskColumn
        Caption = 'RealCod'
        DisableEditor = True
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD'
      end
    end
    object pn: TPanel
      Left = 625
      Top = 15
      Width = 128
      Height = 425
      Align = alRight
      BevelInner = bvSpace
      BevelOuter = bvLowered
      TabOrder = 1
      object btnAdd: TBitBtn
        Left = 12
        Top = 15
        Width = 107
        Height = 25
        Caption = 'Adauga'
        TabOrder = 0
        OnClick = btnAddClick
      end
      object btnAddChild: TBitBtn
        Left = 12
        Top = 47
        Width = 107
        Height = 25
        Caption = 'Adauga Copil'
        TabOrder = 1
        OnClick = btnAddChildClick
      end
      object btnDelete: TBitBtn
        Left = 12
        Top = 79
        Width = 107
        Height = 25
        Caption = 'Sterge'
        TabOrder = 2
        OnClick = btnDeleteClick
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 482
    Width = 755
    Height = 35
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 1
    object btnOk: TBitBtn
      Left = 671
      Top = 8
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      TabOrder = 0
      Kind = bkOK
    end
  end
  object HeadPanel1: THeadPanel
    Left = 0
    Top = 0
    Width = 755
    Height = 40
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'HeadPanel1'
    TabOrder = 2
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Intretinere Tip Cheltuieli - Venituri'
    InfoFont.Charset = DEFAULT_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'MS Sans Serif'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 8
  end
end
