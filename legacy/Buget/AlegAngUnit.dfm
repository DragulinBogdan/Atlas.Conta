object frmBugetAlegAng: TfrmBugetAlegAng
  Left = 285
  Top = 151
  Caption = 'Alege Angajament'
  ClientHeight = 464
  ClientWidth = 710
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 710
    Height = 49
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Alegere Angajament'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Alegere Angajament'
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
      710
      49)
  end
  object pnbottom: TPanel
    Left = 0
    Top = 384
    Width = 710
    Height = 80
    Align = alBottom
    Color = 15788249
    Ctl3D = False
    ParentCtl3D = False
    TabOrder = 1
    DesignSize = (
      710
      80)
    object btnOk: TcxButton
      Left = 608
      Top = 16
      Width = 75
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Ok'
      TabOrder = 0
      OnClick = btnOkClick
    end
  end
  object tabGrid: TTabControl
    Left = 0
    Top = 49
    Width = 710
    Height = 335
    Align = alClient
    HotTrack = True
    MultiLine = True
    Style = tsFlatButtons
    TabOrder = 2
    TabWidth = 150
    ExplicitHeight = 374
    object GridAngajamente: TdxDBGrid
      Left = 4
      Top = 6
      Width = 702
      Height = 325
      SearchType = stContain
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'ID_ANGAJAMENT'
      ShowGroupPanel = True
      SummaryGroups = <>
      SummarySeparator = ', '
      Align = alClient
      TabOrder = 0
      OnDblClick = GridAngajamenteDblClick
      OnKeyUp = GridAngajamenteKeyUp
      DataSource = DTAngajamente
      Filter.Criteria = {00000000}
      LookAndFeel = lfFlat
      OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoDblClick, edgoDragScroll, edgoImmediateEditor, edgoTabThrough, edgoVertThrough]
      OptionsDB = [edgoCancelOnExit, edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoUseBookmarks]
      OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
      ExplicitHeight = 364
      object GridAngajamenteNUMAR: TdxDBGridMaskColumn
        Caption = 'Numar'
        HeaderAlignment = taCenter
        Width = 52
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUMAR'
      end
      object GridAngajamenteDATA_EMITERE: TdxDBGridDateColumn
        Caption = 'Data'
        HeaderAlignment = taCenter
        Width = 94
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_EMITERE'
      end
      object GridAngajamenteID_DEPARTAMENT: TdxDBGridImageColumn
        Alignment = taLeftJustify
        Caption = 'IdDepartament'
        HeaderAlignment = taCenter
        MinWidth = 16
        Visible = False
        Width = 54
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_DEPARTAMENT'
        ShowDescription = True
      end
      object GridAngajamenteSCOPUL: TdxDBGridColumn
        Caption = 'Descriere'
        Width = 54
        BandIndex = 0
        RowIndex = 0
        FieldName = 'SCOPUL'
      end
      object GridAngajamenteID_LST_REPARTITORI: TdxDBGridMaskColumn
        Caption = 'IdSocietate'
        HeaderAlignment = taCenter
        Visible = False
        Width = 51
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_LST_REPARTITORI'
      end
      object GridAngajamenteID_ANGAJAMENT: TdxDBGridMaskColumn
        Caption = 'Identificator'
        HeaderAlignment = taCenter
        Visible = False
        Width = 219
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_ANGAJAMENT'
      end
      object GridAngajamenteCLASA_FUNCTIONALA: TdxDBGridColumn
        Caption = 'Clasa Functionala'
        HeaderAlignment = taCenter
        Width = 134
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CLASA_FUNCTIONALA'
      end
      object GridAngajamenteTIP_ANGAJAMENT: TdxDBGridImageColumn
        Alignment = taLeftJustify
        Caption = 'Tip Angajament'
        HeaderAlignment = taCenter
        MinWidth = 16
        Width = 43
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TIP_ANGAJAMENT'
        ShowDescription = True
      end
      object GridAngajamenteID_UTILIZATORI: TdxDBGridImageColumn
        Alignment = taLeftJustify
        Caption = 'Utilizator'
        HeaderAlignment = taCenter
        MinWidth = 16
        Width = 45
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_UTILIZATORI'
        ShowDescription = True
      end
      object GridAngajamenteNUME_ID_DEPARTAMENT: TdxDBGridColumn
        Caption = 'Departament'
        HeaderAlignment = taCenter
        Width = 105
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUME_ID_DEPARTAMENT'
      end
      object GridAngajamenteNUME_ID_LST_REPARTITORI: TdxDBGridColumn
        Caption = 'Societate'
        HeaderAlignment = taCenter
        Width = 157
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUME_ID_LST_REPARTITORI'
      end
    end
  end
  object DTAngajamente: TDataSource
    DataSet = QryAngajament
    Left = 240
    Top = 1
  end
  object QryAngajament: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_BUGET_ALEG_ANGAJAMENTE')
    Params = <>
    Left = 272
    Top = 1
  end
end
