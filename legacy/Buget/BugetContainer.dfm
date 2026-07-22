object frmBugetContainer: TfrmBugetContainer
  Left = 446
  Top = 133
  AutoScroll = False
  Caption = 'fmBugetContainer'
  ClientHeight = 501
  ClientWidth = 665
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
  object TreeSelectDirectie: TdxDBTreeList
    Tag = 1
    Left = 16
    Top = 16
    Width = 417
    Height = 185
    SearchType = stContain
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BUGET_DIRECTII'
    ParentField = 'ID_PARINTE'
    BorderStyle = bsNone
    TabOrder = 0
    OnDblClick = TreeSelectDirectieDblClick
    OnKeyDown = TreeSelectDirectieKeyDown
    DataSource = frmData.DTBugetDirectii
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoTabThrough]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoCalcPreviewLines, etoAutoWidth, etoBandHeaderWidth, etoPreview, etoUseBitmap, etoUseImageIndexForSelected]
    PreviewFieldName = 'DESCRIERE'
    TreeLineColor = clGrayText
    TreeLineStyle = tlSolid
    object TreeSelectDirectieID_BUGET_DIRECTII: TdxDBTreeListMaskColumn
      Visible = False
      Width = 309
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_BUGET_DIRECTII'
    end
    object TreeSelectDirectieID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 194
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
    object TreeSelectDirectieDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 257
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeSelectDirectieDESCRIERE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 4227
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DESCRIERE'
    end
    object TreeSelectDirectieATRIBUTII: TdxDBTreeListMaskColumn
      Visible = False
      Width = 4227
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ATRIBUTII'
    end
    object TreeSelectDirectieSTARE: TdxDBTreeListCheckColumn
      Visible = False
      Width = 275
      BandIndex = 0
      RowIndex = 0
      FieldName = 'STARE'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeSelectDirectieDATA_START_FUNCTIONARE: TdxDBTreeListDateColumn
      Caption = 'Infiintare'
      Width = 84
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DATA_START_FUNCTIONARE'
    end
    object TreeSelectDirectieDATA_STOP_FUNCTIONARE: TdxDBTreeListDateColumn
      Caption = 'Desfiintare'
      Width = 69
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DATA_STOP_FUNCTIONARE'
    end
    object TreeSelectDirectieSHAPE_TYPE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 212
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_TYPE'
    end
    object TreeSelectDirectieTIP_ORDONATOR: TdxDBTreeListImageColumn
      Alignment = taRightJustify
      Caption = 'Ordonator'
      MinWidth = 16
      Width = 71
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_BUGET_TIP_ORDONATOR'
    end
    object TreeSelectDirectieSHAPE_LEFT_TOP: TdxDBTreeListMaskColumn
      Visible = False
      Width = 281
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_LEFT_TOP'
    end
    object TreeSelectDirectieSHAPE_RIGHT_BOTT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 325
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_RIGHT_BOTT'
    end
    object TreeSelectDirectieSHAPE_COLOR: TdxDBTreeListMaskColumn
      Visible = False
      Width = 235
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_COLOR'
    end
    object TreeSelectDirectieSHAPE_FONT_COL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 288
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_FONT_COL'
    end
    object TreeSelectDirectieSHAPE_FONT_NAME: TdxDBTreeListMaskColumn
      Visible = False
      Width = 2169
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SHAPE_FONT_NAME'
    end
    object TreeSelectDirectiePOS_ID: TdxDBTreeListMaskColumn
      Visible = False
      Width = 178
      BandIndex = 0
      RowIndex = 0
      FieldName = 'POS_ID'
    end
    object TreeSelectDirectieID_UTILIZATORI: TdxDBTreeListMaskColumn
      Visible = False
      Width = 249
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_UTILIZATORI'
    end
  end
  object pnOrdonantator: TPanel
    Left = 360
    Top = 264
    Width = 265
    Height = 153
    BevelOuter = bvNone
    Color = clSkyBlue
    TabOrder = 1
    DesignSize = (
      265
      153)
    object Label1: TLabel
      Left = 8
      Top = 6
      Width = 62
      Height = 13
      Caption = 'Ordonantator'
    end
    object SpeedButton1: TcxButton
      Left = 241
      Top = 24
      Width = 23
      Height = 22
      Anchors = [akTop, akRight]
      Caption = '+'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -16
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 2
    end
    object inspOrdonantatori: TdxDBInspector
      Left = 0
      Top = 52
      Width = 265
      Height = 101
      Align = alBottom
      Color = clWindow
      DataSource = frmData.dtBugetOrdonantatori
      DefaultFields = False
      TabOrder = 0
      DividerPos = 85
      GridColor = clBtnFace
      PaintStyle = ipsNET
      Anchors = [akLeft, akTop, akRight, akBottom]
      Data = {
        8E00000004000000080000000000000015000000696E73704F72646F6E616E74
        61746F72694E554D45080000000000000018000000696E73704F72646F6E616E
        7461746F72695052454E554D45080000000000000015000000696E73704F7264
        6F6E616E7461746F726944415441080000000000000014000000696E73704F72
        646F6E616E7461746F726954495000000000}
      object inspOrdonantatoriNUME: TdxInspectorDBMaskRow
        Caption = 'Nume'
        FieldName = 'NUME'
      end
      object inspOrdonantatoriPRENUME: TdxInspectorDBMaskRow
        Caption = 'Prenume'
        FieldName = 'PRENUME'
      end
      object inspOrdonantatoriDATA: TdxInspectorDBDateRow
        Caption = 'Data'
        FieldName = 'DATA'
      end
      object inspOrdonantatoriTIP: TdxInspectorDBMaskRow
        Caption = 'Tip'
        FieldName = 'TIP'
      end
    end
    object ieOrdonantatori: TdxDBImageEdit
      Left = 2
      Top = 25
      Width = 237
      Style.BorderStyle = xbsFlat
      Style.ButtonStyle = btsDefault
      Style.ButtonTransparence = ebtNone
      Style.Edges = [edgLeft, edgTop, edgRight, edgBottom]
      Style.Shadow = False
      TabOrder = 1
      Anchors = [akLeft, akTop, akRight]
      Alignment = taLeftJustify
      DataField = 'ID_BUGET_ORDONANTATORI'
      DataSource = frmData.DTBugetDirectii
      StoredValues = 1
    end
  end
end
