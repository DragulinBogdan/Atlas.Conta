object frmGEST_ModifyDocum: TfrmGEST_ModifyDocum
  Left = 271
  Top = 137
  Caption = 'Modificare Document'
  ClientHeight = 632
  ClientWidth = 1235
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  KeyPreview = True
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnKeyDown = FormKeyDown
  OnResize = FormResize
  DesignSize = (
    1235
    632)
  PixelsPerInch = 96
  TextHeight = 13
  object pnClient: TPanel
    Left = 0
    Top = 27
    Width = 1235
    Height = 494
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 3
    object GrDocum: TGroupBox
      Left = 1
      Top = 21
      Width = 1233
      Height = 213
      Align = alTop
      Caption = 'Lista documente '
      TabOrder = 0
      object treeDocument: TcxDBTreeList
        Left = 2
        Top = 15
        Width = 1229
        Height = 196
        Align = alClient
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = DTDocum
        DataController.ParentField = 'ID_DOCUMENT_CONEX'
        DataController.KeyField = 'ID_GEST_DOCUM'
        DefaultRowHeight = 19
        DragMode = dmAutomatic
        Images = ImgList
        LookAndFeel.Kind = lfFlat
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragFocusing = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ShowHourGlass = False
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsSelection.HideFocusRect = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GridLineColor = clSilver
        OptionsView.GridLines = tlglBoth
        OptionsView.GroupFooters = tlgfVisibleWhenExpanded
        OptionsView.Indicator = True
        OptionsView.ShowRoot = False
        PopupMenu = pnDocument
        Preview.AutoHeight = False
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        Styles.OnGetContentStyle = treeDocumentStylesGetContentStyle
        TabOrder = 0
        OnFocusedNodeChanged = treeDocumentFocusedNodeChanged
        OnGetNodeImageIndex = treeDocumentGetNodeImageIndex
        OnKeyDown = FormKeyDown
        object TreeDocumentID_GEST_DOCUM: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          DataBinding.FieldName = 'ID_GEST_DOCUM'
          Options.Editing = False
          Width = 114
          Position.ColIndex = 13
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentID_INITIAL: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          DataBinding.FieldName = 'ID_INITIAL'
          Options.Editing = False
          Width = 74
          Position.ColIndex = 14
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentCOD_DOCUM: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Tip Docum'
          DataBinding.FieldName = 'COD_DOCUM'
          Options.Editing = False
          Width = 86
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentPREDATOR: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Predator'
          DataBinding.FieldName = 'PREDATOR'
          Options.Editing = False
          Width = 85
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentPRIMITOR: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Primitor'
          DataBinding.FieldName = 'PRIMITOR'
          Options.Editing = False
          Width = 75
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentNR_DOCUM: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Nr. Doc.'
          DataBinding.FieldName = 'NR_DOCUM'
          Options.Editing = False
          Width = 64
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentDATA_DOCUM: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Caption.AlignVert = vaTop
          Caption.Text = 'Data Doc.'
          DataBinding.FieldName = 'DATA_DOCUM'
          Options.Editing = False
          Width = 56
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentTOTAL_DOCUMENT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Total Doc'
          DataBinding.FieldName = 'TOTAL_DOCUMENT'
          Options.Editing = False
          Width = 83
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentTOTAL_TVA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Total Tva'
          DataBinding.FieldName = 'TOTAL_TVA'
          Options.Editing = False
          Width = 69
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentNUMEINTREG: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Operator'
          DataBinding.FieldName = 'NUMEINTREG'
          Options.Editing = False
          Width = 61
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentDATA_OPERARE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Caption.AlignVert = vaTop
          Caption.Text = 'Operare'
          DataBinding.FieldName = 'DATA_OPERARE'
          Options.Editing = False
          Width = 84
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          SortIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentID_DOCUMENT_CONEX: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Id Parinte'
          DataBinding.FieldName = 'ID_DOCUMENT_CONEX'
          Options.Editing = False
          Width = 149
          Position.ColIndex = 12
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentID_TRANZACTIE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Id'
          DataBinding.FieldName = 'ID_TRANZACTIE'
          Options.Editing = False
          Width = 108
          Position.ColIndex = 11
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentAUTOGENERAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueChecked = 'True'
          Properties.ValueGrayed = ''
          Properties.ValueUnchecked = 'False'
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Autogenerat'
          DataBinding.FieldName = 'AUTOGENERAT'
          MinWidth = 16
          Options.Editing = False
          Width = 100
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDocumentSTARE_DOCUMENT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Stare Document'
          DataBinding.FieldName = 'STARE'
          Options.Editing = False
          Width = 108
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentcxDBTreeListIdSolicitare: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id Solicitare'
          DataBinding.FieldName = 'ID_solicitare'
          Width = 72
          Position.ColIndex = 15
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentcxDBTreeListIDIncarcare: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id Incarcare'
          DataBinding.FieldName = 'ID_incarcare'
          Width = 72
          Position.ColIndex = 16
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_GEST_DOCUM1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_GEST_DOCUM'
          Width = 20
          Position.ColIndex = 17
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_INITIAL1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_INITIAL'
          Width = 20
          Position.ColIndex = 18
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentCOD_DOCUM1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'COD_DOCUM'
          Width = 20
          Position.ColIndex = 19
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentPREDATOR1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'PREDATOR'
          Width = 20
          Position.ColIndex = 20
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentPRIMITOR1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'PRIMITOR'
          Width = 20
          Position.ColIndex = 21
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentNR_DOCUM1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'NR_DOCUM'
          Width = 20
          Position.ColIndex = 22
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentDATA_DOCUM1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'DATA_DOCUM'
          Width = 20
          Position.ColIndex = 23
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentTOTAL_DOCUMENT1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'TOTAL_DOCUMENT'
          Width = 20
          Position.ColIndex = 24
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentTOTAL_TVA1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'TOTAL_TVA'
          Width = 20
          Position.ColIndex = 25
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentNUMEINTREG1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'NUMEINTREG'
          Width = 20
          Position.ColIndex = 26
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentDATA_OPERARE1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'DATA_OPERARE'
          Width = 20
          Position.ColIndex = 27
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_DOCUMENT_CONEX1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_DOCUMENT_CONEX'
          Width = 20
          Position.ColIndex = 28
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_TRANZACTIE1: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_TRANZACTIE'
          Width = 20
          Position.ColIndex = 29
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_solicitare: TcxDBTreeListColumn
          Caption.Text = 'Id Descarcare'
          DataBinding.FieldName = 'ID_Xml'
          Width = 42
          Position.ColIndex = 30
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentID_incarcare: TcxDBTreeListColumn
          DataBinding.FieldName = 'ID_incarcare'
          Width = 42
          Position.ColIndex = 31
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentcxDBTreeList_CifEmitent: TcxDBTreeListColumn
          Caption.Text = 'Cif Emitent'
          DataBinding.FieldName = 'cif_emitent'
          Width = 130
          Position.ColIndex = 32
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object treeDocumentcxDBTreeList_CifBeneficiar: TcxDBTreeListColumn
          Caption.Text = 'Cif Beneficiar'
          DataBinding.FieldName = 'cif_beneficiar'
          Width = 291
          Position.ColIndex = 33
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object GrDetalii: TGroupBox
      Left = 1
      Top = 242
      Width = 1233
      Height = 251
      Align = alClient
      Caption = 'Pozitii document'
      TabOrder = 1
      object gridDetaliiDocument: TcxGrid
        Left = 2
        Top = 15
        Width = 1229
        Height = 234
        Align = alClient
        PopupMenu = SelectMenu
        TabOrder = 0
        LookAndFeel.Kind = lfFlat
        object viewDetaliiDocument: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = DTItemsi
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'ID_GEST_ITEMSI'
          DataController.Options = [dcoAnsiSort, dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding]
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <
            item
              Format = ',0.0000;-,0.0000'
              Kind = skSum
              FieldName = 'PRET_UNITAR'
              Column = viewDetaliiDocumentPRET_UNITAR
            end
            item
              Kind = skSum
              FieldName = 'PRET_UNITAR_VALUTA'
              Column = viewDetaliiDocumentPRET_UNITAR_VALUTA
            end
            item
              Format = ',0.0000;-,0.0000'
              Kind = skSum
              FieldName = 'PRET_TVA'
              Column = viewDetaliiDocumentPRET_TVA
            end
            item
              Format = ',0.0000;-,0.0000'
              Kind = skSum
              FieldName = 'PRET_TOTAL'
              Column = viewDetaliiDocumentPRET_TOTAL
            end
            item
              Format = ',0.0000;-,0.0000'
              Kind = skSum
              FieldName = 'TVA'
              Column = viewDetaliiDocumentTVA
            end
            item
              Format = ',0.0000;-,0.0000'
              Kind = skSum
              FieldName = 'PRET_TOTAL_TVA'
              Column = viewDetaliiDocumentPRET_TOTAL_TVA
            end>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsBehavior.ImmediateEditor = False
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewDetaliiDocumentTIPMAT: TcxGridDBColumn
            Caption = 'Clasa mat.'
            DataBinding.FieldName = 'TIPMAT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 84
          end
          object viewDetaliiDocumentDESCRIERE: TcxGridDBColumn
            Caption = 'Angajament'
            DataBinding.FieldName = 'DESCRIERE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewDetaliiDocumentDENMAT: TcxGridDBColumn
            Caption = 'Den Mat.'
            DataBinding.FieldName = 'DENMAT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 101
          end
          object viewDetaliiDocumentUM: TcxGridDBColumn
            DataBinding.FieldName = 'UM'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 50
          end
          object viewDetaliiDocumentDATA_COD: TcxGridDBColumn
            Caption = 'Data Cod'
            DataBinding.FieldName = 'DATA_COD'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 50
          end
          object viewDetaliiDocumentDATA_EXPIRARE: TcxGridDBColumn
            Caption = 'Data Expir.'
            DataBinding.FieldName = 'DATA_EXPIRARE'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 57
          end
          object viewDetaliiDocumentTIP_MATERIAL: TcxGridDBColumn
            Caption = 'Grup Mat'
            DataBinding.FieldName = 'TIP_MATERIAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 53
          end
          object viewDetaliiDocumentCANTITATE: TcxGridDBColumn
            Caption = 'Cant.'
            DataBinding.FieldName = 'CANTITATE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentPRET_UNITAR: TcxGridDBColumn
            Caption = 'Pret. Unitar'
            DataBinding.FieldName = 'PRET_UNITAR'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentPRET_UNITAR_VALUTA: TcxGridDBColumn
            Caption = 'Pret. Unit. Val'
            DataBinding.FieldName = 'PRET_UNITAR_VALUTA'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 69
          end
          object viewDetaliiDocumentCOTA_TVA: TcxGridDBColumn
            Caption = '% TVA'
            DataBinding.FieldName = 'COTA_TVA'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentPRET_TVA: TcxGridDBColumn
            Caption = 'Pret. TVA'
            DataBinding.FieldName = 'PRET_TVA'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentPRET_TOTAL: TcxGridDBColumn
            Caption = 'Total'
            DataBinding.FieldName = 'PRET_TOTAL'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentTVA: TcxGridDBColumn
            DataBinding.FieldName = 'TVA'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 65
          end
          object viewDetaliiDocumentPRET_TOTAL_TVA: TcxGridDBColumn
            Caption = 'Total TVA'
            DataBinding.FieldName = 'PRET_TOTAL_TVA'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.0000;-,0.0000'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 70
          end
          object viewDetaliiDocumentCODMAT: TcxGridDBColumn
            Caption = 'CodMat'
            DataBinding.FieldName = 'CODMAT'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 43
          end
        end
        object nivelDetaliiDocument: TcxGridLevel
          GridView = viewDetaliiDocument
        end
      end
    end
    object Splitter: TcxSplitter
      Left = 1
      Top = 234
      Width = 1233
      Height = 8
      Cursor = crVSplit
      HotZoneClassName = 'TcxMediaPlayer9Style'
      AlignSplitter = salTop
      AutoSnap = True
      InvertDirection = True
      Control = GrDocum
    end
    object cxTabControl: TcxTabControl
      Left = 1
      Top = 1
      Width = 1233
      Height = 20
      Align = alTop
      TabOrder = 3
      Visible = False
      Properties.CustomButtons.Buttons = <>
      Properties.Style = 9
      Properties.TabIndex = 1
      Properties.Tabs.Strings = (
        'Documente Validate'
        'Documente de revalidat')
      Properties.TabSlants.Kind = skCutCorner
      Properties.TabSlants.Positions = [spLeft, spRight]
      TabSlants.Kind = skCutCorner
      TabSlants.Positions = [spLeft, spRight]
      OnChange = cxTabControlChange
      OnDrawTabEx = cxTabControlDrawTabEx
      ClientRectRight = 0
      ClientRectTop = 0
    end
  end
  object BtnOk: TcxButton
    Left = 1064
    Top = 545
    Width = 65
    Height = 27
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    ModalResult = 1
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      000000000000000000000000000000000000000000010000000E000000270001
      004400010053000100550000004B000000300000001600000005000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000001000000120105015806300BB70D6015E80F77
      19FA107C1CFD107C1CFD0F7119F70B5012E1052208B7000200740000002B0000
      0004000000000000000000000000000000000000000000000000000000000000
      000000000000000000030104014009420FC3107D1BFD21932CFF25A732FF28B3
      35FF23B630FF26B634FF2CB038FF29A235FF1C8A27FF0F7719FA07320BC90000
      0064000000110000000000000000000000000000000000000000000000000000
      000000000004010B025B0D6A17EC269331FF3DB949FF36C344FF2AC038FF20BF
      2FFF1DBE2DFF22BF32FF2BC03AFF38C245FF46C552FF42B14DFF1D8A28FF0C5B
      15E90005017F0000001600000000000000000000000000000000000000000000
      0002020C0358117C1CFB42AB4CFF51C65CFF42C34EFF34C242FF24A62FFF418B
      47FF1DBF2DFF20BF30FF2AC039FF36C244FF43C450FF52C55DFF60C66AFF3298
      3CFF0F7219F60005017E0000000F000000000000000000000000000000000105
      01360E7319F24DAE57FF5FC76AFF51C55DFF43C44FFF2FA73AFF7C927EFFB9B9
      B8FF2FA539FF26BF35FF2DC03CFF38C246FF44C450FF51C65CFF5FC76AFF6ECA
      77FF37993FFF0D5C14E900010060000000030000000000000000000000030A47
      10BD389A41FF6FCA79FF61C76BFF54C65FFF3DA947FF7C927EFFD7D6D6FFF4F5
      F5FF95AC97FF2AB237FF34C242FF3DC34AFF47C453FF53C65EFF5FC769FF6BC8
      74FF77C97FFF238B2DFF07330CC60000002600000000000000000108023F1985
      24FF7CCA83FF71C97AFF64C86EFF46984DFF8D978EFFDADADAFFFFFFFFFFFFFF
      FFFFE8E8E9FF799C7BFF3CC049FF44C450FF4CC558FF57C661FF61C76BFF6BC8
      74FF76CA7EFF62B76AFF0F781AFB0003016D000000030000000109420FB149A3
      52FF7DCC86FF6BBE75FF558258FFA5A8A5FFE2E2E2FFFFFFFFFFE8ECE9FFDCE9
      DDFFFFFFFFFFDCDCDCFF5F9564FF4CC458FF52C55DFF59C664FF62C76CFF6BC8
      74FF74CA7DFF7FCC86FF288E32FF062709B7000000130001000B0E7419F16EBB
      75FF71B277FF7F8C7FFFCDCECEFFEEEEEEFFFFFFFFFFFDFDFEFF96C99AFF78C4
      7FFFFEFDFEFFFDFDFDFFCCCDCCFF5B9C61FF59C764FF5DC668FF63C76DFF6BC8
      74FF73CA7BFF7ACA82FF4FAA59FF0C5814E40000002C020F033D10801CFF81CB
      89FF79CB81FF7DBF84FFD5DBD6FFFDFDFDFFFFFFFFFFDBE1DBFF57C461FF55C5
      61FFD1DFD2FFFFFFFFFFFDFDFDFFC4C5C4FF5C9560FF62C86EFF66C870FF6AC8
      74FF71C97AFF76CA7FFF65BB6DFF0F781AFB0001004803170557148220FF7CCC
      85FF73CA7BFF6AC873FF65C66EFF90C696FFE8E7E8FF8FC795FF61C76BFF64C7
      6EFF81C488FFFCFAFCFFFFFFFFFFF9F9F9FFC3C4C3FF619165FF67C671FF6AC8
      74FF6EC977FF71CA7AFF6EC577FF0F7F1AFF0104015803170559158220FF75CB
      7EFF6CC876FF68C871FF64C76DFF62C76CFF6EC476FF6AC773FF6CC875FF71C9
      79FF74C97CFFB9D2BBFFFFFFFFFFFFFFFFFFFAFAFAFFCACACAFF729374FF5AAB
      63FF6AC874FF6CC876FF6CC575FF0F7F1AFF010401580313044C13811EFF6FCA
      78FF68C871FF65C76EFF63C76EFF67C870FF6CC975FF74CA7CFF7CCA83FF82CB
      88FF86CC8CFF87C88DFFE6E7E5FFFFFFFFFFFFFFFFFFFEFEFEFFDADBDAFFA8AC
      A8FF65B86DFF6AC874FF66C26FFF0E7E1AFE000301450108021B0F7C1BFB62C0
      6BFF64C76EFF62C76CFF65C76FFF6CC975FF76CA7EFF82CB88FF8ACC91FF92CE
      98FF96CE9CFF97CE9DFF9DC8A1FFF4F3F4FFFFFFFFFFFFFFFFFFFFFFFFFFE0E4
      E0FF68C771FF68C871FF56B460FF0E6D18F000000020000000010D6115D049AB
      52FF61C76BFF62C76CFF69C872FF73CA7CFF81CB87FF8DCD94FF99CE9FFFA1CF
      A6FFA6D0AAFFA6D0ABFFA3D0A8FFB4CCB7FFFAF9F9FFFFFFFFFFFFFFFFFFCDDD
      CFFF6BC974FF6AC873FF3FA149FF083E0EC00000000B0000000005260867258D
      30FF68CA72FF63C76DFF6DC876FF7BCA82FF8BCD91FF9BCEA0FFA8D0ACFFB2D2
      B5FFB6D2B8FFB5D2B8FFB1D2B4FFA8D1ACFFB3C9B6FFFBFAFBFFFFFFFFFFCCDD
      CDFF6DC976FF6BC675FF14811EFF020C03610000000100000000000301090E70
      18E44CAD55FF65C86EFF71C979FF81CB88FF93CE99FFA5D0A9FFB4D2B7FFBFD3
      C1FFC3D4C4FFC1D4C2FFBBD2BDFFB1D2B4FFA5D0A9FFACC9AEFFF2F1F1FFD9E3
      DAFF6DC877FF3EA148FF0C5713D50000000F0000000000000000000000000526
      0962168321FF63C06CFF73C97BFF84CC8BFF98CE9DFFABD1AFFFBBD3BEFFC9D4
      C9FFCED5CEFFC9D4CAFFC0D3C2FFB5D2B8FFA8D0ACFF99CF9FFF99C79EFFC5D1
      C6FF5AB864FF11791BF7010A024D000000010000000000000000000000000000
      000109430F94168221FF62C16CFF7FCB87FF99CE9FFFADD1B0FFBED3C0FFCCD5
      CCFFD2D6D3FFCBD5CCFFC1D4C2FFB4D2B7FFA7D0ABFF99CE9FFF7DCB85FF5EBD
      68FF13801EFF041C067900000003000000000000000000000000000000000000
      00000001000409440F9A178321FF4CAF55FF78CB80FF97CE9CFFB9D2BDFFC6D4
      C7FFC9D4CAFFC5D4C7FFBDD3BFFFADD1B1FF8CCD93FF73CB7CFF46AA50FF1381
      1EFE0420077C0000000300000000000000000000000000000000000000000000
      00000000000000010002062D0A700F7619EF299132FF4CB056FF69C771FF7DCD
      85FF86CD8DFF84CD8CFF79CC82FF64C46EFF49AE53FF248E2FFF0D6A17E00212
      0455000000010000000000000000000000000000000000000000000000000000
      000000000000000000000000000001080213083D0D880F7119E9107E1CFF1B86
      25FF208B2BFF208B2BFF198624FF107E1BFD0E6716DA052709700000000A0000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000030108041D07470839
      0D9209410EA009420EA00526097B020C022D0000000100000000000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 0
    OnClick = BtnOkClick
  end
  object BtnCancel: TcxButton
    Left = 1135
    Top = 545
    Width = 83
    Height = 27
    Anchors = [akRight, akBottom]
    Caption = 'Abandon'
    ModalResult = 2
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000707193610103D821717
      56B91E1E6EEC1E1E71F21B1B64D61313489A0C0C2E620101050A000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000202070F0D0D316819195ECA1F1F7AFE18188DFF1212
      A4FF0F0FADFF0E0EB0FF1010A8FF141499FF1C1C82FF1E1E70F01212428D0505
      1125000000000000000000000000000000000000000000000000000000000000
      000000000000070719361B1B66DB1F1F79FF1212A0FF0303D2FF0000E4FF0000
      EAFF0000EDFF0000EDFF0000ECFF0000E8FF0000E0FF0B0BB7FF1B1B86FF2020
      76FC11113F880101020400000000000000000000000000000000000000000000
      0000090922491F1F72F513139AFF0202D2FF0000E3FF0000EEFF0000F5FF0000
      F8FF0000FAFF0000FAFF0000F9FF0000F7FF0000F2FF0000E9FF0000DAFF0B0B
      B1FF1D1D7DFF161654B301010205000000000000000000000000000000000707
      1A371E1E71F31010A1FF0000DAFF0000E7FF0000F0FF0000F7FF0000FBFF0000
      FDFF0000FDFF0000FDFF0000FDFF0000FCFF0000F9FF0000F3FF0000EAFF0000
      DFFF0404C6FF1C1C7FFF15154DA4000000000000000000000000000000001818
      5AC0191989FF0000D0FF0000E3FF0000EDFF0000F4FF0000F9FF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FEFF0000FEFF0000FBFF0000F7FF0000EFFF0000
      E8FF0000DAFF0909B2FF202077FF07071A37000000000000000008081D3E2020
      77FE0505B8FF0000DAFF0000E8FF0000F0FF0000F5FF0000FAFF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FFFF0000FEFF0000FCFF0000F8FF0000F2FF0000
      EBFF0000E2FF0000CFFF17178CFF171756B9000000000000000018185AC01313
      93FF0000CBFF0000DEFF0000E9FF0000F0FF0000F6FF0000FAFF0000FDFF0000
      FFFF0000FFFF0000FFFF0000FFFF0000FEFF0000FCFF0000F8FF0000F2FF0000
      ECFF0000E4FF0000D6FF0505B6FF202078FF07071B3904041022202076FD0606
      AEFF0000CFFF0000DEFF0000E8FF0000EFFF0000F5FF0000F9FF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FFFF0000FEFF0000FBFF0000F7FF0000F1FF0000
      EBFF0000E4FF0000D8FF0000C4FF181887FF1111408A10103A7D1E1E7AFF0000
      BAFF0000D0FF4040C1FF5858BBFF5757BDFF5757C0FF5757C2FF5757C4FF5757
      C5FF5757C5FF5757C5FF5757C5FF5757C4FF5757C3FF5757C1FF5757BEFF5757
      BCFF5555B8FF0000CFFF0000C5FF0F0F96FF19195CC61A1A61CF191983FF0000
      BAFF0000CDFFA7A7DCFFEFEFEFFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDED
      EDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFECEC
      ECFFCECECEFF0101C2FF0000C4FF0A0A9FFF1E1E6EEC1E1E70F0171785FF0000
      B9FF0000CAFFAAAADEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDFD
      FDFFD7D7D7FF0101BFFF0000C2FF0707A2FF1F1F74F91E1E6EEC171784FF0000
      B4FF0000C5FFAAAADDFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDFD
      FDFFD7D7D7FF0101BAFF0000BDFF07079FFF1F1F74F8171756B81A1A80FF0000
      AFFF0000BFFFAAAADBFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFE6E6E6FF0101B4FF0000B7FF0B0B96FF1C1C69E20D0D2F641F1F78FF0101
      A7FF0000B8FF3E3EC1FF6363CCFF5E5ECEFF5959D0FF5858D3FF5858D6FF5858
      D7FF5858D8FF5858D8FF5858D8FF5858D7FF5858D4FF5959D1FF5C5CCFFF6262
      CDFF6060CAFF0101B8FF0000B1FF12128BFF171755B70202070E1F1F73F60A0A
      96FF0000B0FF0C0CBDFF2525CAFF1818CBFF0909CCFF0202CFFF0000D3FF0000
      D6FF0000D7FF0000D8FF0000D7FF0000D4FF0101D1FF0505CEFF1313CBFF2323
      CBFF1B1BC4FF0000B6FF0000A8FF1B1B7DFF0F0F3775000000001414499C1919
      80FF0000A7FF0B0BB5FF3B3BC9FF3737CBFF1F1FC8FF0C0CC7FF0303C9FF0101
      CBFF0000CCFF0000CCFF0000CBFF0101CAFF0707C8FF1616C8FF3030CBFF3F3F
      CCFF2020BFFF0000ADFF090996FF1F1F75FA04040E1D000000000606152C2020
      77FF080896FF0303ACFF4242C7FF5858D0FF4141CCFF2323C7FF1010C4FF0606
      C3FF0303C4FF0202C4FF0404C4FF0B0BC4FF1919C5FF3535CAFF5353D0FF5151
      CDFF1515B6FF0000A5FF191980FF15154DA40000000000000000000000001515
      50AB1A1A7DFF0202A0FF2121B7FF6B6BD2FF7575D7FF5C5CD1FF3F3FCBFF2A2A
      C6FF2020C4FF2020C4FF2525C5FF3535C8FF4F4FCEFF6D6DD5FF7171D5FF3939
      C0FF0202A8FF0D0D8EFF1F1F74F9050513280000000000000000000000000303
      09141A1A62D2171781FF03039FFF2A2AB6FF7F7FD6FF9F9FE2FF9999E0FF8989
      DCFF7F7FD9FF7E7ED9FF8484DAFF9292DFFF9F9FE2FF9595DDFF4545C1FF0505
      A5FF0B0B91FF1F1F77FD0C0C2E62000000000000000000000000000000000000
      000004040D1C1A1A62D11B1B7DFF0A0A91FF1C1CABFF7777CFFFB6B6E7FFCCCC
      EFFFD2D2F1FFD2D2F1FFCFCFF0FFC2C2EBFF9898DCFF3E3EBAFF080899FF1515
      84FF1F1F75FB0E0E326C00000000000000000000000000000000000000000000
      0000000000000202060C14144A9E202077FE1B1B7CFF0F0F8EFF2626A8FF5757
      BFFF7777CDFF7C7CCEFF6767C6FF3A3AB3FF111199FF151583FF202077FF1B1B
      63D408081E400000000000000000000000000000000000000000000000000000
      000000000000000000000000000003030D1B10103A7C1C1C6AE3202077FF1B1B
      7CFF18187EFF18187FFF19197DFF1E1E79FF202076FC161652AF08081D3F0000
      0001000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000010104080A0A254F1414
      4BA01D1D6BE61E1E6FED19195ECA0E0E36730505112400000000000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 1
    OnClick = BtnCancelClick
  end
  object BtnAdauga: TcxButton
    Left = 8
    Top = 545
    Width = 71
    Height = 27
    Anchors = [akLeft, akBottom]
    Caption = 'Adauga'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      0000302009492E1B074F2E1B074F2E1B074F2E1B074F2E1B074F2E1B074F2E1B
      074F2E1B074F2E1B074F261D074F0F370C740D6116C4127D20F3128122F81072
      1BE40B5112A30C1E074300000000000000000000000000000000000000000000
      0000BA8436ECD38F50FFD39253FFD39355FFD39355FFD39355FFD39355FFD393
      55FFD39355FF978940FF197E1EFF1F963BFF23B75CFF15C661FF16C661FF26C4
      69FF2BA954FF19872AFD083B0D77000000000000000000000000000000000000
      0000BB883AECDEA164FFDEA669FFDEA76BFFDEA76BFFDEA76BFFDEA76BFFDEA7
      6BFFA39B53FF1A8528FF47BB73FF3EC879FF26C76BFF81C09CFF96BBA6FF44B5
      74FF3CC878FF51C581FF218C33FF07370C6F0000000000000000000000000000
      0000BD8B3FECE3AC73FFE3B179FFE3B37CFFE3B37CFFE3B37CFFE3B37CFFDFB2
      7AFF2F862BFF56BA7AFF57C988FF43C87CFF2FC770FFC8E5D4FFF2F2F2FF57B4
      7FFF40C87AFF55C987FF65C58CFF1A8327FB0314052900000000000000000000
      0000BF8F44ECE8B782FFE8BD89FFE8BF8CFFE8BF8CFFE8BF8CFFE8BF8CFFACAC
      6CFF339747FF6DCA95FF5AC98AFF4AC880FF3CC878FFCBE5D6FFF4F4F4FF60B5
      85FF4AC880FF59C989FF6ACA94FF41A35AFF09450F8B00000000000000000000
      0000C19348ECEEC391FFEEC999FFEECB9CFFEECB9CFFEECB9CFFEECB9CFF6F9F
      53FF4DAB68FF79C399FFA0BEADFF97BAA6FF96BAA6FFE1E8E4FFF8F8F8FFA8BE
      B2FF98BAA6FF9AB9A7FF7CB494FF5FBD83FF0F6D18DC00000000000000000000
      0000C2964DECF3CFA1FFF3D5AAFFF3D7ADFFF3D7ADFFF3D7ADFFF3D7ADFF649F
      51FF4CAF69FF83C9A1FFFEFEFEFFFBFBFBFFFBFBFBFFFEFEFEFFFFFFFFFFFCFC
      FCFFFBFBFBFFF8F8F8FF91B9A3FF66C78EFF10791AF500000000000000000000
      0000C49A51ECF8DAAFFFF8E0B9FFF8E2BDFFF8E2BDFFF8E2BDFFF8E2BDFF70A7
      5EFF43AC62FF6CC693FFA9D7BDFFACD8BFFFB3D8C3FFEAF0EDFFFBFBFBFFC4D7
      CCFFB4D8C4FFAFD8C0FF81C69EFF5DC286FF107419EA00000000000000000000
      0000C59C55ECFCE2BCFFFCE9C7FFFCEBCBFFFCEBCBFFFCEBCBFFFCEBCBFFB2C9
      94FF2F9A48FF59C989FF60CA8EFF6ECA96FF82CBA1FFDBE6DFFFF4F4F4FF92B8
      A2FF83CBA2FF72CA98FF64CA90FF42AB61FF0B5012A200000000000000000000
      0000C59E59ECFFE8C6FFFFEFD1FFFFF1D5FFFFF1D5FFFFF1D5FFFFF1D5FFF7ED
      CFFF2C8D34FF51BD7AFF65CA90FF79CB9CFF93CCABFFDEE6E1FFF5F5F5FF9BB8
      A7FF8ECCA8FF79CB9CFF64C88FFF1F8B30FF0420074100000000000000000000
      0000C59F5AECFFECCCFFFFF3D7FFFFF5DBFFFFF5DBFFFFF5DBFFFFF5DBFFFFF5
      DBFFA8CA96FF1E8A2FFF5BC183FF7BCB9DFF9ACCAFFFC5D2CAFFD4DED8FFA3C4
      B1FF8FCCA9FF72CA98FF319C4AFF0B4F119F0000000000000000000000000000
      0000C5A05CECFFEED1FFFFF5DCFFFFF8E1FFFFF8E1FFFFF8E1FFFFF8E1FFFFF8
      E1FFFFF8E1FF9CC58FFF158121FF37A254FF6FC592FF92CCABFF96CCADFF84CB
      A2FF5BBE81FF25923BFF0C5A14B6000301060000000000000000000000000000
      0000C5A15DECFFF0D5FFFFF7E0FFFFFAE5FFFFFAE5FFFFFAE5FFFFFAE5FFFFFA
      E5FFFFFAE5FFFFFAE5FFE1EBCCFF7FB779FF2F8E37FF208D32FF239037FF1C89
      2DFF449745FF52652CBF00000001000000000000000000000000000000000000
      0000C5A15EECFFF1D9FFFFF8E4FFFFFBE9FFFFFBE9FFFFFBE9FFFFFBE9FFFFFB
      E9FFFFFBE9FFFFFBE9FFFFFBE9FFFFFBE9FFFEFBE9FFDFEBCEFFD1E4C2FFEFF2
      DAFFFFF5DFFF856944B000000000000000000000000000000000000000000000
      0000C5A25FECFFF3DCFFFFFAE8FFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFD
      EDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFDEDFFFFFC
      ECFFFFF7E3FF856A44B000000000000000000000000000000000000000000000
      0000C5A260ECFFF3DFFFFFFAEBFFFFFDF0FFFFFDF0FFFFFDF0FFFFFDF0FFFFFD
      F0FFFFFDF0FFFFFDF0FFFFFDF0FFFEFDEFFFFDFCEFFFFDFBEEFFFEFCEFFFFEFB
      EEFFFDF6E4FF7F6949B100000001000000000000000000000000000000000000
      0000C5A361ECFFF4E2FFFFFBEEFFFFFEF3FFFFFEF3FFFFFEF3FFFFFEF3FFFFFE
      F3FFFFFEF3FFFFFEF3FFFFFEF3FFFCFBEFFFF7F5E8FFF3F0E2FFF5F1E3FFF7F3
      E4FFF7EEDCFF7D6747B203020107010100020000000000000000000000000000
      0000C5A362ECFFF4E4FFFFFBF0FFFFFEF5FFFFFEF5FFFFFEF5FFFFFEF5FFFFFE
      F5FFFFFEF5FFFFFEF5FFFEFDF4FFF7F4E9FFE9E3D3FFDCD3C0FFDBD2BFFFE1D7
      C4FFE7D9C4FF7B6343B50604020C010100020000000000000000000000000000
      0000C5A363ECFFF4E6FFFFFBF3FFFFFEF8FFFFFEF8FFFFFEF8FFFFFEF8FFFFFE
      F8FFFFFEF8FFFFFEF8FFFCFBF4FFF0EBE1FFD6CAB2FFC2A97DFFBBAA8DFFC7B6
      9CFFD8C3A6FF4F3E22820503010A010100020000000000000000000000000000
      0000C5A363ECFFF4E7FFFFFBF4FFFFFEF9FFFFFEF9FFFFFEF9FFFFFEF9FFFFFE
      F9FFFFFEF9FFFFFEF9FFFCF9F4FFECE7DDFFD3BC94FFF3C996FFD9B680FFD6B6
      84FF6F5223AB0B08031702020105000000000000000000000000000000000000
      0000C5A263ECFFF2E6FFFFF9F2FFFFFBF7FFFFFBF7FFFFFBF7FFFFFBF7FFFFFB
      F7FFFFFBF7FFFFFBF7FFFCF8F3FFEEE7DEFFDFC89FFFFFE8C7FFFFE9C8FF9479
      49C5100C05220403010900000001000000000000000000000000000000000000
      0000C5A161ECFFEEDEFFFFF3E8FFFFF5ECFFFFF5ECFFFFF5ECFFFFF5ECFFFFF5
      ECFFFFF5ECFFFFF5ECFFFDF3E9FFF5EADEFFE5CCA7FFFFE9D0FFAA9169D0130E
      06290503010A0000000100000000000000000000000000000000000000000000
      00007359299388724C9F88734F9F88744F9F88744F9F88744F9F88744F9F8874
      4F9F88744F9F88744F9F87724F9F7C69499F745E3AA17A633DA6181207300503
      010A010100020000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000010101000303020106020201050101
      00020000000000000000000000000000000000000000}
    TabOrder = 2
    OnClick = BtnAdaugaClick
  end
  object BtnAnuleaza: TcxButton
    Left = 85
    Top = 545
    Width = 76
    Height = 27
    Anchors = [akLeft, akBottom]
    Caption = 'Anuleaza'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000001000004000005400202208403032FA102022B9D0101
      12740000002D0000000200000000000000000000000000000000000000000000
      0000AB772CE6B87638F9B8783AF9B8783BF9B8783BF9B8783BF9B8783BF9B878
      3BF9B8783BF99E673CF9291B7AFA0F0FB5FF1414CAFF1010D6FF1616D1FF1414
      BDFF0F0C99FE0A061DA30000000E000000000000000000000000000000000000
      0000BB8739ECDC9E60FFDCA264FFDCA366FFDCA366FFDCA366FFDCA366FFDCA3
      66FFB7886DFF1F1AA3FF3232D9FF3030FDFF1515FFFF0606FFFF1718FFFF3434
      FFFF4544ECFF1818B6FE030333B20000000B0000000000000000000000000000
      0000BC8B3DECE1A86EFFE1AD74FFE1AF76FFE1AF76FFE1AF76FFE1AF76FFDFAD
      76FF362BA0FF4949DEFF6969EAFF5252DEFF3E3FDEFF1010FBFF2A2AEFFF5353
      DEFF6868DEFF6A6AE3FF1919B3FD0101117C0000000000000000000000000000
      0000BF8E43ECE7B37DFFE7B984FFE7BB86FFE7BB86FFE7BB86FFE7BB86FF9F81
      91FF3535C9FF7979FFFF8787E6FFF2F2F2FFE2E2E2FF5656D0FFACACDCFFEDED
      EDFFDDDDE0FF7676EBFF6464E7FF070780EA0000001700000000000000000000
      0000C09146ECECC08CFFECC594FFECC797FFECC797FFECC797FFECC797FF4A3F
      A8FF6767E6FF7A7AFFFF5F5FFBFFD1D1EFFFFEFEFEFFDADADEFFF6F6F6FFFCFC
      FCFF9A9ADDFF7474FFFF8686FEFF1616B7FF0000074F00000000000000000000
      0000C1954BECF1CA9BFFF1D0A4FFF1D2A7FFF1D2A7FFF1D2A7FFF1D2A7FF2C28
      AAFF7676F3FF7676FFFF6665FFFF7D7EEBFFFEFEFEFFFFFFFFFFFEFEFEFFD7D7
      E4FF6C6CF5FF7575FFFF7E7EFFFF2C2CC5FF0101156C00000000000000000000
      0000C39950ECF7D7ABFFF7DDB4FFF7DFB8FFF7DFB8FFF7DFB8FFF7DFB8FF2A27
      ABFF6A6AF4FF7170FFFF6B6BFFFF8281E7FFFCFCFCFFFFFFFFFFFEFEFEFFC5C5
      D1FF7576F6FF7575FFFF7575FFFF2E2EC7FF0101176D00000000000000000000
      0000C49B54ECFBDFB8FFFBE6C3FFFBE8C7FFFBE8C7FFFBE8C7FFFBE8C7FF4540
      B5FF5757EBFF6A6AFFFF7170FAFFCDCDE4FFFEFEFEFFF3F3F6FFFCFCFDFFF1F1
      F1FF9595CCFF7878FFFF7272FFFF2020BDFF01010B4D00000000000000000000
      0000C59D58ECFEE7C3FFFEEECEFFFEF0D2FFFEF0D2FFFEF0D2FFFEF0D2FF968E
      C3FF3838D1FF6969FFFF9595E8FFFDFDFDFFFFFFFFFFC4C4E3FFD9D9ECFFFFFF
      FFFFE2E2E4FF7F7FEFFF6768F4FF09099BF20000001100000000000000000000
      0000C59F5AECFFEBCAFFFFF2D6FFFFF4DAFFFFF4DAFFFFF4DAFFFFF4DAFFF6EC
      D8FF2524B5FF5E5EEFFF8282F4FFA2A2EEFFBFBFEEFFD7D7FDFFC9C9F8FFAFAF
      EEFF9796EEFF7E7EF9FF2D2DC7FF0202277D0000000000000000000000000000
      0000C5A05CECFFEECFFFFFF5DBFFFFF7DFFFFFF7DFFFFFF7DFFFFFF7DFFFFFF7
      DFFFBFBAD3FF1E1EB7FF5D5DE9FF9F9FFFFFCCCBFFFFDBDBFFFFCDCDFFFFB0B1
      FFFF8383FCFF3131CBFF06065BB5000000040000000000000000000000000000
      0000C5A15DECFFEFD4FFFFF6DFFFFFF9E4FFFFF9E4FFFFF9E4FFFFF9E4FFFFF9
      E4FFFFF9E4FFC8C3D8FF2929B6FF3030CCFF5858E6FF7474EEFF6868ECFF4444
      DCFF1919BAFF120D55A900000004000000000000000000000000000000000000
      0000C5A15EECFFF1D8FFFFF8E3FFFFFBE8FFFFFBE8FFFFFBE8FFFFFBE8FFFFFB
      E8FFFFFBE8FFFFFBE8FFFBF7E7FFADABD5FF5958C2FF3E3DBBFF4646BDFF7977
      C8FFD9CDCFFF39260D5B00000000000000000000000000000000000000000000
      0000C5A25FECFFF2DBFFFFF9E7FFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFC
      ECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFA
      E9FFFDF1D9FF39260D5B00000000000000000000000000000000000000000000
      0000C5A260ECFFF3DEFFFFFAEAFFFFFDEFFFFFFDEFFFFFFDEFFFFFFDEFFFFFFD
      EFFFFFFDEFFFFFFDEFFFFFFDEFFFFFFDEFFFFEFCEFFFFEFCEFFFFFFDEFFFFFFB
      EBFFFCF0DBFF3325115C00000000000000000000000000000000000000000000
      0000C5A361ECFFF4E1FFFFFBEDFFFFFEF2FFFFFEF2FFFFFEF2FFFFFEF2FFFFFE
      F2FFFFFEF2FFFFFEF2FFFFFEF2FFFCFBEEFFF9F7EAFFF8F6E9FFFAF8EBFFFBF7
      E8FFF8ECD8FF3426125F01010003000000010000000000000000000000000000
      0000C5A361ECFFF4E3FFFFFBEFFFFFFEF4FFFFFEF4FFFFFEF4FFFFFEF4FFFFFE
      F4FFFFFEF4FFFFFEF4FFFDFCF2FFF5F2E6FFE9E4D3FFE3DCCAFFE5DFCDFFE9E1
      CFFFEBDCC6FF3527126403020107000000010000000000000000000000000000
      0000C5A362ECFFF4E5FFFFFBF2FFFFFEF7FFFFFEF7FFFFFEF7FFFFFEF7FFFFFE
      F7FFFFFEF7FFFFFEF7FFFBFAF1FFEBE6D9FFD3C6ADFFC4B69DFFC7B89FFFD3C3
      ACFFDFCAB0FF2E220F5703020107000000010000000000000000000000000000
      0000C5A363ECFFF4E7FFFFFBF4FFFFFEF9FFFFFEF9FFFFFEF9FFFFFEF9FFFFFE
      F9FFFFFEF9FFFFFEF9FFF9F7F0FFE3DCCEFFD5B380FFD3AB70FFBCA071FFC9AB
      7BFF684F27A20806031202010104000000000000000000000000000000000000
      0000C5A263ECFFF3E7FFFFFAF3FFFFFCF8FFFFFCF8FFFFFCF8FFFFFCF8FFFFFC
      F8FFFFFCF8FFFFFCF8FFF9F5F0FFE3DCCFFFE5C48DFFFEE5C1FFFDE2BDFF856A
      3DB80E0A041E0302010700000001000000000000000000000000000000000000
      0000C5A162ECFFF0E2FFFFF6ECFFFFF8F1FFFFF8F1FFFFF8F1FFFFF8F1FFFFF8
      F1FFFFF8F1FFFFF8F1FFFBF3EBFFECE1D4FFE9C996FFFFEBD0FF9F875CCA110D
      06250503010A0000000100000000000000000000000000000000000000000000
      0000B7924EE2E3C9A5F4E3CCAAF4E3CEACF4E3CEACF4E3CEACF4E3CEACF4E3CE
      ACF4E3CEACF4E3CEACF4E0CBAAF4DBC4A3F4CCA972F4C5A678E3140F06290504
      020B010100020000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000010100020202010504030108030201060101
      00020000000000000000000000000000000000000000}
    TabOrder = 4
    OnClick = BtnAnuleazaClick
  end
  object BtnSterge: TcxButton
    Left = 167
    Top = 545
    Width = 69
    Height = 27
    Anchors = [akLeft, akBottom]
    Caption = 'Sterge'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000202
      01031814122739312C58534A438774685FBA877C70E8706158CF4D433D850C0A
      0A12000000000000000000000000000000000000000000000000000000000000
      00000000000000000000010000010D0B0A162B2420464A3E38776F5E54AF9D86
      77F1A99182FFAB9787FFAB9A8DFFAA9B8EFFA4988CFF99887DFF907C71FF8472
      67E53D3B394B0000000000000000000000000000000000000000000000000101
      0101473B36776F5E55BC917B6FEDA2897CFFA48C7EFFA78E80FFB0998AFFB9A0
      92FFBCA495FFBEAB9CFFC0B0A3FFC0B1A5FFBCB0A5FFB2A398FFA9968BFFA794
      89FFAA968BFF5C55517F00000000000000000000000000000000000000000101
      01019C7E72F2A88C7FFFB1988CFFC1AEA7FFA88E87FFA9918AFFB19E95FFB6A2
      94FFCBB2A1FFDAC4B4FFDFCEC2FFE2D5CAFFDBD1C6FFD1C5BDFFC3B2A7FFBDA8
      9CFFBCA597FF74645AFF0D0A0ACA0E0B0B500000000100000000000000000D0A
      0913AA8A7DFFAE8E7FFFC8B2A7FF886359FF825B51FFA58A83FFAB928BFFBCA7
      A1FFC8B9B0FFEDDED5FFFAF3ECFFFCF6F2FFF2EDE9FFE6DFDAFFD5C7BFFFC9B4
      A9FFC2AA9CFF8A766AFF030303FF010101FF130F0F7E00000000000000001E19
      172DB09084FFB7988BFFD1BBB3FF8F675DFFB8A09AFFBCABA1FFE7D2C9FFF7E7
      DFFFF5ECE8FFEDE1D9FFFCF6F1FFFFFCF9FFFAF9F7FFF5F4F3FFEAE5E1FFD8CA
      C1FFC8B3A6FFA28C7FFF040404FF000000FF070606EC0000000000000000342B
      284BB6998EFFC1A59AFFD8C3BBFFAB8A81FFBFA7A1FFDBCFCCFFD2C0B7FFF8E5
      DCFFFDF1ECFFCFBFBAFFD4CDC7FFFDFAF8FFFEFDFDFFFEFEFEFFF5F2F0FFE2D7
      D1FFCDBAAEFFBAA293FF070707FE0C0908D70E0B0B540000000000000000483C
      3768B89B90FFC4A99EFFD9C8C3FFC2A8A1FFAA867DFFBAA8A2FFD7C4BBFFF7E5
      DCFFF6EBE6FFAD8A81FFCCC0BBFFD9D5D1FFF9F8F7FFFDFDFCFFF6F3F2FFE5DB
      D6FFCFBDB2FFC0A899FF322F2E49010101010000000000000000000000005C4D
      4786B89B90FFC3A79CFFD8C5BDFFD4C2BCFFAE8E85FFBAABA4FFDDCAC1FFF7E4
      DBFFD7C4BEFFAC8A81FFBB9F98FFE3DBD7FFEAE8E6FFFEFEFEFFF8F5F4FFE7DE
      D9FFD1BFB5FFC1A99BFF413E3C4901010101000000000000000000000000715D
      56A4B89B90FFC2A69BFFC9ADA2FFD1BAAFFFE5D8D2FFC9BDB7FFD6C3BAFFF7E7
      E0FFE7DDD8FFB6A097FFC0AFA8FFCAC5C0FFFDFCFCFFFFFEFEFFFAF8F7FFE9E1
      DCFFD3C3B9FFC3AB9DFF57514E6502020202000000000000000000000000826B
      63BDB89B91FFC2A69BFFC8ACA1FFD2BBB0FFE5D2CAFFDCD4D0FFCBBEB7FFDECB
      C1FFF4E7E1FFA38F85FFB8AAA2FFCCC7C2FFFDFDFCFFFEFEFDFFFBF9F9FFECE5
      E1FFD6C7BEFFC4ADA0FF665F5B7A030303030000000000000000000000008F77
      6ED1B89B90FFC2A69BFFC6AA9FFFD1BAAFFFE1CCC4FFE3D6D0FFA89C92FFB1A7
      9EFFA6998FFF887668FFB6ACA4FFDAD4D0FFFCFCFAFFFEFEFEFFFDFCFCFFEEE8
      E5FFD7C8C0FFC6AFA1FF766B659103030303000000000000000000000000A185
      7BEAB89B90FFC1A59BFFC5A89DFFD0B9AFFFE0CAC1FFE7D2C9FFE7D9D2FFB4AA
      A1FF8F8376FF998E83FFD4CEC7FFF6F1EDFFFBF9F7FFFDFDFCFFFBFAF9FFEBE4
      E0FFD5C4BBFFC4AD9FFF82736BA70303030300000000000000000806060BB599
      8FFDC2A89DFFCCB4AAFFCDB5AAFFD8C3B9FFE4D0C8FFEAD7CFFFEEDDD4FFF4E2
      DAFFF8EBE4FFF5E8E1FFF9F0EAFFFDF7F4FFF7F4F0FFF8F6F4FFF3EEECFFE3D8
      D2FFD2C0B6FFC3AA9CFF8D7A6FC00202020200000000000000001C181726C6AD
      A4FFD4BFB5FFDECAC2FFDBC6BDFFE0CFC6FFE9DAD2FFEDDED7FFF2E3DCFFF3E5
      DDFFF1E3DBFFE6DBD2FFDBD2C9FFC9BFB5FFB0A192FFAF9F91FFC2B7ACFFD0C3
      BBFFD6C3BBFFC9B2A6FF9B8376DB010101010000000000000000362F2D44D4BF
      B6FFE0CDC5FFE5D4CCFFDFCCC3FFDECFC5FFDDCFC5FFD5C7BCFFC7B8AAFFBAAA
      9AFFB9AA9BFFC3B7AAFFCFC5BBFFDED7D0FFCEC3B8FFEAE6E1FFD7CFC6FFC7BB
      AFFFC2B2A5FFD2BFB5FFB49B8EF30403030500000000000000005D54506FDCCB
      C2FFDECFC4FFCEC0B4FFBDAFA2FFB8A897FFBCAD9DFFCCC1B6FFE6E1DBFFFAF9
      F7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDAD2CAFFFDFDFDFFFFFFFFFFFFFF
      FFFF8F7C6CFFA99A8DFFC9B7AAFE1512111C00000000000000002624222F9288
      7EBF99897DFF736257FF6F5E53FFD5CBC1FFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDAD2CAFFFDFDFDFFFFFFFFFFFFFF
      FFFFA69687F4867B72C077706898100F0E140000000000000000000000000101
      010123211E2F645D558C86796DE4D8D0C7FFFEFEFDFFFEFEFDFFFEFEFDFFFEFE
      FDFFFEFEFDFFFEFEFDFFFEFEFDFFFEFEFDFFDAD1C9FFFBF9F7FFFBFAF8FFFBFA
      F8FF827669B90000000000000000000000000000000000000000000000000000
      0000000000000000000019161327DAD1C8FFF8F5F2FFF7F4F1FFF7F4F1FFF7F4
      F1FFF7F4F1FFF7F4F1FFF7F4F1FFF7F4F1FFD6CCC2FFE0D8CFFFDCD3CAFFD6CD
      C3FE8C7E6FCB0000000000000000000000000000000000000000000000000000
      000000000000000000000807060D3B332A6250463B7E60554B90776C60A89F92
      85D6BFB1A4F7CDC0B3FFD7CCC1FFE3DAD2FFCCC0B4FB1A16122B100E0B1B0505
      0409010101020000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000001010102171410262D261F4B4A3F347C5C4F419800000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 5
    Visible = False
    OnClick = BtnStergeClick
  end
  object BtnRevalidariDocum: TcxButton
    Left = 242
    Top = 545
    Width = 60
    Height = 27
    Anchors = [akLeft, akBottom]
    Caption = 'Revalidari'
    TabOrder = 6
    Visible = False
    OnClick = BtnRevalidariDocumClick
  end
  object btnImpExp: TcxButton
    Left = 308
    Top = 545
    Width = 104
    Height = 27
    Anchors = [akLeft, akBottom]
    Caption = 'Import/Export'
    DropDownMenu = SelectMenu
    Kind = cxbkDropDownButton
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360400000000000036000000280000001000000010000000010020000000
      000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EFF1EFFFD0D9D0FFF8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DEE5DEFF1A8318FFABBE
      ABFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DDE6DDFF0C9D07FF0A89
      06FF84A884FFF6F6F6FFF8F8F800F8F8F800F8F8F800F8F8F800569E55FF2D8B
      2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8F2AFF0AA103FF08A4
      00FF079401FF649663FFF0F0F0FFF8F8F800F8F8F800F8F8F80036AD32FF08A8
      00FF08A800FF08A800FF08A800FF08A800FF08A800FF08A800FF08A800FF08A8
      00FF08A800FF069F00FF448D41FFE0E4E0FFF8F8F800F8F8F8003BAE38FF08AD
      00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD
      00FF08AD00FF08AD00FF07A900FF308F2DFFE7EAE7FFF8F8F8003AB036FF08B0
      00FF08B000FF08B000FF08B000FF08B000FF08B000FF08B000FF08B000FF08B0
      00FF08B000FF08B000FF1AAF14FFB0D5AFFFF8F8F800F8F8F80039B435FF08B5
      00FF08B500FF08B500FF08B500FF08B500FF08B500FF08B500FF08B500FF08B5
      00FF08B500FF27B422FFC6E0C6FFF8F8F800F8F8F800F8F8F800BFDCBDFFB3DA
      B0FFB3DAB0FFB3DAB0FFB3DAB1FFB3DAB3FFB3DAB3FFA1CFA0FF0BB505FF08B8
      00FF3EBB3BFFDCE9DCFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DEE6DEFF0CB807FF58C0
      55FFE8EEE8FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800E5EBE4FF84CA84FFF4F5
      F4FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
      F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
    OptionsImage.Layout = blGlyphRight
    TabOrder = 7
    Visible = False
  end
  object Progress: TcxProgressBar
    Left = 418
    Top = 545
    Anchors = [akLeft, akRight, akBottom]
    AutoSize = False
    ParentColor = False
    Properties.BarBevelOuter = cxbvRaised
    Properties.BarStyle = cxbsGradient
    Properties.BeginColor = 3265321
    Properties.BorderWidth = 1
    Properties.EndColor = 11399085
    Properties.OverloadValue = 100.000000000000000000
    Properties.PeakValue = 100.000000000000000000
    Properties.ShowTextStyle = cxtsPosition
    Properties.SolidTextColor = True
    Style.BorderColor = clInfoBk
    Style.BorderStyle = ebsUltraFlat
    Style.Color = clWhite
    Style.Edges = [bLeft, bTop, bRight, bBottom]
    Style.Shadow = False
    Style.TextStyle = [fsBold]
    Style.TransparentBorder = True
    StyleHot.TextStyle = []
    TabOrder = 8
    Height = 28
    Width = 546
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 1235
    Height = 27
    Align = alTop
    TabOrder = 9
    DesignSize = (
      1235
      27)
    object Label1: TLabel
      Left = 16
      Top = 7
      Width = 49
      Height = 13
      Caption = 'An fiscal : '
    end
    object Label2: TLabel
      Left = 168
      Top = 7
      Width = 83
      Height = 13
      Caption = 'Luna Raportare : '
    end
    object Label3: TLabel
      Left = 371
      Top = 7
      Width = 50
      Height = 13
      Caption = '&Operator : '
    end
    object edListaAni: TcxImageComboBox
      Left = 67
      Top = 3
      Properties.Items = <>
      Properties.OnChange = edListaAniPropertiesChange
      TabOrder = 0
      Width = 95
    end
    object edListaLuni: TcxImageComboBox
      Left = 251
      Top = 3
      Properties.Items = <>
      Properties.OnChange = edListaLuniPropertiesChange
      TabOrder = 1
      Width = 113
    end
    object edOperator: TcxImageComboBox
      Left = 422
      Top = 3
      Anchors = [akLeft, akTop, akRight]
      Properties.Items = <>
      Properties.OnChange = edListaLuniPropertiesChange
      TabOrder = 2
      Width = 676
    end
    object btnRefresh: TcxButton
      Left = 1129
      Top = 3
      Width = 75
      Height = 20
      Anchors = [akTop, akRight]
      Caption = 'Refresh'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E0000000000000000000000000000000000000000
        0000272017FF080704FF382D1DFF58462EFF685438FF675438FF503F29FF2A22
        16FF020201FF0000000000000000000000000000000000000000000000000000
        00004C3E2CFF9F8763FFD0BA8EFFE9D5A6FFEDDAA8FFEDD8A5FFE4CE9AFFC5AC
        7DFF89704CFF231C12FF0000000000000000000000001B1712FF16130FFF5C4D
        3AFFBAA687FFECDEBDFFB1A18DFFA49584FFC4B59EFFF0E4C5FFF1E1BAFFEDD9
        A8FFE6D09CFF9E855DFF1F1911FF00000000000000002A241DFF4B4033FFB19F
        86FFF4ECD6FFB3A69AFF83726BFF83726BFF82716AFF978880FFE6DFD2FFF6EC
        D4FFF0DFB6FFE4CE9AFF8E744FFF0E0B08FF000000004A4036FF9A8978FFF5EF
        E4FFFCF9F1FF9E8F88FF8E7C75FFD8D2D0FFF7F6F6FFE1DDDBFFC3BAB6FFF1ED
        E8FFF9F3E3FFF0E0B9FFDAC391FF624E33FF00000000958373FFD6CEC6FFFEFE
        FDFFFFFEFEFFA5958EFFA4948EFFFBFAFAFFFEFDFDFFFFFFFFFFFFFFFFFFF6F5
        F4FFFDFCF8FFF6EED9FFEEDBAFFFB1976BFF0D0A07FFA39182FFF3F1EFFFFFFF
        FFFFFDFDFDFFB5A6A0FFA5938CFFC9BEBAFFFFFFFFFFFFFFFFFFFFFFFFFFDDD6
        D3FFFBFAF9FFFAF5E9FFF2E4C3FFCBB588FF2C2317FFAC9D8FFFFEFEFEFFFFFF
        FFFFD2C8C4FFA6938AFFA6928AFFD8CFCBFFFFFFFFFFFFFFFFFFFFFFFFFFC6B9
        B4FFC3B5B0FFFBF7F0FFF5EAD0FFD8C49EFF3B2F20FFB7A99DFFFEFDFDFFFFFF
        FFFFFFFFFFFFE7E1DFFFAC9A91FFEDE9E7FFFFFFFFFFFFFFFFFFFDFDFCFFAE9C
        93FFA69289FFC1B3A9FFEEE4D0FFD9C9A8FF3B3022FFC2B6ABFFF4F2F1FFFFFF
        FFFFFFFFFFFFFFFFFFFFE2DCD8FFFBFAF9FFFFFFFFFFFFFFFFFFE2DCD8FFBCAD
        A4FFA38F83FFE2DAD2FFF9F2E1FFCEBEA2FF272018FFC9BFB6FFE6E2DDFFFFFF
        FFFFFFFFFFFFFFFFFFFFFFFFFFFFF7F6F5FFFFFFFFFFFFFFFFFFFEFDFDFFDCD5
        D0FF9D897BFFD8CFC6FFFAF4E6FFAE9B83FF070604FFD0C7BFFFD5CEC7FFFCFC
        FBFFFFFFFFFFFFFFFFFFFFFFFFFFD1C8C1FFE0DBD6FFFDFCFCFFFDFDFDFFB8AA
        9FFF978373FFD7CFC7FFE4DDD1FF605243FF00000000D4CCC6FFD5CDC6FFE0DB
        D7FFFDFDFCFFFFFFFFFFFFFFFFFFFCFCFBFFC0B4AAFF958271FF94806FFF907B
        6AFF917D6CFFDFD9D4FF8E7F71FF0C0B09FF00000000D5CDC7FFD5CDC7FFC1BC
        B7FFE0DCD8FFFCFCFBFFFFFFFFFFFFFFFFFFFFFFFFFFF4F2F0FFBFB4A9FFB1A4
        97FFCFC6BEFF988B80FF0E0C0BFF0000000000000000D5CDC7FFD5CDC7FFB9B4
        AFFFB9B5B1FFDAD5D1FFEBE7E4FFF7F5F3FFFBFAF9FFFAF9F8FFEEEBE7FFD0C8
        C0FF7C726AFF100F0DFF000000000000000000000000D5CDC7FFD5CDC7FFB9B4
        AFFFB4B0ACFFB2AEAAFFA6A29EFF9E9A95FF8D8883FF837D78FF595450FF1B19
        18FF0000000000000000000000000000000000000000}
      TabOrder = 3
      OnClick = btnRefreshClick
    end
  end
  object btnTiparire: TcxButton
    Left = 970
    Top = 545
    Width = 88
    Height = 27
    Hint = 'Tiparire'
    Anchors = [akRight, akBottom]
    Caption = 'Tiparire'
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D361000000000000036000000280000002000000020000000010020000000
      000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00EFEFEFFF94846BFFAD9C
      94FFCED6D6FFEFF7F7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00EFDEC6FFD6AD94FF9C846BFF946B31FF946B
      42FF947B52FF9C8C63FFA5947BFFB5ADA5FFD6D6CEFFDEDEE7FFF7FFFFFFFFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00DED6C6FFCE946BFF8C4A18FF9C734AFFDEBD9CFFCEAD
      84FFCEA57BFFC69C6BFFB58452FFAD7B4AFF947342FF94734AFF9C8C6BFFAD9C
      84FFB5ADA5FFCED6DEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7F7EFFFCE845AFFBD7B63FFB59C7BFFEFE7CEFFE7CEB5FFDEC6
      ADFFD6BDA5FFD6B59CFFDEBD9CFFD6B594FFCEAD84FFCE9C73FFB59463FFB57B
      52FF9C7342FF8C6B4AFFA58C73FFF7EFE7FFFFFFFF00FFFFFF00FFFFFF00FFFF
      FF00DEDEE7FFB58C7BFFBD9C84FFC6B5B5FFD6D6D6FFD6E7E7FFE7F7FFFFFFFF
      FF00EFEFEFFFE7B58CFFE79463FFE7A58CFFF7F7EFFFFFFFFF00FFFFF7FFFFF7
      F7FFF7F7EFFFF7EFE7FFEFE7DEFFEFDED6FFE7D6C6FFE7D6C6FFE7CEBDFFDEC6
      ADFFEFCEBDFFD6AD8CFFC6AD84FFF7EFEFFFFFFFFF00FFFFFF00FFFFFF00F7FF
      FFFFA58C84FFA54210FFC67331FFC66B39FFBD734AFFAD7B52FFBD8C73FFB59C
      94FFD6B59CFFFFAD8CFFFFB594FFF7BD9CFFFFFFF7FFFFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7F7FFFFF7
      EFFFE7E7DEFFC6B594FFFFF7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00CED6
      DEFF9C5A31FFBD6B31FFF7B56BFFF7B55AFFEF9C4AFFE79442FFDE7B29FFCE7B
      39FFF7AD94FFFFB59CFFEFBDA5FFF7E7D6FFFFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FF
      FFFFCE9C73FFDE9C7BFFFFFFF7FFFFFFFF00FFFFFF00FFFFFF00F7FFFFFFAD94
      8CFFBD5A18FFC68452FFF7E7D6FFFFEFCEFFFFDEADFFFFDE9CFFFFE79CFFEFBD
      84FFD69C84FFE7B59CFFEFE7D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFF7FFDEB5
      94FFEF946BFFF7B59CFFF7EFEFFFFFFFFF00FFFFFF00FFFFFF00C6D6D6FFA563
      42FFDE8429FFCEA584FFF7F7FFFFFFFFFF00FFFFFF00EFEFEFFFA5736BFFB584
      63FFDEB594FFF7DED6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DEAD94FFF7AD
      9CFFF7CEBDFFF7DED6FFFFFFFF00FFFFFF00FFFFFF00F7FFFFFFA58C84FFC66B
      21FFEFAD6BFFCEBDB5FFF7F7EFFFFFFFFF00FFFFFF00CEBDB5FF5A0800FF9C63
      42FFDEAD7BFFFFCE94FFFFDEADFFFFE7BDFFFFE7CEFFFFEFDEFFFFF7E7FFFFFF
      F7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00F7F7F7FFCEB594FFE7A584FFEFA5
      7BFFD6B59CFFF7FFFFFFFFFFFF00FFFFFF00FFFFFF00DEE7EFFFAD734AFFEF8C
      31FFF7D6ADFFC6BDC6FFF7EFE7FFFFFFF7FFFFFFFF00D6B5ADFFA54218FFC67B
      52FF524221FF523918FF7B5229FF946331FFB57339FFCE8C4AFFE79C63FFF7B5
      73FFFFBD84FFFFC694FFFFD6ADFFFFDEBDFFC6AD84FFC69C84FFF7D6B5FFEF9C
      4AFFCE845AFFE7F7FFFFFFFFFF00FFFFFF00FFFFFF00DEDEE7FFBD8452FFFFB5
      63FFEFE7DEFFC6ADADFFF7F7DEFFFFFFE7FFFFFFFF00DEB5ADFFB54210FFD67B
      4AFFA57B5AFF7B6B52FF6B5A52FF524A42FF4A4231FF4A4229FF5A4229FF6B42
      31FF845231FF946339FFAD6B39FFB57339FFC69C7BFFF7E7DEFFFFFFFF00FFEF
      DEFFCE9C7BFFE7DEDEFFFFFFFF00FFFFFF00FFFFFF00DEDEEFFFCE8C52FFFFDE
      ADFFE7EFEFFFCEAD9CFFFFEFD6FFFFF7DEFFFFFFF7FFE7B5A5FFC64A18FFDE6B
      39FFF78C52FFFF9463FFFF9C63FFF79C6BFFE79C73FFCE9473FFB58C6BFF9473
      5AFF735A4AFF634A42FF4A2918FF5A3118FFD68463FFFFF7F7FFFFFFFF00F7EF
      E7FFCEAD8CFFE7DEDEFFFFFFFF00FFFFFF00FFFFFF00DEDEE7FFD69C6BFFFFFF
      E7FFDED6D6FFD6B5A5FFFFEFCEFFFFE7CEFFFFFFEFFFEFB594FFD64A21FFEF6B
      42FFF77342FFF76B39FFF77339FFFF7B42FFFF844AFFFF8C5AFFFF9C63FFFFA5
      6BFFFF9C6BFFEF9C6BFFCE8C63FFC67342FFDE734AFFFFEFEFFFFFFFFF00FFE7
      DEFFD6A584FFE7E7DEFFFFFFFF00FFFFFF00FFFFFF00DED6D6FFDEC6BDFFFFFF
      FF00D6BDBDFFEFCEA5FFFFE7BDFFFFDEB5FFFFFFDEFFE79C84FFE74A18FFFF73
      42FFFF7B42FFFF7B4AFFFF844AFFFF844AFFFF7342FFFF7339FFFF7339FFFF73
      42FFFF7B42FFFF844AFFFF8C5AFFF76331FFDE7B5AFFFFFFF7FFFFFFFF00FFE7
      CEFFCEA584FFE7E7E7FFFFFFFF00FFFFFF00FFFFFF00DEDED6FFD6D6D6FFF7F7
      FFFFD6BDA5FFFFD6A5FFFFDEADFFFFE7B5FFFFDEB5FFDE6342FFE75A29FFF784
      4AFFFF8C5AFFFF946BFFFF9C7BFFFF9C7BFFFF946BFFFF8452FFFF7342FFFF73
      42FFFF7342FFFF7342FFF76B42FFDE4210FFE79484FFFFFFFF00FFFFEFFFEFCE
      ADFFD6AD94FFF7F7FFFFFFFFFF00FFFFFF00FFFFFF00DED6D6FFDEDEDEFFEFE7
      EFFFEFB584FFFFD694FFFFCE9CFFFFEFB5FFDE9473FFC64218FFE77B4AFFE78C
      63FFEF9C73FFEFA584FFEFA584FFEFAD8CFFF7A584FFEF946BFFEF7B4AFFEF73
      42FFE77342FFE77342FFDE6329FFC64218FFEFD6C6FFFFFFE7FFFFEFD6FFEFB5
      94FFD6BDB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00E7DEDEFFD6CED6FFF7D6
      C6FFFFCE7BFFFFCE8CFFFFDE9CFFEFCEA5FFA54A29FFB55229FFCE7B52FFD68C
      6BFFDE9C84FFE7A58CFFE7A58CFFDEA58CFFE79C84FFDE9C73FFDE8C5AFFD66B
      42FFCE6B39FFCE6B39FFAD3100FFC67352FFFFF7D6FFFFE7C6FFFFDEB5FFDEA5
      84FFE7E7E7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00D6CECEFFDEB5
      8CFFF7CE94FFFFCE94FFF7C68CFFDE8C6BFFD6846BFFDE9473FFCE8C63FFC684
      63FFBD8463FFC68463FFBD8C6BFFC69473FFC6946BFFBD846BFFBD735AFFB563
      42FFAD5229FF943108FFA55231FFF7DEB5FFFFE7BDFFFFDEB5FFEFB584FFDEC6
      B5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00E7DE
      DEFFDEC6BDFFEFD6BDFFCE9C73FFBD5A39FFEFAD94FFF7B59CFFFFAD94FFFFAD
      8CFFEF9C84FFDE9C73FFD69473FFCE8C6BFFC6845AFFB57B5AFFAD734AFFA56B
      42FF8C4221FF8C4221FFE7CEA5FFFFEFB5FFFFD69CFFEFBD84FFD6BDB5FFF7FF
      FFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFFFFE7E7E7FFBD9484FFA5734AFFAD6B4AFFBD7B63FFC684
      6BFFCE8C6BFFDE9473FFF7A58CFFEFA58CFFF7A58CFFEFA584FFE7946BFFD684
      63FFB56B42FFEFCE94FFFFE7A5FFFFC684FFEFB57BFFDECEBDFFF7FFFFFFFFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00F7FFFFFFA58473FFC6734AFFD69C73FFB573
      4AFFCEAD94FFD6C6BDFF9C6B52FFB5734AFFC68463FFCE8C6BFFE79C8CFFD684
      73FFD6A58CFFF7DEADFFEFC694FFDEBD94FFE7DED6FFFFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00DEEFEFFFAD735AFFFFA58CFFFFB59CFFE794
      73FFFFFFFF00CEDEE7FFA56342FFE79C7BFFC67B52FFDEC6B5FFF7F7FFFFF7EF
      EFFFF7FFFFFFEFEFF7FFE7E7EFFFF7F7FFFFFFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00BDBDBDFFBD7B5AFFFFBD9CFFF7A584FFF7C6
      ADFFE7FFFFFF947B6BFFF7946BFFFFBD9CFFE7A58CFFFFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00B59C94FFA56339FFD69C7BFFD69C73FFD6B5
      94FFA59C8CFFBD7B5AFFFFBD9CFFE78C63FFE7B5A5FFFFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00E7DEDEFFA5735AFF9C6B4AFFA57B5AFF9C6B
      4AFF9C5239FFB57352FFB5734AFFAD6342FFEFE7D6FFFFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7F7FFE7D6CEFFDEC6BDFFDEBD
      B5FFDEB5A5FFCEAD94FFCEAD94FFEFDEDEFFFFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
    TabOrder = 10
    OnClick = btnTiparireClick
  end
  object DTDocum: TDataSource
    DataSet = QryDocumListaDocum
    Left = 312
    Top = 88
  end
  object QryDocumListaDocum: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'SELECT ID_GEST_DOCUM, A.ID_INITIAL, COD_DOCUM, D.NUME AS PREDATO' +
        'R, E.NUME AS PRIMITOR, A.NR_DOCUM, DATA_DOCUM, TOTALDOC AS TOTAL' +
        '_DOCUMENT, TOTALTVA AS TOTAL_TVA, B.NUMEINTREG, DATA_OPERARE, '
      
        'A.ID_DOCUMENT_CONEX, A.ID_TRANZACTIE, A.ID_incarcare,A.ID_Xml, A' +
        '.cif_emitent, A.cif_beneficiar  '
      
        '  FROM GEST_DOCUM A JOIN UTILIZATORI B ON (A.ID_UTILIZATORI = B.' +
        'ID_UTILIZATORI)'
      
        '       LEFT JOIN GEST_TIP_DOCUM C ON (A.ID_GEST_TIP_DOCUM = C.ID' +
        '_GEST_TIP_DOCUM)'
      
        '       LEFT JOIN REPARTITORI D ON (A.ID_PREDATOR = D.ID_REPARTIT' +
        'ORI)'
      
        '       LEFT JOIN REPARTITORI E ON (A.ID_PRIMITOR = E.ID_REPARTIT' +
        'ORI)'
      'WHERE A.STARE=1'
      'ORDER BY A.ID_INITIAL, ID_GEST_DOCUM')
    Params = <>
    Properties.Strings = (
      'KeyFields=ID_GEST_DOCUM')
    Left = 392
    Top = 88
    object QryDocumListaDocumID_GEST_DOCUM: TIntegerField
      FieldName = 'ID_GEST_DOCUM'
      ReadOnly = True
    end
    object QryDocumListaDocumID_INITIAL: TIntegerField
      FieldName = 'ID_INITIAL'
    end
    object QryDocumListaDocumCOD_DOCUM: TStringField
      FieldName = 'COD_DOCUM'
      Size = 64
    end
    object QryDocumListaDocumPREDATOR: TStringField
      FieldName = 'PREDATOR'
      Size = 255
    end
    object QryDocumListaDocumPRIMITOR: TStringField
      FieldName = 'PRIMITOR'
      Size = 255
    end
    object QryDocumListaDocumNR_DOCUM: TStringField
      FieldName = 'NR_DOCUM'
      Size = 100
    end
    object QryDocumListaDocumDATA_DOCUM: TDateTimeField
      FieldName = 'DATA_DOCUM'
    end
    object QryDocumListaDocumTOTAL_DOCUMENT: TFloatField
      FieldName = 'TOTAL_DOCUMENT'
    end
    object QryDocumListaDocumTOTAL_TVA: TFloatField
      FieldName = 'TOTAL_TVA'
    end
    object QryDocumListaDocumNUMEINTREG: TStringField
      FieldName = 'NUMEINTREG'
      Required = True
      Size = 50
    end
    object QryDocumListaDocumDATA_OPERARE: TDateTimeField
      FieldName = 'DATA_OPERARE'
    end
    object QryDocumListaDocumID_DOCUMENT_CONEX: TIntegerField
      FieldName = 'ID_DOCUMENT_CONEX'
    end
    object QryDocumListaDocumID_TRANZACTIE: TIntegerField
      FieldName = 'ID_TRANZACTIE'
    end
    object QryDocumListaDocumID_incarcare: TStringField
      FieldName = 'ID_incarcare'
      Size = 200
    end
    object QryDocumListaDocumID_Xml: TStringField
      FieldName = 'ID_Xml'
    end
    object QryDocumListaDocumcif_emitent: TStringField
      FieldName = 'cif_emitent'
    end
    object QryDocumListaDocumcif_beneficiar: TStringField
      FieldName = 'cif_beneficiar'
    end
  end
  object DTItemsi: TDataSource
    DataSet = QryItemsiListaDocum
    Left = 312
    Top = 136
  end
  object QryItemsiListaDocum: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT'
      '  C.DENUMIRE AS TIP_MATERIAL,'
      ''
      'FROM GEST_ITEMSI A JOIN GEST_GNMCL B ON (A.CODMAT = B.CODMAT)'
      
        '     LEFT JOIN GEST_TIP_MATERIAL C ON (C.ID_GEST_TIP_MATERIAL = ' +
        'A.ID_GEST_TIP_MATERIAL)'
      
        '     LEFT JOIN ANGAJAMENTE_DEFALCARE D ON (D.ID_ANGAJAMENTE_DEFA' +
        'LCARE = A.ID_ANGAJAMENTE_DEFALCARE)'
      'WHERE ID_GEST_DOCUM = :ID_GEST_DOCUM'
      'ORDER BY ID_GEST_ITEMSI'
      ' '
      ' '
      ' '
      ' '
      ' '
      '')
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID_GEST_DOCUM'
        ParamType = ptUnknown
      end>
    Properties.Strings = (
      'KeyFields=ID_GEST_ITEMSI')
    DataSource = DTDocum
    Left = 392
    Top = 136
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID_GEST_DOCUM'
        ParamType = ptUnknown
      end>
  end
  object ppDetaliiMenu: TPopupMenu
    Left = 105
    Top = 192
    object ppIntroducereClasific: TMenuItem
      Caption = 'Introducere Clasificatie'
      ShortCut = 16416
      OnClick = ppIntroducereClasificClick
    end
    object itemFisaMaterial: TMenuItem
      Caption = 'Fisa Material'
      OnClick = itemFisaMaterialClick
    end
  end
  object SelectMenu: TPopupMenu
    Left = 88
    Top = 374
    object CmdImportXML: TMenuItem
      Caption = 'Import din XML'
      OnClick = CmdImportXMLClick
    end
    object ImportdinSQL1: TMenuItem
      Caption = '-'
    end
    object CmdExportXML: TMenuItem
      Caption = 'Export in XML'
    end
    object CmdExportSQL: TMenuItem
      Caption = 'Export in SQL'
      OnClick = CmdExportSQLClick
    end
  end
  object SaveDialog: TSaveDialog
    Filter = 'SQL Script File|*.SQL'
    Options = [ofOverwritePrompt, ofHideReadOnly, ofPathMustExist, ofEnableSizing]
    Left = 159
    Top = 376
  end
  object ImgList: TImageList
    Left = 176
    Top = 80
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
      0000000000000000000000000000000000000000000000000000D6944A00D694
      5200D6945200D6945200D6945200D6945200D6945200D6945200D6945200D694
      5200D6945200F7EFE700000000000000000000000000000000009C6B39008463
      4A007B5A3900D6945200D6945200D6945200D6945200D6945200D6945200D694
      5200D6945200E7D6C60000000000000000000000000000000000000000000000
      000000000000C6BDBD00948C8C00948C8C00948C8C00948C8C00948C8C00948C
      8C00948C8C00948C840000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000DEA56300E7AD
      7300E7AD7300E7AD7300E7AD7300E7AD7300E7AD7300E7AD7300E7AD7300E7AD
      7300E7AD6B00F7EFE700000000000000000000000000BDA59400DEA56B00E7AD
      7300E7AD73007B5A4200E7AD7300E7AD7300E7AD7300E7AD7300E7AD7300E7AD
      7300E7AD7300EFD6C60000000000000000000000000000000000000000000000
      000000000000BDADA500E7D6CE00E7D6CE00E7D6CE00E7D6CE00E7D6CE00E7D6
      CE00E7D6CE00D6C6C60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000E7B57B00EFBD
      8C00EFBD8C00EFBD8C00EFBD8C00EFBD8C00EFBD8C00EFBD8C00EFBD8C00EFBD
      8C00EFBD8400F7EFE7000000000000000000000000009C7B6B00EFB58400EFBD
      8C00EFBD8C00CEA58C00E7BD8C00EFBD8C00EFBD8C00EFBD8C00EFBD8C00EFBD
      8C00EFBD8400EFDEC60000000000000000000000000000000000F7F7F700F7F7
      F700F7F7F700AD9C9C00DECEC600DECEC600DECEC600DECEC600DECECE00E7DE
      D600E7DED600DECEC60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000EFC68C00F7CE
      A500F7D6A500F7D6A500F7D6A500F7D6A500F7D6A500F7D6A500F7D6A500F7D6
      A500F7CE9C00F7EFE700000000000000000000000000E7D6C600F7C69C00F7CE
      A500CEAD8400EFCEA500947B5A00F7D6A500F7D6A500F7D6A500F7D6A500F7D6
      A500F7CE9C00EFDECE00000000000000000000000000FFFFFF00D6C6BD00D6C6
      BD00D6C6BD00D6C6BD00D6C6BD00D6C6BD00D6C6BD00D6C6BD00948C8400EFDE
      DE00EFDEDE00DED6CE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000F7D6A500FFE7
      BD00FFE7BD00FFE7BD00FFE7BD00FFE7BD00FFE7BD00FFE7BD00FFE7BD00FFE7
      BD00FFDEB500F7EFE70000000000000000000000000000000000FFDEAD00CEA5
      8400F7DEB5008C736300BDA58C00F7E7BD00FFE7BD00FFE7BD00FFE7BD00FFE7
      BD00FFDEB500EFDECE00000000000000000000000000FFFFFF00E7D6CE00E7D6
      CE00E7D6CE00E7D6CE00E7D6CE00E7D6CE00E7D6CE00E7D6CE00948C8C00EFE7
      E700EFE7E700E7D6D60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFDEB500FFEF
      CE00FFEFD600FFEFD600FFEFD600FFEFD600FFEFD600FFEFD600FFEFD600FFEF
      D600FFEFCE00F7EFE70000000000000000000000000000000000FFE7C600EFD6
      BD00EFDEBD00F7E7C600CEBDA500A5947B00FFEFD600FFEFD600FFEFD600FFEF
      D600FFEFCE00EFDECE00000000000000000000000000FFFFFF00EFDED600EFDE
      D600EFDED600EFDED600EFDED600EFDED600EFDED600EFDED600948C8C00F7EF
      E700F7EFE700E7DED60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFE7BD00FFF7
      DE00FFF7DE00FFF7DE00FFF7DE00FFF7DE00FFF7DE00FFF7DE00FFF7DE00FFF7
      DE00FFEFD600F7EFE70000000000000000000000000000000000FFEFCE00FFF7
      DE008C6B5A00FFF7DE008C735A00BD9C8400FFF7DE00FFF7DE00FFF7DE00FFF7
      DE00FFF7D600EFE7CE00000000000000000000000000FFFFFF00EFE7DE00EFE7
      DE00EFE7DE00EFE7DE00EFE7DE00EFE7DE00EFE7DE00EFE7DE009C8C8C00F7EF
      EF00F7EFEF00E7DEDE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFE7C600FFF7
      DE00FFFFE700FFFFE700FFFFE700FFFFE700FFFFE700FFFFE700FFFFE700FFFF
      E700FFF7DE00F7EFEF0000000000000000000000000000000000FFEFD600FFF7
      E700E7CEBD00F7EFD600EFDECE00E7DEC6009C948400FFFFE700FFFFE700FFFF
      E700FFF7DE00EFE7D600000000000000000000000000FFFFFF00F7E7E700F7E7
      E700F7E7E700F7E7E700F7E7E700F7E7E700F7E7E700F7E7E7009C8C8C00F7EF
      EF00F7EFEF00E7DEDE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFE7C600FFFF
      E700FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFF
      EF00FFF7DE00F7F7EF0000000000000000000000000000000000FFF7D600FFFF
      E700FFFFEF00846B5A00FFFFEF00735A4A00AD948400FFFFEF00FFFFEF00FFFF
      EF00FFF7E700EFE7D600000000000000000000000000FFFFFF00F7EFEF00F7EF
      EF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF009C8C8C00F7EF
      E700F7EFE700E7DEDE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFEFCE00FFFF
      EF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFFEF00FFFF
      EF00FFF7E700F7EFEF0000000000000000000000000000000000FFF7DE00FFFF
      EF00FFFFEF00DECEB500F7F7E700EFE7D600FFF7E700B5AD9C00FFFFEF00FFFF
      EF00FFF7E700EFDED600000000000000000000000000FFFFFF00F7EFEF00F7EF
      EF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00948C8C00F7EF
      E700F7EFE700E7DED60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFEFCE00FFFF
      EF00FFFFF700FFFFF700FFFFF700FFFFF700FFFFF700FFF7EF00F7EFE700F7F7
      E700F7EFDE00EFE7DE00FFFFFF00000000000000000000000000FFF7E700FFFF
      F700FFFFF700FFFFF7008C7B6B00FFFFF700FFFFF700A5847300F7EFE700F7F7
      E700F7EFDE00DECEBD00FFFFFF000000000000000000FFFFFF00F7EFEF00F7EF
      EF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00F7EFEF00948C8C00F7E7
      E700F7E7E700E7DED60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFEFD600FFFF
      F700FFFFF700FFFFF700FFFFF700FFFFF700FFFFF700EFE7D600CEBDA500CEBD
      A500DECEB500E7DED600FFFFFF00000000000000000000000000FFF7E700FFFF
      F700FFFFF700FFFFF700CEBDAD00FFFFF700FFFFF700E7DECE00A5948400CEBD
      A500DECEB500D6CEBD00FFFFFF000000000000000000FFFFFF00B5A5A500DECE
      CE00F7EFE700F7EFE700F7EFE700F7EFE700F7EFE700F7EFE7007B736B00C6B5
      B500C6B5B500C6B5B50000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFEFD600FFFF
      F700FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFF700DED6C600F7CE9C00D6B5
      7B00C6A57300FFFFF70000000000000000000000000000000000FFF7E700FFFF
      F700FFFFFF00FFFFFF00FFFFF7008C7B7300FFFFF700CEC6B500D6AD8400D6B5
      7B00C6A56B00FFF7F70000000000000000000000000000000000F7F7F700C6B5
      AD00EFDEDE00F7E7E700F7E7E700F7E7E700F7E7E700F7E7E700A5A5A5000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFE7D600FFF7
      EF00FFFFF700FFFFF700FFFFF700FFFFF700FFF7EF00E7DECE00FFEFCE00D6BD
      8C00F7F7EF00FFFFFF0000000000000000000000000000000000FFEFE700FFF7
      EF00FFFFF700FFFFF700FFFFF700F7E7DE00947B6B00C6AD9400FFEFCE00D6BD
      8C00F7F7EF00FFFFFF000000000000000000000000000000000000000000F7F7
      F700CEC6BD00E7DED600E7DED600E7DED600E7DED600E7DED600ADADAD000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000EFDEBD00EFDE
      BD00EFDEC600EFDEC600EFDEC600EFDEC600EFDEC600E7D6BD00D6BD8C00F7F7
      F700FFFFFF000000000000000000000000000000000000000000EFDEBD00EFDE
      BD00EFDEC600EFDEC600EFDEC600EFDEBD00E7D6BD00E7CEB500D6BD8C00F7F7
      F700FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00FFFFFFFFFFFF0000C003C003F8030000
      C0038003F8030000C0038003C0030000C003800380030000C003C00380030000
      C003C00380030000C003C00380030000C003C00380030000C003C00380030000
      C003C00380030000C001C00180030000C001C00180030000C003C003C01F0000
      C003C003E01F0000C007C007FFFF000000000000000000000000000000000000
      000000000000}
  end
  object pnDocument: TPopupMenu
    Left = 25
    Top = 136
    object mnuGenAng: TMenuItem
      Caption = 'Genereaza angajament in baza documentului'
      OnClick = mnuGenAngClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object mnuAsocierePlata: TMenuItem
      Caption = 'Asociere plata la documentul curent'
      OnClick = mnuAsocierePlataClick
    end
    object N2: TMenuItem
      Caption = '-'
    end
    object mnuSelDM: TMenuItem
      Caption = 'Asociere Tethys'
      OnClick = mnuSelDMClick
    end
    object mnuDezAsoc: TMenuItem
      Caption = 'Deasociere Tethys'
      OnClick = mnuDezAsocClick
    end
  end
  object stiluriAfisare: TcxStyleRepository
    Left = 240
    Top = 328
    PixelsPerInch = 96
    object stilInregistrareCurenta: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clGreen
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object stilDocumentCurent: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clAqua
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object stilDocumentConex: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clRed
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object stilDocumentAnulat: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWindow
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsStrikeOut]
      TextColor = clRed
    end
  end
end
