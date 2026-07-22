object frmAlegeUtilizator: TfrmAlegeUtilizator
  Left = 551
  Top = 290
  Caption = 'Lista Utilizatori Cunoscuti'
  ClientHeight = 359
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
    359)
  PixelsPerInch = 96
  TextHeight = 13
  object BtnCancel: TcxButton
    Left = 512
    Top = 287
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
    ModalResult = 2
    TabOrder = 0
  end
  object BtnOk: TcxButton
    Left = 431
    Top = 287
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    TabOrder = 1
    OnClick = BtnOkClick
  end
  object gridUtilizatori: TcxGrid
    Left = 0
    Top = 0
    Width = 591
    Height = 281
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 2
    LookAndFeel.Kind = lfUltraFlat
    object viewUtilizatori: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      FindPanel.DisplayMode = fpdmAlways
      ScrollbarAnnotations.CustomAnnotations = <>
      OnSelectionChanged = viewUtilizatoriSelectionChanged
      DataController.DataSource = DTUtilizatori
      DataController.KeyFieldNames = 'ID_UTILIZATORI'
      DataController.Options = [dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding, dcoMultiSelectionSyncGroupWithChildren]
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.IncSearch = True
      OptionsBehavior.FocusCellOnCycle = True
      OptionsData.CancelOnExit = False
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsSelection.MultiSelect = True
      OptionsSelection.CheckBoxVisibility = [cbvDataRow, cbvColumnHeader]
      OptionsSelection.MultiSelectMode = msmPersistent
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      OptionsView.GroupFooters = gfVisibleWhenExpanded
      OptionsView.Indicator = True
      Preview.AutoHeight = False
      Preview.MaxLineCount = 2
      object viewUtilizatoriNUME: TcxGridDBColumn
        Caption = 'Utilizator'
        DataBinding.FieldName = 'NUME'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Options.Filtering = False
        Width = 153
      end
      object viewUtilizatoriNUMEINTREG: TcxGridDBColumn
        Caption = 'Nume Intreg'
        DataBinding.FieldName = 'NUMEINTREG'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Options.Filtering = False
        Width = 326
      end
    end
    object nivelUtilizatori: TcxGridLevel
      GridView = viewUtilizatori
    end
  end
  object DTUtilizatori: TDataSource
    Left = 64
    Top = 88
  end
end
