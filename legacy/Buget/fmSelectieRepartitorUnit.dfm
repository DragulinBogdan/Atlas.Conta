object fmSelectieRepartitor: TfmSelectieRepartitor
  Left = 316
  Top = 235
  Caption = 'Selectie Clasificatie Economica'
  ClientHeight = 386
  ClientWidth = 640
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
  object cxTreeRepartitori: TcxDBTreeList
    Left = 0
    Top = 0
    Width = 640
    Height = 386
    Align = alClient
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = dtRepartitori
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_REPARTITORI'
    FindPanel.DisplayMode = fpdmAlways
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.AutoDragCopy = True
    OptionsBehavior.ConfirmDelete = False
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.DragExpand = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.CancelOnExit = False
    OptionsData.Editing = False
    OptionsData.Appending = True
    OptionsData.Deleting = False
    OptionsData.Inserting = True
    OptionsSelection.HideFocusRect = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = Null
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 0
    OnDblClick = cxTreeRepartitoriDblClick
    OnKeyUp = cxTreeRepartitoriKeyUp
    object cxTreeRepartitoriNUME: TcxDBTreeListColumn
      Caption.Text = 'Denumire'
      DataBinding.FieldName = 'NUME'
      Width = 298
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeRepartitoriADRESA: TcxDBTreeListColumn
      Caption.Text = 'Adresa'
      DataBinding.FieldName = 'ADRESA'
      Width = 157
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeRepartitoriCONT: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Cont'
      DataBinding.FieldName = 'CONT'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeRepartitoriCODFISC: TcxDBTreeListColumn
      Caption.Text = 'Cod Fiscal'
      DataBinding.FieldName = 'CODFISC'
      Width = 104
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeRepartitoriGESTINT: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Tip Gestiune'
      DataBinding.FieldName = 'GESTINT'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object dtRepartitori: TDataSource
    DataSet = qryRepartitori
    Left = 312
    Top = 200
  end
  object qryRepartitori: TZReadOnlyQuery
    SQL.Strings = (
      
        'exec spGetUserRepartitori :refUser, :tipRelatie, :tipuriRepartit' +
        'ori')
    Params = <
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'tipRelatie'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'tipuriRepartitori'
        ParamType = ptUnknown
      end>
    Left = 376
    Top = 200
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'tipRelatie'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'tipuriRepartitori'
        ParamType = ptUnknown
      end>
  end
end
