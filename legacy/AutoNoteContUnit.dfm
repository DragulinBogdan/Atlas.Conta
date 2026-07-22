object frmAutoNoteCont: TfrmAutoNoteCont
  Left = 205
  Top = 183
  Caption = 'Generare automata nota contabila'
  ClientHeight = 392
  ClientWidth = 574
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Wizard: TdxWizardControl
    Left = 0
    Top = 0
    Width = 574
    Height = 392
    Buttons.Back.Caption = '&Inapoi'
    Buttons.Cancel.Caption = '&Abandon'
    Buttons.CustomButtons.Buttons = <>
    Buttons.Help.Visible = False
    Buttons.Next.Caption = '&Urmatorul'
    InfoPanel.Caption = 'Import automat note contabile'
    OnButtonClick = WizardButtonClick
    object Inceput: TdxWizardControlPage
      Header.Description = 'ATENTIE : Automat programul va storna notele preluate anterior.'
      Header.Title = 'Bine ati venit la vrajitorul de import de note'
      object Label6: TLabel
        Left = 0
        Top = 0
        Width = 552
        Height = 256
        Align = alClient
        AutoSize = False
        Caption = 
          '      Prin intermediul acestui vrajitor puteti genera automat no' +
          'te contabile din documentele sau operatiile efectuatein modulele' +
          ' conexe ale aplicatiei ATLAS.'#13#10'      Notele contabile se vor gen' +
          'era conform regulilor definite prin intermediul aplicatiei.'#13#10'   ' +
          '   Pentru a modifica regulile care stau la baza importului va ru' +
          'gam folositi partea de configurare a exportului corespunzatoare ' +
          'fiecarui modul in parte.'
        Layout = tlCenter
        WordWrap = True
        ExplicitLeft = 8
        ExplicitTop = 128
        ExplicitWidth = 393
        ExplicitHeight = 81
      end
    end
    object SelectieModul: TdxWizardControlPage
      Header.Description = 
        'ATENTIE : Notele preluate anterior pentru perioada selectata vor' +
        ' fi anulate !'
      Header.Title = 'Selectie Module'
      object chkAll: TcxCheckBox
        Left = -1
        Top = 3
        Caption = 'Import Complet (x)/ Pe Module ()'
        ParentFont = False
        TabOrder = 0
        OnClick = chkAllClick
      end
      object BifeScroll: TScrollBox
        Left = 0
        Top = 96
        Width = 552
        Height = 160
        Align = alBottom
        Anchors = [akLeft, akTop, akRight, akBottom]
        Ctl3D = False
        ParentCtl3D = False
        TabOrder = 1
      end
    end
    object SelectiePerioada: TdxWizardControlPage
      object Label10: TLabel
        Left = 72
        Top = 184
        Width = 272
        Height = 13
        Caption = 'Se vor procesa notele cu data mai mare decat specificata'
      end
      object Label11: TLabel
        Left = 80
        Top = 208
        Width = 67
        Height = 13
        Caption = 'Data minima : '
      end
      object Label12: TLabel
        Left = 80
        Top = 264
        Width = 70
        Height = 13
        Caption = 'Data maxima : '
      end
      object Label13: TLabel
        Left = 72
        Top = 240
        Width = 271
        Height = 13
        Caption = 'Se vor procesa notele cu data mai mica decat specificata'
      end
      object Label15: TLabel
        Left = 72
        Top = 120
        Width = 230
        Height = 13
        Caption = 'Se vor procesa documentele din luna specificata'
      end
      object Label17: TLabel
        Left = 224
        Top = 141
        Width = 33
        Height = 13
        Caption = 'Luna : '
      end
      object Label14: TLabel
        Left = 80
        Top = 141
        Width = 27
        Height = 13
        Caption = 'Anul :'
      end
      object edDataMinima: TdxDateEdit
        Left = 152
        Top = 205
        Width = 121
        TabOrder = 0
        Date = -700000.000000000000000000
        UseEditMask = True
        StoredValues = 4
      end
      object edDataMaxima: TdxDateEdit
        Left = 152
        Top = 261
        Width = 121
        TabOrder = 1
        Date = -700000.000000000000000000
        UseEditMask = True
        StoredValues = 4
      end
      object edAnul: TdxMRUEdit
        Left = 112
        Top = 138
        Width = 105
        TabOrder = 2
        OnChange = edAnulChange
      end
      object edLuna: TdxPickEdit
        Left = 256
        Top = 138
        Width = 121
        TabOrder = 3
        OnChange = edAnulChange
        DropDownListStyle = True
      end
    end
    object NoteStornate: TdxWizardControlPage
      Header.Description = 'Pagina contine notele care urmeaza a fi stornate'
      Header.Title = 'Note Stornate'
      object gridNoteStornate: TcxGrid
        Left = 0
        Top = 0
        Width = 552
        Height = 256
        Align = alClient
        TabOrder = 0
        LookAndFeel.Kind = lfFlat
        object viewNoteStornate: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          DataController.DataSource = dtNoteStornate
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'RecId'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsBehavior.ImmediateEditor = False
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewNoteStornateJURNAL: TcxGridDBColumn
            Caption = 'Jurnal'
            DataBinding.FieldName = 'JURNAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 34
          end
          object viewNoteStornateNRDOC: TcxGridDBColumn
            Caption = 'Nr. Nota'
            DataBinding.FieldName = 'NRDOC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 53
          end
          object viewNoteStornateDATA: TcxGridDBColumn
            Caption = 'Data Nota'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            SortIndex = 0
            SortOrder = soAscending
            Width = 82
          end
          object viewNoteStornateCONT_DEBT: TcxGridDBColumn
            Caption = 'Cont Deb'
            DataBinding.FieldName = 'CONT_DEBT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 51
          end
          object viewNoteStornateVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 lei;-,0.00 lei'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 59
          end
          object viewNoteStornateCONT_CRED: TcxGridDBColumn
            Caption = 'Cont Cred'
            DataBinding.FieldName = 'CONT_CRED'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 70
          end
          object viewNoteStornateEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 68
          end
          object viewNoteStornateNUME_DEBIT: TcxGridDBColumn
            Caption = 'Repartitor Debit'
            DataBinding.FieldName = 'NUME_DEBIT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 69
          end
          object viewNoteStornateNUME_CREDIT: TcxGridDBColumn
            Caption = 'Repartitor Credit'
            DataBinding.FieldName = 'NUME_CREDIT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 70
          end
        end
        object nivelNoteStornate: TcxGridLevel
          GridView = viewNoteStornate
        end
      end
    end
    object NoteGenerate: TdxWizardControlPage
      Header.Description = 'Pagina contine notele care se vor genera'
      Header.Title = 'Note Generate'
      object gridNoteGenerate: TcxGrid
        Left = 0
        Top = 0
        Width = 552
        Height = 256
        Align = alClient
        TabOrder = 0
        LookAndFeel.Kind = lfFlat
        object viewNoteGenerate: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          DataController.DataSource = dtNoteGenerate
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'RecId'
          DataController.Summary.DefaultGroupSummaryItems.Separator = ', '
          DataController.Summary.DefaultGroupSummaryItems = <
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              Position = spFooter
              FieldName = 'VALOARE'
              Column = viewNoteGenerateVALOARE
            end>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsBehavior.ImmediateEditor = False
          OptionsData.Editing = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewNoteGenerateDATA: TcxGridDBColumn
            Caption = 'Data Nota'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 72
          end
          object viewNoteGenerateNUME_REPARTITOR: TcxGridDBColumn
            Caption = 'Repartitor'
            DataBinding.FieldName = 'NUME_REPARTITOR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 108
          end
          object viewNoteGenerateCONT_DEBT: TcxGridDBColumn
            Caption = 'Cont Deb.'
            DataBinding.FieldName = 'CONT_DEBT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 71
          end
          object viewNoteGenerateVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 lei;-,0.00 lei'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 59
          end
          object viewNoteGenerateCONT_CRED: TcxGridDBColumn
            Caption = 'Cont Cred.'
            DataBinding.FieldName = 'CONT_CRED'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 73
          end
          object viewNoteGenerateDOCUMENT: TcxGridDBColumn
            Caption = 'Document'
            DataBinding.FieldName = 'DOCUMENT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 68
          end
          object viewNoteGeneratePOZITIE: TcxGridDBColumn
            Caption = 'Pozitie'
            DataBinding.FieldName = 'POZITIE'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 37
          end
          object viewNoteGenerateMODUL: TcxGridDBColumn
            Caption = 'Modul'
            DataBinding.FieldName = 'MODUL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Tranzactii'
                ImageIndex = 0
                Value = '1'
              end
              item
                Description = 'Casa / Banca'
                ImageIndex = 1
                Value = '2'
              end
              item
                Description = 'Mijloace Fixe'
                ImageIndex = 2
                Value = '4'
              end
              item
                Description = 'Salarizare'
                ImageIndex = 3
                Value = '8'
              end>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 100
          end
          object viewNoteGenerateCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Clasa Func.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteGenerateCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Clasa Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
        end
        object nivelNoteGenerate: TcxGridLevel
          GridView = viewNoteGenerate
        end
      end
    end
    object NoteEronate: TdxWizardControlPage
      Header.Description = 'Pagina contine notele generate cu eroare'
      Header.Title = 'Lista note eronate'
      object gridNoteEronate: TcxGrid
        Left = 0
        Top = 0
        Width = 552
        Height = 256
        Align = alClient
        TabOrder = 0
        LookAndFeel.Kind = lfFlat
        object viewNoteEronate: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          DataController.DataSource = dtNoteEronate
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'RecId'
          DataController.Summary.DefaultGroupSummaryItems.Separator = ', '
          DataController.Summary.DefaultGroupSummaryItems = <
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              FieldName = 'VALOARE'
            end>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsBehavior.ImmediateEditor = False
          OptionsData.Editing = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewNoteEronateNR_DOCUM: TcxGridDBColumn
            Caption = 'Nr Docum'
            DataBinding.FieldName = 'NR_DOCUM'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronateTIP_EROARE: TcxGridDBColumn
            Caption = 'Tip Eroare'
            DataBinding.FieldName = 'TIP_EROARE'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Credit inexistent'
                ImageIndex = 0
                Value = '1'
              end
              item
                Description = 'Debit inexistent'
                ImageIndex = 1
                Value = '2'
              end
              item
                Description = 'Credit invalid'
                ImageIndex = 2
                Value = '3'
              end
              item
                Description = 'Debit invalid'
                ImageIndex = 3
                Value = '4'
              end
              item
                Description = 'Credit sintetic'
                ImageIndex = 4
                Value = '5'
              end
              item
                Description = 'Debit sintetic'
                ImageIndex = 5
                Value = '6'
              end>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 100
          end
          object viewNoteEronateID_DOCUMENT: TcxGridDBColumn
            Caption = 'Id Docum'
            DataBinding.FieldName = 'ID_DOCUMENT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 60
          end
          object viewNoteEronateDATA: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 80
          end
          object viewNoteEronateNUME_REPARTITOR: TcxGridDBColumn
            Caption = 'Nume Rep'
            DataBinding.FieldName = 'NUME_REPARTITOR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronateDOCUMENT: TcxGridDBColumn
            Caption = 'Docum'
            DataBinding.FieldName = 'DOCUMENT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronatePOZITIE: TcxGridDBColumn
            Caption = 'Poz'
            DataBinding.FieldName = 'POZITIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronateEXPLICATIE: TcxGridDBColumn
            Caption = 'Explic'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 200
          end
          object viewNoteEronateVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 lei;-,0.00 lei'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronateCONT_CRED: TcxGridDBColumn
            Caption = 'Credit'
            DataBinding.FieldName = 'CONT_CRED'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 80
          end
          object viewNoteEronateCONT_DEBT: TcxGridDBColumn
            Caption = 'Debit'
            DataBinding.FieldName = 'CONT_DEBT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 80
          end
          object viewNoteEronateMODUL: TcxGridDBColumn
            Caption = 'Modul'
            DataBinding.FieldName = 'MODUL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Tranzactii'
                ImageIndex = 0
                Value = '1'
              end
              item
                Description = 'Casa/Banca'
                ImageIndex = 1
                Value = '2'
              end
              item
                Description = 'MiFix'
                ImageIndex = 2
                Value = '4'
              end
              item
                Description = 'Salarii'
                ImageIndex = 3
                Value = '8'
              end>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 100
          end
          object viewNoteEronateCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Clasa Func.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 100
          end
          object viewNoteEronateCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Clasa Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Width = 100
          end
        end
        object nivelNoteEronate: TcxGridLevel
          GridView = viewNoteEronate
        end
      end
    end
    object Finalizare: TdxWizardControlPage
      object LbCaption: TLabel
        Left = 16
        Top = 280
        Width = 88
        Height = 13
        Caption = 'Operatie curenta : '
      end
      object LbTotal: TLabel
        Left = 16
        Top = 312
        Width = 59
        Height = 13
        Caption = 'Progres total'
      end
      object LbCurentProgress: TLabel
        Left = 104
        Top = 248
        Width = 3
        Height = 13
      end
      object FinishInfo: TdxMemo
        Left = 16
        Top = 80
        Width = 383
        TabOrder = 0
        SelectionBar = False
        Height = 193
      end
      object LocalProgress: TcxProgressBar
        Left = 24
        Top = 296
        Properties.AssignedValues.Min = True
        Properties.Max = 100.000000000000000000
        TabOrder = 1
        Width = 377
      end
      object GlobalProgress: TcxProgressBar
        Left = 24
        Top = 328
        Properties.AssignedValues.Min = True
        Properties.Max = 100.000000000000000000
        TabOrder = 2
        Width = 377
      end
    end
  end
  object dtNoteStornate: TDataSource
    Left = 121
    Top = 352
  end
  object dtNoteGenerate: TDataSource
    Left = 25
    Top = 352
  end
  object dtNoteEronate: TDataSource
    Left = 73
    Top = 352
  end
end
