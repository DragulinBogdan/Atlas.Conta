object frmFunctieRep: TfrmFunctieRep
  Left = 343
  Top = 222
  BorderStyle = bsSingle
  ClientHeight = 339
  ClientWidth = 689
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  DesignSize = (
    689
    339)
  PixelsPerInch = 96
  TextHeight = 13
  object HeadPanel1: THeadPanel
    Left = 0
    Top = 0
    Width = 689
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Asociere Utilizator(i)'
    InfoFont.Charset = DEFAULT_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'MS Sans Serif'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 8
    ActAsCaption = False
    HideCaption = False
    DesignSize = (
      689
      41)
  end
  object GridUtilizatori: TdxDBTreeList
    Left = 0
    Top = 41
    Width = 689
    Height = 216
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    Align = alTop
    TabOrder = 3
    Anchors = [akLeft, akTop, akRight, akBottom]
    OnDblClick = GridUtilizatoriDblClick
    OnKeyUp = GridUtilizatoriKeyUp
    DataSource = DTUtilizatori
    HeaderColor = clWindow
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object GridUtilizatoriSEL: TdxDBGridCheckColumn
      Caption = 'Sel'
      HeaderAlignment = taCenter
      Width = 40
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SEL'
      ValueChecked = '1'
      ValueUnchecked = '0'
    end
    object GridUtilizatoriNUME: TdxDBGridMaskColumn
      Caption = 'NumeIntreg'
      HeaderAlignment = taCenter
      Width = 200
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object GridUtilizatoriADRESA: TdxDBGridMaskColumn
      Caption = 'Nume'
      HeaderAlignment = taCenter
      Width = 100
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object GridUtilizatoriID_REPARTITORI: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_REPARTITORI'
    end
    object GridUtilizatoriID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
  end
  object BtnCancel: TcxButton
    Left = 570
    Top = 263
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
    ModalResult = 2
    TabOrder = 1
  end
  object BtnOk: TcxButton
    Left = 474
    Top = 263
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    ModalResult = 1
    TabOrder = 2
  end
  object TblUtilizatori: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 512
  end
  object DTUtilizatori: TDataSource
    DataSet = TblUtilizatori
    Left = 480
  end
end
