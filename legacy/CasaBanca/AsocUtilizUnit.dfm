object frmAsocUtilizatori: TfrmAsocUtilizatori
  Left = 343
  Top = 222
  BorderStyle = bsSingle
  ClientHeight = 339
  ClientWidth = 641
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  DesignSize = (
    641
    339)
  PixelsPerInch = 96
  TextHeight = 13
  object HeadPanel1: THeadPanel
    Left = 0
    Top = 0
    Width = 641
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
      641
      41)
  end
  object PageControl: TPageControl
    Left = 0
    Top = 32
    Width = 641
    Height = 249
    ActivePage = pageUtiliz
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 3
    object pageUtiliz: TTabSheet
      Caption = 'Utilizatori'
      ExplicitHeight = 237
      object GridUtilizatori: TdxDBTreeList
        Left = 0
        Top = 0
        Width = 633
        Height = 221
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_UTILIZATORI'
        ParentField = 'ID_UTILIZATORI'
        Align = alClient
        TabOrder = 0
        OnDblClick = GridUtilizatoriDblClick
        OnKeyUp = GridUtilizatoriKeyUp
        DataSource = DTUtilizatori
        LookAndFeel = lfUltraFlat
        OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoUseBitmap, etoUseImageIndexForSelected]
        ShowRoot = False
        TreeLineColor = clGrayText
        ExplicitTop = -3
        ExplicitHeight = 237
        object GridUtilizatoriSEL: TdxDBGridCheckColumn
          Caption = 'Sel'
          HeaderAlignment = taCenter
          Width = 55
          BandIndex = 0
          RowIndex = 0
          FieldName = 'SEL'
          ValueChecked = '1'
          ValueUnchecked = '0'
        end
        object GridUtilizatoriNUME: TdxDBGridMaskColumn
          Caption = 'NumeIntreg'
          DisableEditor = True
          HeaderAlignment = taCenter
          Width = 383
          BandIndex = 0
          RowIndex = 0
          FieldName = 'NUME'
        end
        object GridUtilizatoriADRESA: TdxDBGridMaskColumn
          Caption = 'Nume'
          DisableEditor = True
          HeaderAlignment = taCenter
          Width = 193
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ADRESA'
        end
        object GridUtilizatoriID_REPARTITORI: TdxDBTreeListMaskColumn
          DisableEditor = True
          Visible = False
          Width = 99
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_REPARTITORI'
        end
        object GridUtilizatoriID_PARINTE: TdxDBTreeListMaskColumn
          DisableEditor = True
          Visible = False
          Width = 72
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_PARINTE'
        end
      end
    end
    object pageGrup: TTabSheet
      Caption = 'Grupuri'
      Enabled = False
      ImageIndex = 1
      ExplicitHeight = 237
    end
  end
  object BtnCancel: TcxButton
    Left = 558
    Top = 287
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
    ModalResult = 2
    TabOrder = 1
  end
  object BtnOk: TcxButton
    Left = 474
    Top = 287
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
    Left = 32
    Top = 304
  end
  object DTUtilizatori: TDataSource
    DataSet = TblUtilizatori
    Top = 304
  end
end
