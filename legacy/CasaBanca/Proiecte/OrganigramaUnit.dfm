object FrmOrganigrama: TFrmOrganigrama
  Left = 303
  Top = 255
  Caption = 'Organigrama'
  ClientHeight = 329
  ClientWidth = 458
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  PixelsPerInch = 96
  TextHeight = 13
  object pnInfo: THeadPanel
    Left = 0
    Top = 0
    Width = 458
    Height = 49
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'pnInfo'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Intretinere Organigrama Functii'
    InfoFont.Charset = DEFAULT_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -19
    InfoFont.Name = 'MS Sans Serif'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 8
    ActAsCaption = True
    HideCaption = False
    DesignSize = (
      458
      49)
  end
  object pnTot: TPanel
    Left = 0
    Top = 49
    Width = 458
    Height = 207
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    ExplicitHeight = 252
    object GridFunctii: TdxDBGrid
      Left = 0
      Top = 0
      Width = 458
      Height = 207
      SearchType = stStart
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      SummaryGroups = <>
      SummarySeparator = ', '
      Align = alClient
      TabOrder = 0
      OnDblClick = GridFunctiiDblClick
      DataSource = frmData.DTOrganigrama
      Filter.Criteria = {00000000}
      HeaderColor = clWindow
      LookAndFeel = lfUltraFlat
      OptionsDB = [edgoCancelOnExit, edgoCanDelete, edgoCanInsert, edgoCanNavigation, edgoConfirmDelete, edgoLoadAllRecords, edgoUseBookmarks]
      OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
      ExplicitHeight = 252
      object GridFunctiiID_ORGANIGRAMA: TdxDBGridMaskColumn
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_ORGANIGRAMA'
      end
      object GridFunctiiDENUMIRE: TdxDBGridMaskColumn
        Caption = 'Denumire'
        Sorted = csUp
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DENUMIRE'
      end
      object GridFunctiiID_PARINTE: TdxDBGridMaskColumn
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_PARINTE'
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 256
    Width = 458
    Height = 73
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 2
    DesignSize = (
      458
      73)
    object btnOk: TSpeedButton
      Left = 375
      Top = 2
      Width = 75
      Height = 25
      Anchors = [akTop, akRight]
      Caption = '&Ok'
      Flat = True
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        0400000000000001000000000000000000001000000010000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
        555555555555555555555555555555555555555555FF55555555555559055555
        55555555577FF5555555555599905555555555557777F5555555555599905555
        555555557777FF5555555559999905555555555777777F555555559999990555
        5555557777777FF5555557990599905555555777757777F55555790555599055
        55557775555777FF5555555555599905555555555557777F5555555555559905
        555555555555777FF5555555555559905555555555555777FF55555555555579
        05555555555555777FF5555555555557905555555555555777FF555555555555
        5990555555555555577755555555555555555555555555555555}
      NumGlyphs = 2
      OnClick = btnOkClick
    end
  end
end
