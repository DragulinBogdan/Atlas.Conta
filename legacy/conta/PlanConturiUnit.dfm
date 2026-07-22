object FrmPlanConturi: TFrmPlanConturi
  Left = 523
  Top = 197
  Caption = 'Intretinere Plan De Conturi'
  ClientHeight = 431
  ClientWidth = 666
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  DesignSize = (
    666
    431)
  PixelsPerInch = 96
  TextHeight = 13
  object GrPlanConturi: TGroupBox
    Left = 0
    Top = 0
    Width = 666
    Height = 361
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    Caption = 'Planul de Conturi '
    Ctl3D = True
    ParentCtl3D = False
    TabOrder = 0
    object Splitter1: TSplitter
      Left = 437
      Top = 15
      Height = 344
      Align = alRight
      ExplicitLeft = 476
      ExplicitHeight = 376
    end
    object pnContInfo: TPanel
      Left = 440
      Top = 15
      Width = 224
      Height = 344
      Align = alRight
      BevelOuter = bvNone
      TabOrder = 0
      ExplicitHeight = 376
      object vContInfo: TcxDBVerticalGrid
        Left = 0
        Top = 0
        Width = 224
        Height = 344
        Align = alClient
        LookAndFeel.Kind = lfFlat
        OptionsView.CellTextMaxLineCount = 3
        OptionsView.AutoScaleBands = False
        OptionsView.GridLineColor = clBtnShadow
        OptionsView.RowHeaderMinWidth = 30
        OptionsView.RowHeaderWidth = 113
        OptionsView.ValueWidth = 82
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        DataController.DataSource = frmData.DTPlanCont
        ExplicitHeight = 376
        Version = 1
        object vContInfoROMANA: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Captura'
          Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.MaxLength = 0
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'ROMANA'
          ID = 0
          ParentID = -1
          Index = 0
          Version = 1
        end
        object vContInfoSUMATOR: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Sumator'
          Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
          Properties.EditProperties.Alignment = taLeftJustify
          Properties.EditProperties.NullStyle = nssUnchecked
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SUMATOR'
          ID = 1
          ParentID = -1
          Index = 1
          Version = 1
        end
        object vContInfoDBMultiEditorRow1: TcxDBMultiEditorRow
          Expanded = False
          Properties.Editors = <
            item
              Caption = 'Nr'
              EditPropertiesClassName = 'TcxTextEditProperties'
              EditProperties.Alignment.Horz = taLeftJustify
              EditProperties.CharCase = ecUpperCase
              EditProperties.MaxLength = 0
              EditProperties.ReadOnly = False
              Width = 20
              DataBinding.FieldName = 'FCTCONT'
            end
            item
              Caption = 'Descri.'
              EditPropertiesClassName = 'TcxImageComboBoxProperties'
              EditProperties.Alignment.Horz = taLeftJustify
              EditProperties.DropDownRows = 7
              EditProperties.Items = <
                item
                  Description = 'Cont Creditor'
                  ImageIndex = 0
                  Value = 'C'
                end
                item
                  Description = 'Cont Debitor'
                  ImageIndex = 1
                  Value = 'D'
                end
                item
                  Description = 'Cont BiFunctional'
                  ImageIndex = 2
                  Value = 'B'
                end
                item
                  Description = 'Cont Ignorat'
                  ImageIndex = 3
                  Value = 'N'
                end>
              EditProperties.ReadOnly = False
              DataBinding.FieldName = 'FCTCONT'
            end>
          ID = 2
          ParentID = -1
          Index = 2
          Version = 1
        end
        object vContInfoBALANTA: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Defalcare'
          Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.DropDownRows = 7
          Properties.EditProperties.Items = <
            item
              Description = 'Defalcat pe Repartitori'
              ImageIndex = 0
              Value = 'R'
            end
            item
              Description = 'Defalcare Standard'
              ImageIndex = 1
              Value = 'S'
            end
            item
              Description = 'Defalcat pe Articole'
              ImageIndex = 2
              Value = 'T'
            end>
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'BALANTA'
          ID = 3
          ParentID = -1
          Index = 3
          Version = 1
        end
        object vContInfoDBEditorRow: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Clasa Economica'
          Properties.EditPropertiesClassName = 'TcxPopupEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.MaxLength = 0
          Properties.EditProperties.ReadOnly = False
          ID = 4
          ParentID = -1
          Index = 4
          Version = 1
        end
        object vContInfoIS_SINTETIC: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Este Sintetic(Balanta Sintetica)'
          Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
          Properties.EditProperties.Alignment = taLeftJustify
          Properties.EditProperties.NullStyle = nssUnchecked
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'IS_SINTETIC'
          ID = 5
          ParentID = -1
          Index = 5
          Version = 1
        end
        object vContInfoSID: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Sold Initial Debitor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SID'
          ID = 6
          ParentID = -1
          Index = 6
          Version = 1
        end
        object vContInfoSIC: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Sold Initial Creditor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SIC'
          ID = 7
          ParentID = -1
          Index = 7
          Version = 1
        end
        object vContInfoSPD: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Rulaj Precedent Debitor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SPD'
          ID = 8
          ParentID = -1
          Index = 8
          Version = 1
        end
        object vContInfoSPC: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Rulaj Precedent Creditor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SPC'
          ID = 9
          ParentID = -1
          Index = 9
          Version = 1
        end
        object vContInfoRD: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Rulaj Debitor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'RD'
          ID = 10
          ParentID = -1
          Index = 10
          Version = 1
        end
        object vContInfoRC: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Rulaj Creditor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'RC'
          ID = 11
          ParentID = -1
          Index = 11
          Version = 1
        end
        object vContInfoUnitate: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Unitate/Proiect'
          Properties.EditPropertiesClassName = 'TcxPopupEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.MaxLength = 0
          Properties.EditProperties.PopupControl = TreeUnitate
          Properties.EditProperties.PopupSysPanelStyle = True
          Properties.EditProperties.ReadOnly = True
          ID = 12
          ParentID = -1
          Index = 12
          Version = 1
        end
        object vContInfoSC: TcxDBEditorRow
          Properties.Caption = 'Sold Creditor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SC'
          Visible = False
          ID = 13
          ParentID = -1
          Index = 13
          Version = 1
        end
        object vContInfoSD: TcxDBEditorRow
          Properties.Caption = 'Sold Debitor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'SD'
          Visible = False
          ID = 14
          ParentID = -1
          Index = 14
          Version = 1
        end
        object vContInfoTIP: TcxDBEditorRow
          Properties.Caption = 'Tip'
          Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.DropDownRows = 7
          Properties.EditProperties.Items = <
            item
              Description = 'Sintetic'
              ImageIndex = 0
              Value = 'S'
            end
            item
              Description = 'Analitic'
              ImageIndex = 1
              Value = 'A'
            end>
          Properties.EditProperties.ReadOnly = False
          Properties.EditProperties.ShowDescriptions = False
          Properties.DataBinding.FieldName = 'TIP'
          Visible = False
          ID = 15
          ParentID = -1
          Index = 15
          Version = 1
        end
        object vContInfoTSC: TcxDBEditorRow
          Properties.Caption = 'Total Sold Creditor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'TSC'
          Visible = False
          ID = 16
          ParentID = -1
          Index = 16
          Version = 1
        end
        object vContInfoTSD: TcxDBEditorRow
          Properties.Caption = 'Total Sold Debitor'
          Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.AssignedValues.MaxValue = True
          Properties.EditProperties.AssignedValues.MinValue = True
          Properties.EditProperties.DecimalPlaces = 2
          Properties.EditProperties.DisplayFormat = ',0.00;-,0.00'
          Properties.EditProperties.Nullable = False
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'TSD'
          Visible = False
          ID = 17
          ParentID = -1
          Index = 17
          Version = 1
        end
      end
    end
    object TreePlan: TcxDBTreeList
      Left = 2
      Top = 15
      Width = 435
      Height = 344
      Align = alClient
      Bands = <
        item
          Caption.AlignHorz = taCenter
        end>
      DataController.DataSource = frmData.DTPlanCont
      DataController.ParentField = 'PARINTE'
      DataController.KeyField = 'CONT'
      DefaultRowHeight = 18
      DragMode = dmAutomatic
      LookAndFeel.Kind = lfFlat
      Navigator.Buttons.CustomButtons = <>
      OptionsBehavior.ImmediateEditor = False
      OptionsBehavior.AutoDragCopy = True
      OptionsBehavior.DragCollapse = False
      OptionsBehavior.DragFocusing = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.ShowHourGlass = False
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsSelection.HideFocusRect = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GridLineColor = clNone
      PopupMenu = ppPlanConturi
      Preview.AutoHeight = False
      RootValue = -1
      ScrollbarAnnotations.CustomAnnotations = <>
      Styles.OnGetContentStyle = TreePlanStylesGetContentStyle
      TabOrder = 1
      OnFocusedNodeChanged = TreePlanFocusedNodeChanged
      OnGetNodeImageIndex = TreePlanGetNodeImageIndex
      OnNodeChanged = TreePlanNodeChanged
      ExplicitHeight = 376
      object TreePlanCONT: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaTop
        Caption.Text = 'Cont'
        DataBinding.FieldName = 'CONT'
        Options.Editing = False
        Width = 110
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        SortOrder = soAscending
        SortIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanROMANA: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaTop
        Caption.Text = 'Plan Cont'
        DataBinding.FieldName = 'ROMANA'
        Options.Editing = False
        Width = 239
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanSID: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignVert = vaTop
        DataBinding.FieldName = 'SID'
        Options.Editing = False
        Width = 41
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanSIC: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignVert = vaTop
        DataBinding.FieldName = 'SIC'
        Options.Editing = False
        Width = 43
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanFCTCONT: TcxDBTreeListColumn
        PropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.DropDownRows = 7
        Properties.Items = <
          item
            Description = 'Cont de debit'
            ImageIndex = 0
            Value = 'D'
          end
          item
            Description = 'Cont de credit'
            ImageIndex = 1
            Value = 'C'
          end
          item
            Description = 'Cont bifunctional'
            ImageIndex = 2
            Value = 'B'
          end>
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaTop
        Caption.Text = 'Tip Cont'
        DataBinding.FieldName = 'FCTCONT'
        MinWidth = 16
        Options.Editing = False
        Width = 100
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanBALANTA: TcxDBTreeListColumn
        PropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.DropDownRows = 7
        Properties.Items = <
          item
            Description = 'Defalcat pe Repartitori'
            ImageIndex = 0
            Value = 'R'
          end
          item
            Description = 'Defalcat pe Articole'
            ImageIndex = 1
            Value = 'T'
          end>
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaTop
        Caption.Text = 'Defalcat'
        DataBinding.FieldName = 'BALANTA'
        MinWidth = 16
        Options.Editing = False
        Width = 100
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object TreePlanPARINTE: TcxDBTreeListColumn
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignVert = vaTop
        Caption.Text = 'Parinte'
        DataBinding.FieldName = 'PARINTE'
        Options.Customizing = False
        Options.Editing = False
        Width = 39
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
  end
  object ExpandLevels: TToolBar
    Left = 6
    Top = 398
    Width = 277
    Height = 21
    Align = alNone
    Anchors = [akLeft, akBottom]
    ButtonHeight = 21
    ButtonWidth = 65
    Caption = 'Nivele de sinteza'
    EdgeInner = esNone
    EdgeOuter = esNone
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    ParentFont = False
    ShowCaptions = True
    TabOrder = 2
  end
  object TreeUnitate: TdxDBTreeList
    Left = 64
    Top = 168
    Width = 305
    Height = 193
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'id'
    ParentField = 'id_parinte'
    TabOrder = 3
    Visible = False
    OnDblClick = TreeUnitateDblClick
    OnKeyDown = TreeUnitateKeyDown
    DataSource = DTUnitate
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowHeader = False
    TreeLineColor = clGrayText
    object TreeUnitateid: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'id'
    end
    object TreeUnitateDenumire: TdxDBTreeListColumn
      Sorted = csUp
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Denumire'
    end
    object TreeUnitateid_parinte: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'id_parinte'
    end
  end
  object BtnOk: TcxButton
    Left = 570
    Top = 367
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    TabOrder = 1
    OnClick = BtnOkClick
  end
  object ImaginiConturi: TImageList
    Left = 376
    Top = 168
    Bitmap = {
      494C010103000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400000000000000000000000000000000000000
      00000000000000000000000000000000000084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C6000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF00008484000000000000000000000000000000
      0000000000000000000000000000000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FF000000FF000000FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF00008484000084840000000000C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00FF000000C6C6C600FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000FFFF00FFFFFF0000FF
      FF0000FFFF00008484000000000000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000000000000000000000848400FFFFFF0000FF
      FF000084840000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000C6C6C600C6C6C600FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF000000000000FF000000FF000000FF00000000000000FFFF00FFFFFF000000
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00C6C6C600FF000000FF000000FF000000FF00
      0000FF000000FF000000FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000FF000000FF000000FF000000000000FFFFFF000000
      00000000000000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00C6C6C600FF000000FF000000FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF0000848400FFFFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF000000000000FF000000FF000000FF00000000000000FF
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF00FFFFFF0000FFFF000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      000000000000FFFFFF00FFFFFF000000000000FF000000FF000000FF000000FF
      00000000000000FFFF0000848400000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF0000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF0084840000848400000000000000FF000000FF000000FF
      00000000000000848400008484000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF008484840084848400848484008484
      840084848400FFFFFF00C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C6000000000084848400FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00848400000000000000FF000000FF000000FF000000FF
      000000000000FFFFFF00008484000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000008484840084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      000000000000FFFFFF0000FFFF0000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      000000000000000000000000000084848400FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0084848400000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00C1FF007FC107000080FF007F80010000
      0000007F00000000000000030000000000000001000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000080000000800000008000000080000000C001FE00C0010000
      E003FF01E0030000F07FFF83F07F000000000000000000000000000000000000
      000000000000}
  end
  object ppPlanConturi: TPopupMenu
    Left = 280
    Top = 56
    object AdaugaNouAnalitic1: TMenuItem
      Action = Cmd_AdaugaContAnalitic
    end
    object AdaugaContPeNivelulCurent1: TMenuItem
      Action = Cmd_AdaugaContPeAcelasiNeivle
    end
    object ppMutaMaiSus: TMenuItem
      Caption = 'Muta pe nivel superior'
      OnClick = ppMutaMaiSusClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object ModificareContCurent1: TMenuItem
      Action = Cmd_ModificareContCurent
    end
    object StergeContCurent1: TMenuItem
      Action = Cmd_DeleteContCurent
    end
    object N2: TMenuItem
      Caption = '-'
    end
  end
  object Actiuni: TActionList
    Left = 216
    Top = 56
    object Cmd_AdaugaContAnalitic: TAction
      Caption = 'Adauga Nou Analitic'
      ShortCut = 16429
      OnExecute = Cmd_AdaugaContAnaliticExecute
    end
    object Cmd_AdaugaContPeAcelasiNeivle: TAction
      Caption = 'Adauga Cont Pe Nivelul Curent'
      ShortCut = 45
      OnExecute = Cmd_AdaugaContPeAcelasiNeivleExecute
    end
    object Cmd_FisaContului: TAction
      Caption = 'Fisa Contului'
    end
    object Cmd_ModificareContCurent: TAction
      Caption = 'Modificare Cont Curent'
      OnExecute = Cmd_ModificareContCurentExecute
    end
    object Cmd_DeleteContCurent: TAction
      Caption = 'Sterge Cont Curent'
      ShortCut = 16430
      OnExecute = Cmd_DeleteContCurentExecute
    end
  end
  object DTUnitate: TDataSource
    DataSet = qryUnitate
    Left = 320
    Top = 216
  end
  object qryUnitate: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spContaGetListUnitati')
    Params = <>
    Left = 376
    Top = 216
  end
  object stiluri: TcxStyleRepository
    Left = 72
    Top = 112
    PixelsPerInch = 96
    object stilBifunctionalFunza: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clNavy
    end
    object stilCreditFrunza: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clGreen
    end
    object stilDebitFrunza: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clRed
    end
    object stilBifunctional: TcxStyle
      AssignedValues = [svTextColor]
      TextColor = clNavy
    end
    object stilDebit: TcxStyle
      AssignedValues = [svTextColor]
      TextColor = clRed
    end
    object stilCredit: TcxStyle
      AssignedValues = [svTextColor]
      TextColor = clGreen
    end
    object stilNormal: TcxStyle
    end
  end
end
