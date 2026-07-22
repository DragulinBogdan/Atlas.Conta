object CustomReport: TCustomReport
  Left = 447
  Top = 66
  ClientHeight = 289
  ClientWidth = 485
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = True
  Position = poScreenCenter
  ShowHint = True
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object TreePlanConturi: TdxDBTreeList
    Left = 136
    Top = 112
    Width = 313
    Height = 161
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'CONT'
    ParentField = 'PARINTE'
    TabOrder = 0
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTPlanCont
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoTabThrough]
    OptionsCustomize = [etoColumnSizing]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreePlanConturiCONT: TdxDBTreeListMaskColumn
      Caption = 'Cont'
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 77
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT'
    end
    object TreePlanConturiROMANA: TdxDBTreeListMaskColumn
      Caption = 'Romana'
      HeaderAlignment = taCenter
      Width = 218
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ROMANA'
    end
  end
  object TreeEconomic: TdxDBTreeList
    Left = 64
    Top = 72
    Width = 313
    Height = 177
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_ECONOMIC'
    ParentField = 'ID_PARINTE'
    TabOrder = 1
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTBGPlanEconomic
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoTabThrough]
    OptionsCustomize = [etoColumnSizing]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn
      Caption = 'Cod Eco.'
      HeaderAlignment = taCenter
      Width = 120
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_ECONOMIC'
    end
    object TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      HeaderAlignment = taCenter
      Width = 191
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
  end
  object TreeFunctional: TdxDBTreeList
    Left = 24
    Top = 24
    Width = 321
    Height = 201
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    ParentField = 'ID_PARINTE'
    TabOrder = 2
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTBGPlanFunctional
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoTabThrough]
    OptionsCustomize = [etoColumnSizing]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn
      Caption = 'Cod Func.'
      HeaderAlignment = taCenter
      Width = 118
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_FUNCTIONAL'
    end
    object TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      HeaderAlignment = taCenter
      Width = 193
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
  end
  object TreeRepartitori: TdxDBTreeList
    Left = 75
    Top = -63
    Width = 409
    Height = 217
    SearchType = stContain
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    TabOrder = 3
    Visible = False
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTRepartitori
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoIndicator, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeRepartitoriNUME: TdxDBTreeListMaskColumn
      Caption = 'Nume'
      HeaderAlignment = taCenter
      Width = 141
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object TreeRepartitoriADRESA: TdxDBTreeListMaskColumn
      Caption = 'Adresa'
      HeaderAlignment = taCenter
      Width = 127
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object TreeRepartitoriCODREP: TdxDBTreeListMaskColumn
      Caption = 'Cod'
      HeaderAlignment = taCenter
      Width = 74
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODREP'
    end
    object TreeRepartitoriCONT: TdxDBTreeListMaskColumn
      Caption = 'Cont'
      HeaderAlignment = taCenter
      Width = 53
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT'
    end
  end
  object TreeProiecte: TdxDBTreeList
    Left = 120
    Top = 0
    Width = 321
    Height = 201
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_OI_PROIECTE'
    ParentField = 'ID_PARINTE'
    TabOrder = 4
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTOIProiecte
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoTabThrough]
    OptionsCustomize = [etoColumnSizing]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeProiecteID_OI_PROIECTE: TdxDBTreeListMaskColumn
      Alignment = taLeftJustify
      Caption = 'Cod'
      Width = 40
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_OI_PROIECTE'
    end
    object TreeProiecteDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 279
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
  end
  object TreeUnitati: TdxDBTreeList
    Left = 128
    Top = 40
    Width = 321
    Height = 201
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_OI_UNITATI'
    ParentField = 'ID_PARINTE'
    TabOrder = 5
    OnDblClick = TreePlanConturiDblClick
    OnKeyDown = TreePlanConturiKeyDown
    DataSource = frmData.DTOIUnitati
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoTabThrough]
    OptionsCustomize = [etoColumnSizing]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeUnitatiID_OI_UNITATI: TdxDBTreeListMaskColumn
      Alignment = taLeftJustify
      Caption = 'Cod'
      Width = 70
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_OI_UNITATI'
    end
    object TreeUnitatiDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 154
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeUnitatiCOD_FUNCTIONAL: TdxDBTreeListColumn
      Caption = 'Funct'
      Width = 79
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_FUNCTIONAL'
    end
  end
  object dsTable: TDataSource
    DataSet = QryTable
    Left = 42
    Top = 75
  end
  object plTable: TppBDEPipeline
    DataSource = dsTable
    UserName = 'Table'
    Visible = False
    Left = 7
    Top = 75
  end
  object dsField: TDataSource
    DataSet = QryField
    Left = 42
    Top = 113
  end
  object plField: TppBDEPipeline
    DataSource = dsField
    UserName = 'Field'
    Visible = False
    Left = 7
    Top = 113
  end
  object RapDictionary: TppDataDictionary
    AllowManualJoins = True
    AutoJoin = True
    BuilderSettings.DatabaseName = 'DbRaportare'
    BuilderSettings.SessionType = 'ADOSession'
    FieldFieldNames.AutoSearch = 'autosearch'
    FieldFieldNames.DataType = 'datatype'
    FieldFieldNames.FieldName = 'field_name'
    FieldFieldNames.FieldAlias = 'field_alias'
    FieldFieldNames.Mandatory = 'mandatory'
    FieldFieldNames.Searchable = 'searchable'
    FieldFieldNames.Selectable = 'selectable'
    FieldFieldNames.Sortable = 'sortable'
    FieldFieldNames.TableName = 'table_name'
    FieldPipeline = plField
    JoinFieldNames.TableName1 = 'table_name1'
    JoinFieldNames.TableName2 = 'table_name2'
    JoinFieldNames.JoinType = 'join_type'
    JoinFieldNames.FieldNames1 = 'field_names1'
    JoinFieldNames.FieldNames2 = 'field_names2'
    JoinFieldNames.Operators = 'operators'
    JoinPipeline = plJoin
    TableFieldNames.TableName = 'table_name'
    TableFieldNames.TableAlias = 'table_alias'
    TablePipeline = plTable
    UserName = 'RapDictionary'
    ValidateFieldNames = False
    ValidateTableNames = False
    Left = 216
    Top = 38
  end
  object RapDesigner: TppDesigner
    AllowDataSettingsChange = True
    Caption = 'Modificare'
    DataSettings.DatabaseName = 'DBRaportare'
    DataSettings.SessionType = 'BDESession'
    DataSettings.AllowEditSQL = True
    DataSettings.DatabaseType = dtMSSQLServer
    DataSettings.DataDictionary = RapDictionary
    DataSettings.GuidCollationType = gcString
    DataSettings.IsCaseSensitive = True
    DataSettings.SQLType = sqSQL2
    DataSettings.UseDataDictionary = True
    Position = poScreenCenter
    Report = ppRaport
    IniStorageType = 'IniFile'
    IniStorageName = '.\RBuilder.ini'
    WindowHeight = 400
    WindowLeft = 100
    WindowTop = 50
    WindowWidth = 600
    OnClose = RapDesignerClose
    Left = 177
    Top = 38
  end
  object dsItem: TDataSource
    DataSet = QryItems
    Left = 42
    Top = 38
  end
  object plItem: TppBDEPipeline
    DataSource = dsItem
    UserName = 'plItem'
    Visible = False
    Left = 7
    Top = 38
  end
  object ppRaport: TppReport
    AutoStop = False
    NoDataBehaviors = [ndMessageOnPage, ndBlankReport]
    OnPrintingComplete = ppRaportPrintingComplete
    PassSetting = psTwoPass
    PrinterSetup.BinName = 'Default'
    PrinterSetup.DocumentName = 'Report'
    PrinterSetup.PaperName = 'Custom'
    PrinterSetup.PrinterName = 'Default'
    PrinterSetup.SaveDeviceSettings = False
    PrinterSetup.mmMarginBottom = 6350
    PrinterSetup.mmMarginLeft = 6350
    PrinterSetup.mmMarginRight = 6350
    PrinterSetup.mmMarginTop = 6350
    PrinterSetup.mmPaperHeight = 0
    PrinterSetup.mmPaperWidth = 0
    PrinterSetup.PaperSize = 256
    Template.DatabaseSettings.DataPipeline = plItem
    Template.DatabaseSettings.NameField = 'Name'
    Template.DatabaseSettings.TemplateField = 'Template'
    Template.SaveTo = stDatabase
    AfterOpenDataPipelines = ppRaportAfterOpenDataPipelines
    AllowPrintToArchive = True
    AllowPrintToFile = True
    ArchiveFileName = '($MyDocuments)\ReportArchive.raf'
    BeforeAutoSearchDialogCreate = ppRaportBeforeAutoSearchDialogCreate
    BeforeOpenDataPipelines = ppRaportBeforeOpenDataPipelines
    CachePages = True
    DeviceType = 'Screen'
    DefaultFileDeviceType = 'PDF'
    EmailSettings.ReportFormat = 'PDF'
    EmailSettings.Enabled = True
    LanguageID = 'Default'
    ModalPreview = False
    OnPreviewFormCreate = ppRaportPreviewFormCreate
    OpenFile = False
    OutlineSettings.CreateNode = True
    OutlineSettings.CreatePageNodes = True
    OutlineSettings.Enabled = False
    OutlineSettings.Visible = False
    ThumbnailSettings.Enabled = True
    ThumbnailSettings.Visible = True
    ThumbnailSettings.DeadSpace = 30
    PDFSettings.EmbedFontOptions = [efUseSubset]
    PDFSettings.EncryptSettings.AllowCopy = True
    PDFSettings.EncryptSettings.AllowInteract = True
    PDFSettings.EncryptSettings.AllowModify = True
    PDFSettings.EncryptSettings.AllowPrint = True
    PDFSettings.EncryptSettings.Enabled = False
    PDFSettings.EncryptSettings.KeyLength = kl40Bit
    PDFSettings.FontEncoding = feAnsi
    PDFSettings.ImageCompressionLevel = 25
    RTFSettings.DefaultFont.Charset = DEFAULT_CHARSET
    RTFSettings.DefaultFont.Color = clWindowText
    RTFSettings.DefaultFont.Height = -13
    RTFSettings.DefaultFont.Name = 'Arial'
    RTFSettings.DefaultFont.Style = []
    ShowAutoSearchDialog = True
    TextFileName = '($MyDocuments)\Report.pdf'
    TextSearchSettings.DefaultString = '<FindText>'
    TextSearchSettings.Enabled = True
    XLSSettings.AppName = 'ReportBuilder'
    XLSSettings.Author = 'ReportBuilder'
    XLSSettings.Subject = 'Report'
    XLSSettings.Title = 'Report'
    Left = 217
    Top = 2
    Version = '15.03'
    mmColumnWidth = 0
    object ppHeaderBand1: TppHeaderBand
      Background.Brush.Style = bsClear
      mmBottomOffset = 0
      mmHeight = 3500180
      mmPrintPosition = 0
    end
    object ppDetailBand1: TppDetailBand
      Background1.Brush.Style = bsClear
      Background2.Brush.Style = bsClear
      mmBottomOffset = 0
      mmHeight = 7630599
      mmPrintPosition = 0
    end
    object ppFooterBand1: TppFooterBand
      Background.Brush.Style = bsClear
      mmBottomOffset = 0
      mmHeight = 3500180
      mmPrintPosition = 0
    end
    object ppDesignLayers1: TppDesignLayers
      object ppDesignLayer1: TppDesignLayer
        UserName = 'Foreground'
        LayerType = ltBanded
        Index = 0
      end
    end
    object ppParameterList1: TppParameterList
    end
  end
  object dsFolder: TDataSource
    DataSet = QryFolder
    Left = 42
    Top = 2
  end
  object plFolder: TppBDEPipeline
    DataSource = dsFolder
    UserName = 'plFolder'
    Visible = False
    Left = 7
    Top = 2
  end
  object RapExplorer: TppReportExplorer
    Designer = RapDesigner
    FolderFieldNames.FolderId = 'folder_id'
    FolderFieldNames.Name = 'folder_name'
    FolderFieldNames.ParentId = 'parent_id'
    FolderPipeline = plFolder
    ItemFieldNames.Deleted = 'deleted'
    ItemFieldNames.FolderId = 'folder_id'
    ItemFieldNames.ItemId = 'item_id'
    ItemFieldNames.Modified = 'modified'
    ItemFieldNames.Name = 'item_name'
    ItemFieldNames.Size = 'item_size'
    ItemFieldNames.Template = 'template'
    ItemFieldNames.ItemType = 'item_type'
    ItemPipeline = plItem
    FormCaption = 'Explorer Rapoarte'
    FormPosition = poScreenCenter
    FormHeight = 400
    FormLeft = 100
    FormTop = 50
    FormWidth = 600
    FormState = wsMaximized
    OnClose = RapExplorerClose
    Left = 176
    Top = 2
  end
  object dsJoin: TDataSource
    DataSet = QryJoin
    Left = 42
    Top = 152
  end
  object plJoin: TppBDEPipeline
    DataSource = dsJoin
    UserName = 'plJoin'
    Visible = False
    Left = 7
    Top = 152
  end
  object qryItem: TADOQuery
    Tag = -1
    Connection = DbRaportare
    Parameters = <
      item
        Name = 'ITEM_ID'
        DataType = ftInteger
        Value = Null
      end>
    SQL.Strings = (
      'SET TEXTSIZE 524288000'
      
        'SELECT * FROM REPORTS_ITEM WHERE ITEM_ID=:ITEM_ID AND FOLDER_ID ' +
        '<>-2')
    Left = 176
    Top = 75
  end
  object QryFolder: TADOQuery
    Tag = 1
    Connection = DbRaportare
    Parameters = <>
    SQL.Strings = (
      'SELECT * FROM REPORTS_FOLDER ORDER BY'
      'FOLDER_ID')
    Left = 80
    Top = 2
  end
  object QryTable: TADOQuery
    Connection = DbRaportare
    Parameters = <>
    SQL.Strings = (
      'SELECT * FROM REPORTS_TABLE ORDER BY '
      'TABLE_NAME')
    Left = 80
    Top = 75
  end
  object QryField: TADOQuery
    Tag = 1
    Connection = DbRaportare
    Parameters = <>
    SQL.Strings = (
      'SELECT * FROM REPORTS_FIELD ORDER BY '
      'TABLE_NAME')
    Left = 80
    Top = 112
  end
  object QryJoin: TADOQuery
    Connection = DbRaportare
    Parameters = <>
    SQL.Strings = (
      'SELECT * FROM REPORTS_JOIN')
    Left = 80
    Top = 152
  end
  object QryItems: TADOQuery
    Connection = DbRaportare
    Parameters = <>
    SQL.Strings = (
      'SET TEXTSIZE 1'
      
        'SELECT ITEM_ID, FOLDER_ID, ITEM_NAME, ITEM_SIZE, ITEM_TYPE, MODI' +
        'FIED, DELETED, TEMPLATE'
      'FROM REPORTS_ITEM'
      'ORDER BY ITEM_ID')
    Left = 80
    Top = 38
    object QryItemsitem_id: TAutoIncField
      FieldName = 'item_id'
      Origin = 'rb_item.item_id'
    end
    object QryItemsfolder_id: TIntegerField
      FieldName = 'folder_id'
      Origin = 'rb_item.folder_id'
    end
    object QryItemsitem_name: TStringField
      FieldName = 'item_name'
      Origin = 'rb_item.item_name'
      Size = 60
    end
    object QryItemsitem_size: TIntegerField
      FieldName = 'item_size'
      Origin = 'rb_item.item_size'
    end
    object QryItemsitem_type: TIntegerField
      FieldName = 'item_type'
      Origin = 'rb_item.item_type'
    end
    object QryItemsmodified: TFloatField
      FieldName = 'modified'
      Origin = 'rb_item.modified'
    end
    object QryItemsdeleted: TFloatField
      FieldName = 'deleted'
      Origin = 'rb_item.deleted'
    end
    object QryItemstemplate: TBlobField
      FieldName = 'template'
      Origin = 'rb_item.template'
    end
  end
  object RapDevices: TExtraOptions
    About = 'TExtraDevices 3.00'
    HTML.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    HTML.BackLink = '&lt&lt'
    HTML.ForwardLink = '&gt&gt'
    HTML.ShowLinks = True
    HTML.UseTextFileName = False
    HTML.ZoomableImages = False
    HTML.Visible = True
    HTML.PixelFormat = pf8bit
    HTML.SingleFileOutput = False
    XHTML.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    XHTML.BackLink = '&lt&lt'
    XHTML.ForwardLink = '&gt&gt'
    XHTML.ShowLinks = True
    XHTML.UseTextFileName = False
    XHTML.ZoomableImages = False
    XHTML.Visible = True
    XHTML.PixelFormat = pf8bit
    XHTML.SingleFileOutput = False
    RTF.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    RTF.Visible = True
    RTF.RichTextAsImage = False
    RTF.UseTextBox = True
    RTF.PixelFormat = pf8bit
    RTF.PixelsPerInch = 96
    Lotus.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    Lotus.Visible = True
    Lotus.ColSpacing = 16934
    Quattro.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    Quattro.Visible = True
    Quattro.ColSpacing = 16934
    Excel.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    Excel.Visible = True
    Excel.ColSpacing = 16934
    Excel.RowSizing = False
    Excel.AutoConvertToNumber = True
    Graphic.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    Graphic.PixelFormat = pf8bit
    Graphic.UseTextFileName = False
    Graphic.Visible = True
    Graphic.PixelsPerInch = 96
    Graphic.GrayScale = False
    PDF.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    PDF.Creator = 'TExtraDevices'
    PDF.Author = 'ATLAS'
    PDF.FastCompression = False
    PDF.CompressImages = True
    PDF.ScaleImages = True
    PDF.Visible = True
    PDF.RichTextAsImage = False
    PDF.RichEditPixelFormat = pf1bit
    PDF.PixelFormat = pf24bit
    PDF.PixelsPerInch = 96
    PDF.Permissions = [ppPrint, ppModify, ppCopy, ppModifyAnnot]
    PDF.ViewerPreferences = []
    PDF.AutoEmbedFonts = True
    PDF.ImageFormat = riBitmap
    DotMatrix.ItemsToExport = [reText, reImage, reLine, reShape, reRTF, reBarCode, reCheckBox]
    DotMatrix.Visible = True
    DotMatrix.CharsPerInch = cs10CPI
    DotMatrix.LinesPerInch = ls6LPI
    DotMatrix.Port = 'LPT1'
    DotMatrix.ContinousPaper = False
    DotMatrix.PrinterType = ptEpson
    Left = 256
    Top = 74
  end
  object DbRaportare: TADOConnection
    CommandTimeout = 1800
    ConnectionString = 
      'Provider=SQLOLEDB.1;Password=QAZWSX;Persist Security Info=True;U' +
      'ser ID=SA;Initial Catalog=Conta_PITESTI3;Data Source=NTdx'
    ConnectionTimeout = 60
    IsolationLevel = ilReadUncommitted
    LoginPrompt = False
    Mode = cmRead
    Provider = 'SQLOLEDB.1'
    OnExecuteComplete = DbRaportareExecuteComplete
    Left = 216
    Top = 75
  end
  object ppAntet: TppBDEPipeline
    DataSource = frmData.DTAntetUnitate
    RefreshAfterPost = True
    UserName = 'Antet'
    Left = 8
    Top = 200
  end
end
