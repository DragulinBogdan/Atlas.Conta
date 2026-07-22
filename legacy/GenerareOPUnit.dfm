object frmGenerareOP: TfrmGenerareOP
  Left = 35
  Top = 444
  Caption = 'Generare Ordin de Plata'
  ClientHeight = 590
  ClientWidth = 874
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  WindowState = wsMaximized
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnClient: TPanel
    Left = 0
    Top = 0
    Width = 874
    Height = 590
    Align = alClient
    BevelOuter = bvNone
    BorderWidth = 10
    TabOrder = 0
    OnResize = pnClientResize
    object Splitter1: TSplitter
      Left = 10
      Top = 185
      Width = 854
      Height = 8
      Cursor = crVSplit
      Align = alTop
    end
    object GRTop: TGroupBox
      Left = 10
      Top = 10
      Width = 854
      Height = 175
      Align = alTop
      Caption = 'Cautare Facturi Fiscale '
      TabOrder = 0
      object pnTop: TPanel
        Left = 2
        Top = 15
        Width = 850
        Height = 57
        Align = alTop
        TabOrder = 0
        object Label6: TLabel
          Left = 8
          Top = 8
          Width = 122
          Height = 13
          Caption = 'Anul / &Luna de raportare :'
          FocusControl = edListaLuni
        end
        object Label7: TLabel
          Left = 9
          Top = 32
          Width = 100
          Height = 13
          Caption = '&Numar / Data Nota : '
          FocusControl = edNrNota
        end
        object Label8: TLabel
          Left = 310
          Top = 8
          Width = 76
          Height = 13
          Caption = '&Operator Nota : '
          FocusControl = edOperator
        end
        object Label9: TLabel
          Left = 311
          Top = 32
          Width = 45
          Height = 13
          Caption = 'Nr &Unic : '
          FocusControl = edNrNota
        end
        object edListaLuni: TcxImageComboBox
          Left = 202
          Top = 5
          Properties.Items = <>
          Properties.OnEditValueChanged = edNrNotaPropertiesEditValueChanged
          TabOrder = 0
          Width = 103
        end
        object edData: TcxDateEdit
          Left = 202
          Top = 29
          Properties.OnEditValueChanged = edNrNotaPropertiesEditValueChanged
          TabOrder = 1
          Width = 103
        end
        object edNrNota: TcxMaskEdit
          Left = 133
          Top = 29
          Properties.OnEditValueChanged = edNrNotaPropertiesEditValueChanged
          TabOrder = 2
          Width = 65
        end
        object edOperator: TcxImageComboBox
          Left = 390
          Top = 5
          Properties.Items = <>
          Properties.OnEditValueChanged = edNrNotaPropertiesEditValueChanged
          TabOrder = 3
          Width = 137
        end
        object edListaAni: TcxImageComboBox
          Left = 133
          Top = 5
          Properties.Items = <>
          Properties.OnChange = edListaAniChange
          TabOrder = 4
          Width = 65
        end
        object edNrUnic: TcxMaskEdit
          Left = 390
          Top = 29
          Properties.OnEditValueChanged = edNrNotaPropertiesEditValueChanged
          TabOrder = 5
          Width = 137
        end
      end
      object TreeCredit: TdxDBTreeList
        Left = 520
        Top = 134
        Width = 321
        Height = 161
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'CONT'
        ParentField = 'PARINTE'
        TabOrder = 1
        Visible = False
        OnDblClick = TreeCreditDblClick
        OnKeyDown = TreeDebitKeyDown
        DataSource = DTCredit
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
        TreeLineColor = clGrayText
        object TreeCreditCONT: TdxDBTreeListMaskColumn
          Caption = 'Cont'
          HeaderAlignment = taCenter
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CONT'
        end
        object TreeCreditROMANA: TdxDBTreeListMaskColumn
          Caption = 'Explicatie'
          HeaderAlignment = taCenter
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ROMANA'
          OnGetText = TreeCreditROMANAGetText
        end
      end
      object TreeDebit: TdxDBTreeList
        Left = 296
        Top = 126
        Width = 329
        Height = 169
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'CONT'
        ParentField = 'PARINTE'
        TabOrder = 2
        Visible = False
        OnDblClick = TreeCreditDblClick
        OnKeyDown = TreeDebitKeyDown
        DataSource = DTDebit
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
        TreeLineColor = clGrayText
        object TreeDebitCONT: TdxDBTreeListMaskColumn
          Caption = 'Cont'
          HeaderAlignment = taCenter
          Visible = False
          Width = 93
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CONT'
        end
        object TreeDebitROMANA: TdxDBTreeListMaskColumn
          Caption = 'Explicatie'
          HeaderAlignment = taCenter
          Width = 155
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ROMANA'
          OnGetText = TreeDebitROMANAGetText
        end
      end
      object gridNote: TcxGrid
        Left = 2
        Top = 72
        Width = 850
        Height = 101
        Align = alClient
        TabOrder = 3
        LookAndFeel.Kind = lfFlat
        object viewNote: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = DTFacturi
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.OnChanged = viewNoteDataControllerFilterChanged
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'NR'
          DataController.Options = [dcoAnsiSort, dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding]
          DataController.Summary.DefaultGroupSummaryItems.Separator = ', '
          DataController.Summary.DefaultGroupSummaryItems = <
            item
              Kind = skSum
              FieldName = 'VALOARE'
            end>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.ImmediateEditor = False
          OptionsCustomize.ColumnHiding = True
          OptionsData.Editing = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          Styles.Content = cxStyle5
          Styles.Footer = cxStyle7
          Styles.Header = cxStyle6
          Styles.Indicator = cxStyle6
          Styles.Preview = cxStyle8
          object viewNoteJURNAL: TcxGridDBColumn
            Caption = 'Jurnal'
            DataBinding.FieldName = 'JURNAL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 56
          end
          object viewNoteNRDOC: TcxGridDBColumn
            Caption = 'Nr.'
            DataBinding.FieldName = 'NRDOC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 50
          end
          object viewNoteDATA: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 53
          end
          object viewNoteEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 68
          end
          object viewNoteCONT_DEBT: TcxGridDBColumn
            Caption = 'Cont Deb.'
            DataBinding.FieldName = 'CONT_DEBT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 54
          end
          object viewNoteREPARTITOR_DEBIT: TcxGridDBColumn
            Caption = 'Repartitor Debit'
            DataBinding.FieldName = 'REPARTITOR_DEBIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 78
          end
          object viewNoteCONT_CRED: TcxGridDBColumn
            Caption = 'Cont Cred.'
            DataBinding.FieldName = 'CONT_CRED'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 66
          end
          object viewNoteREPARTITOR_CREDIT: TcxGridDBColumn
            Caption = 'Repartitor Credit'
            DataBinding.FieldName = 'REPARTITOR_CREDIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 77
          end
          object viewNoteVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 RON;-,0.00 RON'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 49
          end
          object viewNoteMODUL: TcxGridDBColumn
            Caption = 'Modul'
            DataBinding.FieldName = 'MODUL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Nota introdusa'
                ImageIndex = 0
                Value = '0'
              end
              item
                Description = 'Tranzactii'
                ImageIndex = 1
                Value = '1'
              end
              item
                Description = 'Casa/Banca'
                ImageIndex = 2
                Value = '2'
              end
              item
                Description = 'Mijloace Fixe'
                ImageIndex = 3
                Value = '4'
              end
              item
                Description = 'Salarizare'
                ImageIndex = 4
                Value = '8'
              end>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 40
          end
          object viewNoteBUGET: TcxGridDBColumn
            Caption = 'Buget'
            DataBinding.FieldName = 'BUGET'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 36
          end
          object viewNoteCOD: TcxGridDBColumn
            Caption = 'Cod'
            DataBinding.FieldName = 'COD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object viewNotePOZ: TcxGridDBColumn
            Caption = 'Poz'
            DataBinding.FieldName = 'POZ'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object viewNoteECL: TcxGridDBColumn
            Caption = 'Ecl'
            DataBinding.FieldName = 'ECL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 28
          end
          object viewNoteCOMPUSA: TcxGridDBColumn
            Caption = 'Comp'
            DataBinding.FieldName = 'COMPUSA'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object viewNoteCONTD: TcxGridDBColumn
            Caption = 'ContD'
            DataBinding.FieldName = 'CONTD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 121
          end
          object viewNoteCONTC: TcxGridDBColumn
            Caption = 'ContC'
            DataBinding.FieldName = 'CONTC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 121
          end
          object viewNoteC_O: TcxGridDBColumn
            Caption = 'Operator'
            DataBinding.FieldName = 'C_O'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 32
          end
          object viewNoteDATA_OPERARE: TcxGridDBColumn
            Caption = 'Data Operare'
            DataBinding.FieldName = 'DATA_OPERARE'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 45
          end
          object viewNoteID_INITIAL: TcxGridDBColumn
            Caption = 'Id Initial'
            DataBinding.FieldName = 'ID_INITIAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object viewNoteID_PARINTE: TcxGridDBColumn
            Caption = 'Parinte'
            DataBinding.FieldName = 'ID_PARINTE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object viewNoteSTARE: TcxGridDBColumn
            Caption = 'Stare'
            DataBinding.FieldName = 'STARE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object viewNoteCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Clas. Func.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 47
          end
          object viewNoteCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Clas. Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 29
          end
          object viewNoteDATA_OP: TcxGridDBColumn
            Caption = 'Data Doc'
            DataBinding.FieldName = 'DATA_OP'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Width = 41
          end
          object viewNoteNR_OP: TcxGridDBColumn
            Caption = 'Nr Unic'
            DataBinding.FieldName = 'NR_OP'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 27
          end
        end
        object nivelNote: TcxGridLevel
          GridView = viewNote
        end
      end
    end
    object Panel1: TPanel
      Left = 10
      Top = 496
      Width = 854
      Height = 84
      Align = alBottom
      TabOrder = 1
      DesignSize = (
        854
        84)
      object btnCancel: TcxButton
        Left = 766
        Top = 6
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Cancel'
        ModalResult = 2
        TabOrder = 0
      end
      object BtnOk: TcxButton
        Left = 685
        Top = 6
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Ok'
        TabOrder = 1
        OnClick = BtnOkClick
      end
      object BtnSalvare: TcxButton
        Tag = 1
        Left = 604
        Top = 6
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Salvare'
        TabOrder = 2
        OnClick = BtnOkClick
      end
    end
    object GroupBox2: TGroupBox
      Left = 10
      Top = 193
      Width = 854
      Height = 303
      Align = alClient
      Caption = 'Pozitii Ordin de Plata'
      TabOrder = 2
      ExplicitHeight = 357
      DesignSize = (
        854
        303)
      object Label1: TLabel
        Left = 8
        Top = 20
        Width = 68
        Height = 13
        Caption = 'Cont de extras'
      end
      object Label2: TLabel
        Left = 213
        Top = 20
        Width = 61
        Height = 13
        Caption = 'Data Extras: '
      end
      object Label3: TLabel
        Left = 8
        Top = 44
        Width = 50
        Height = 13
        Caption = 'Cont Debit'
      end
      object Label4: TLabel
        Left = 193
        Top = 44
        Width = 58
        Height = 13
        Caption = 'Cont Credit :'
      end
      object Label5: TLabel
        Left = 375
        Top = 20
        Width = 37
        Height = 13
        Caption = 'Jurnal : '
      end
      object edNrOrdin: TcxImageComboBox
        Left = 80
        Top = 16
        Properties.Items = <>
        Properties.OnChange = edNrOrdinChange
        TabOrder = 0
        Width = 129
      end
      object edDataOrdin: TdxDateEdit
        Left = 279
        Top = 16
        Width = 89
        TabOrder = 1
        OnChange = edNrOrdinChange
        OnValidate = edDataOrdinValidate
        Date = -700000.000000000000000000
        SaveTime = False
        UseEditMask = True
        StoredValues = 4
      end
      object edJurnal: TcxImageComboBox
        Left = 424
        Top = 16
        Anchors = [akLeft, akTop, akRight]
        Properties.Items = <>
        TabOrder = 2
        Width = 754
      end
      object edContDebit: TdxPopupEdit
        Left = 64
        Top = 40
        Width = 121
        TabOrder = 3
        HideEditCursor = True
        PopupControl = TreeDebit
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = edContDebitCloseUp
        OnPopup = edContDebitPopup
      end
      object edContCredit: TdxPopupEdit
        Left = 256
        Top = 40
        Width = 121
        TabOrder = 4
        HideEditCursor = True
        PopupControl = TreeCredit
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = edContCreditCloseUp
        OnPopup = edContCreditPopup
      end
      object edGrupGridOp: TcxImageComboBox
        Left = 631
        Top = 40
        Anchors = [akLeft, akTop, akRight]
        Constraints.MaxWidth = 200
        EditValue = '0'
        Properties.Items = <
          item
            Description = 'Negrupat'
            ImageIndex = 0
            Value = 0
          end
          item
            Description = 'Grupat Functional'
            Value = 1
          end
          item
            Description = 'Grupat Economic'
            Value = 2
          end>
        Properties.OnChange = edGrupGridOpChange
        TabOrder = 5
        Width = 200
      end
      object BtnAdaugaFactura: TcxButton
        Left = 392
        Top = 40
        Width = 73
        Height = 22
        Caption = 'Adauga'
        TabOrder = 6
        OnClick = BtnAdaugaFacturaClick
      end
      object BtnRemoveFactura: TcxButton
        Left = 472
        Top = 40
        Width = 73
        Height = 22
        Caption = 'Sterge'
        TabOrder = 7
        OnClick = BtnRemoveFacturaClick
      end
      object BtnAnulareOP: TcxButton
        Left = 552
        Top = 40
        Width = 73
        Height = 22
        Caption = 'Anulare OP'
        TabOrder = 8
        OnClick = BtnAnulareOPClick
      end
      object gridOrdine: TcxGrid
        Left = 2
        Top = 69
        Width = 850
        Height = 232
        Align = alBottom
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 9
        LookAndFeel.Kind = lfFlat
        ExplicitHeight = 286
        object viewOrdine: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedRecordChanged = viewOrdineFocusedRecordChanged
          DataController.DataSource = DTOp
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'RecId'
          DataController.Summary.DefaultGroupSummaryItems = <
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              Position = spFooter
              Column = viewOrdineVALOARE
            end
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              Column = viewOrdineVALOARE
            end>
          DataController.Summary.FooterSummaryItems = <
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              FieldName = 'VALOARE'
              Column = viewOrdineVALOARE
            end>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsData.Appending = True
          OptionsData.DeletingConfirmation = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsSelection.InvertSelect = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          Styles.Content = cxStyle1
          Styles.Footer = cxStyle3
          Styles.Header = cxStyle2
          Styles.Indicator = cxStyle2
          Styles.Preview = cxStyle4
          object viewOrdineNRDOC: TcxGridDBColumn
            Caption = 'Nr. Doc'
            DataBinding.FieldName = 'NRDOC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 62
          end
          object viewOrdineTIP_DOCUMENT: TcxGridDBColumn
            Caption = 'Tip Docum'
            DataBinding.FieldName = 'TIP_DOCUMENT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'OP Plata'
                ImageIndex = 0
                Value = '1'
              end
              item
                Description = 'FV'
                ImageIndex = 1
                Value = '2'
              end
              item
                Description = 'CEC'
                ImageIndex = 2
                Value = '3'
              end
              item
                Description = 'OP Retur'
                ImageIndex = 3
                Value = '4'
              end
              item
                Description = 'MP'
                ImageIndex = 4
                Value = '5'
              end
              item
                Description = 'MP Retur'
                ImageIndex = 5
                Value = '6'
              end
              item
                Description = 'Alt Doc'
                ImageIndex = 6
                Value = '7'
              end>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Editing = False
            Width = 100
          end
          object viewOrdineDATA: TcxGridDBColumn
            Caption = 'Data Nota'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikMask
            Properties.SaveTime = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 79
          end
          object viewOrdineEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.CharCase = ecLowerCase
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 77
          end
          object viewOrdineVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 RON;-,0.00 RON'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 44
          end
          object viewOrdineCONTD: TcxGridDBColumn
            Caption = 'Debit'
            DataBinding.FieldName = 'CONTD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 50
          end
          object viewOrdineREPARTITOR_DEBIT: TcxGridDBColumn
            Caption = 'Repartitor Debit'
            DataBinding.FieldName = 'REPARTITOR_DEBIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Editing = False
            Width = 20
          end
          object viewOrdineCONTC: TcxGridDBColumn
            Caption = 'Credit'
            DataBinding.FieldName = 'CONTC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 50
          end
          object viewOrdineREPARTITOR_CREDIT: TcxGridDBColumn
            Caption = 'Repartitor Credit'
            DataBinding.FieldName = 'REPARTITOR_CREDIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 20
          end
          object viewOrdineCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Cod Funct.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxPopupEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.PopupControl = TreeFunctional
            Properties.PopupSysPanelStyle = True
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 50
          end
          object viewOrdineCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Cod Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxPopupEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.PopupControl = TreeEconomic
            Properties.PopupSysPanelStyle = True
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 50
          end
          object viewOrdineNR_OP: TcxGridDBColumn
            Caption = 'Nr. Unic'
            DataBinding.FieldName = 'NR_OP'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 20
          end
          object viewOrdineDATA_OP: TcxGridDBColumn
            Caption = 'Data Fact.'
            DataBinding.FieldName = 'DATA_OP'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 29
          end
          object viewOrdineTIP_NOTA: TcxGridDBColumn
            Caption = 'Tip Nota'
            DataBinding.FieldName = 'TIP_NOTA'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 46
          end
        end
        object nivelOrdine: TcxGridLevel
          GridView = viewOrdine
        end
      end
    end
  end
  object TreeFunctional: TdxDBTreeList
    Left = 145
    Top = 344
    Width = 329
    Height = 169
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    ParentField = 'ID_PARINTE'
    TabOrder = 1
    Visible = False
    OnDblClick = TreeCreditDblClick
    OnKeyDown = TreeDebitKeyDown
    DataSource = frmData.DTBGPlanFunctional
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand]
    OptionsDB = [etoAutoCalcKeyValue, etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeFunctionalDESCRIERE: TdxDBTreeListMaskColumn
      Caption = 'Buget'
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 70
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUMAR_RAND'
      OnGetText = TreeFunctionalDESCRIEREGetText
    end
    object TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      HeaderAlignment = taCenter
      Visible = False
      Width = 178
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_BUGET'
    end
  end
  object TreeEconomic: TdxDBTreeList
    Left = 402
    Top = 311
    Width = 329
    Height = 169
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_ECONOMIC'
    ParentField = 'ID_PARINTE'
    TabOrder = 2
    Visible = False
    OnDblClick = TreeCreditDblClick
    OnKeyDown = TreeDebitKeyDown
    DataSource = frmData.DTBGPlanEconomic
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand]
    OptionsDB = [etoAutoCalcKeyValue, etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeEconomicDESCRIERE: TdxDBTreeListMaskColumn
      Caption = 'Buget'
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 70
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUMAR_RAND'
      OnGetText = TreeEconomicDESCRIEREGetText
    end
    object TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      HeaderAlignment = taCenter
      Visible = False
      Width = 178
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_BUGET'
    end
  end
  object TblNota: TdxMemData
    Indexes = <>
    SortOptions = []
    AfterOpen = TblNotaAfterOpen
    AfterPost = TblNotaAfterPost
    BeforeDelete = TblNotaBeforeDelete
    OnNewRecord = TblNotaNewRecord
    Left = 34
    Top = 106
  end
  object DTOp: TDataSource
    DataSet = TblNota
    Left = 34
    Top = 138
  end
  object QryFacturi: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM CNOTE'
      ''
      'ORDER BY NR')
    Params = <>
    Left = 122
    Top = 82
  end
  object DTFacturi: TDataSource
    DataSet = QryFacturi
    Left = 90
    Top = 82
  end
  object QryDebit: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT CONT, ROMANA, PARINTE FROM CPLAN '
      'WHERE CONT LIKE :START_DEBIT'
      'ORDER BY PARINTE, CONT')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'START_DEBIT'
        ParamType = ptUnknown
        Size = 50
      end>
    Left = 122
    Top = 114
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'START_DEBIT'
        ParamType = ptUnknown
        Size = 50
      end>
  end
  object QryCredit: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT CONT, ROMANA, PARINTE FROM CPLAN '
      'WHERE CONT LIKE :START_CREDIT'
      'ORDER BY PARINTE, CONT')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'START_CREDIT'
        ParamType = ptUnknown
        Size = 50
      end>
    Left = 122
    Top = 146
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'START_CREDIT'
        ParamType = ptUnknown
        Size = 50
      end>
  end
  object DTDebit: TDataSource
    DataSet = QryDebit
    Left = 90
    Top = 114
  end
  object DTCredit: TDataSource
    DataSet = QryCredit
    Left = 90
    Top = 146
  end
  object DTOphturi: TDataSource
    DataSet = QryOPH
    Left = 90
    Top = 178
  end
  object QryOPH: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM CNOTE'
      ''
      'ORDER BY NR')
    Params = <>
    Left = 122
    Top = 178
  end
  object cxStyleRepository1: TcxStyleRepository
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWindow
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle2: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBtnFace
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle3: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBtnFace
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle4: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWindow
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
  end
  object cxStyleRepository2: TcxStyleRepository
    PixelsPerInch = 96
    object cxStyle5: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWindow
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle6: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBtnFace
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle7: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBtnFace
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
    object cxStyle8: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWindow
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
  end
end
