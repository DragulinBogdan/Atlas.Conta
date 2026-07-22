object frmModificDocument: TfrmModificDocument
  Left = 320
  Top = 90
  Caption = 'Modificare Document'
  ClientHeight = 624
  ClientWidth = 849
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PopupMenu = ppCopiereMenu
  Position = poScreenCenter
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  DesignSize = (
    849
    624)
  PixelsPerInch = 96
  TextHeight = 13
  object Bevel1: TBevel
    Left = 112
    Top = 8
    Width = 724
    Height = 3
    Anchors = [akLeft, akTop, akRight]
    Shape = bsTopLine
    ExplicitWidth = 637
  end
  object Label1: TcxLabel
    Left = 22
    Top = 22
    Caption = 'Cod - Denumire :'
    FocusControl = edCod
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object Label2: TcxLabel
    Left = 11
    Top = 2
    Caption = 'Detalii Document '
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object Label10: TcxLabel
    Left = 22
    Top = 49
    Caption = 'Predator - Primitor :'
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object tabDefaDoc: TcxTabControl
    Left = 8
    Top = 128
    Width = 823
    Height = 21
    Anchors = [akLeft, akTop, akRight]
    TabOrder = 4
    Properties.CustomButtons.Buttons = <>
    Properties.Images = Enabled
    Properties.TabIndex = 0
    Properties.Tabs.Strings = (
      'Pred. in. <-> Prim. in.'
      'Pred. in. <-> Prim. ext.'
      'Pred. ext. <-> Prim. in.'
      'Pred. ext. <-> Prim. ext.')
    OnChange = tabDefaDocChange
    OnChanging = tabDefaDocChanging
    OnGetImageIndex = tabDefaDocGetImageIndex
    ClientRectBottom = 23
    ClientRectRight = 823
    ClientRectTop = 23
  end
  object PageDescDocum: TcxPageControl
    Left = 8
    Top = 152
    Width = 824
    Height = 424
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 6
    Properties.ActivePage = tbDescDocum
    Properties.CustomButtons.Buttons = <>
    ClientRectBottom = 424
    ClientRectRight = 824
    ClientRectTop = 24
    object tbDescDocum: TcxTabSheet
      Caption = 'Descriere mod tratare document'
      object pnDescClient: TPanel
        Left = 0
        Top = 0
        Width = 824
        Height = 400
        Align = alClient
        BevelInner = bvLowered
        TabOrder = 0
        object Splitter3: TcxSplitter
          Left = 544
          Top = 2
          Width = 8
          Height = 396
          HotZoneClassName = 'TcxSimpleStyle'
          AlignSplitter = salRight
          Control = inspTemplate
        end
        object grDetaliiDoc: TcxGroupBox
          Left = 2
          Top = 2
          Align = alClient
          TabOrder = 0
          DesignSize = (
            542
            396)
          Height = 396
          Width = 542
          object BtnModifyStockPredator: TcxButton
            Left = 353
            Top = 26
            Width = 23
            Height = 22
            Hint = 'Modifica tipul stocului'
            Anchors = [akTop, akRight]
            Caption = '...'
            TabOrder = 5
            OnClick = BtnModifyStockPredatorClick
          end
          object BtnModifyStockPrimitor: TcxButton
            Left = 353
            Top = 68
            Width = 23
            Height = 22
            Hint = 'Modifica tipul stocului'
            Anchors = [akTop, akRight]
            Caption = '...'
            TabOrder = 7
            OnClick = BtnModifyStockPrimitorClick
          end
          object BtnAdaugaColoana: TcxButton
            Left = 384
            Top = 26
            Width = 145
            Height = 22
            Anchors = [akTop, akRight]
            Caption = 'Adauga Coloana Noua'
            TabOrder = 8
            OnClick = BtnAdaugaColoanaClick
          end
          object Label14: TcxLabel
            Left = 386
            Top = 54
            Anchors = [akTop, akRight]
            Caption = 'Lista campuri pentru :'
          end
          object chkStockPredator: TcxCheckBox
            Left = 8
            Top = 6
            Caption = 'Tip de stock la predator'
            ParentFont = False
            State = cbsChecked
            TabOrder = 0
            Transparent = True
          end
          object edTipStockPred: TcxDBImageComboBox
            Left = 8
            Top = 27
            Anchors = [akLeft, akTop, akRight]
            DataBinding.DataField = 'ID_GEST_TIP_STOC_PREDATOR'
            DataBinding.DataSource = frmData.DTDefaDoc
            Properties.Items = <>
            TabOrder = 1
            Width = 340
          end
          object chkStockPrimitor: TcxCheckBox
            Left = 8
            Top = 48
            Caption = 'Tip de stock la primitor'
            ParentFont = False
            TabOrder = 2
            Transparent = True
          end
          object edTipStockPrim: TcxDBImageComboBox
            Left = 8
            Top = 69
            Anchors = [akLeft, akTop, akRight]
            DataBinding.DataField = 'ID_GEST_TIP_STOC_PRIMITOR'
            DataBinding.DataSource = frmData.DTDefaDoc
            Properties.Items = <>
            TabOrder = 3
            Width = 340
          end
          object edTipListaCampuri: TcxImageComboBox
            Left = 384
            Top = 70
            Anchors = [akTop, akRight]
            EditValue = 0
            Properties.Items = <
              item
                Description = 'Pozitii Document'
                ImageIndex = 0
                Value = 0
              end
              item
                Description = 'Document'
                Value = 1
              end>
            Properties.OnChange = edTipListaCampuriChange
            TabOrder = 4
            Width = 145
          end
          object gridTemplate: TcxGrid
            Left = 6
            Top = 97
            Width = 523
            Height = 288
            Anchors = [akLeft, akTop, akRight, akBottom]
            TabOrder = 6
            LookAndFeel.Kind = lfFlat
            object viewTemplate: TcxGridDBTableView
              Navigator.Buttons.CustomButtons = <>
              ScrollbarAnnotations.CustomAnnotations = <>
              DataController.DataSource = DTTemplateCrid
              DataController.Filter.MaxValueListCount = 1000
              DataController.Filter.Active = True
              DataController.KeyFieldNames = 'ID_GEST_DEFA_DOCUM_ITEMSI'
              DataController.Summary.DefaultGroupSummaryItems = <>
              DataController.Summary.FooterSummaryItems = <>
              DataController.Summary.SummaryGroups = <>
              Filtering.ColumnPopup.MaxDropDownItemCount = 12
              OptionsBehavior.FocusCellOnTab = True
              OptionsBehavior.IncSearch = True
              OptionsData.CancelOnExit = False
              OptionsData.Deleting = False
              OptionsData.DeletingConfirmation = False
              OptionsSelection.HideFocusRectOnExit = False
              OptionsView.ColumnAutoWidth = True
              OptionsView.GroupByBox = False
              OptionsView.GroupFooters = gfVisibleWhenExpanded
              OptionsView.Indicator = True
              Preview.Column = viewTemplateFORMULA_CALCUL
              Preview.MaxLineCount = 2
              Preview.Visible = True
              object viewTemplateCLASS_NAME: TcxGridDBColumn
                Caption = 'Mod Editare'
                DataBinding.FieldName = 'CLASS_NAME'
                PropertiesClassName = 'TcxComboBoxProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.DropDownRows = 7
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 100
              end
              object viewTemplatePOS: TcxGridDBColumn
                Caption = 'Pos'
                DataBinding.FieldName = 'POS'
                PropertiesClassName = 'TcxSpinEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.AssignedValues.MaxValue = True
                Properties.AssignedValues.MinValue = True
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                SortIndex = 0
                SortOrder = soAscending
                Width = 51
              end
              object viewTemplateFIELD_NAME: TcxGridDBColumn
                Caption = 'Nume Camp'
                DataBinding.FieldName = 'FIELD_NAME'
                PropertiesClassName = 'TcxComboBoxProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.DropDownRows = 7
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 85
              end
              object viewTemplateCAPTION: TcxGridDBColumn
                Caption = 'Captura'
                DataBinding.FieldName = 'CAPTION'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 106
              end
              object viewTemplateMIN_WIDTH: TcxGridDBColumn
                Caption = 'Min Latime'
                DataBinding.FieldName = 'MIN_WIDTH'
                PropertiesClassName = 'TcxSpinEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.AssignedValues.MaxValue = True
                Properties.AssignedValues.MinValue = True
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 55
              end
              object viewTemplateMAX_WIDTH: TcxGridDBColumn
                Caption = 'Max Latime'
                DataBinding.FieldName = 'MAX_WIDTH'
                PropertiesClassName = 'TcxSpinEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.AssignedValues.MaxValue = True
                Properties.AssignedValues.MinValue = True
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 55
              end
              object viewTemplateCAPTION_ALIGN: TcxGridDBColumn
                Caption = 'Asezare capt.'
                DataBinding.FieldName = 'CAPTION_ALIGN'
                PropertiesClassName = 'TcxImageComboBoxProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.DropDownRows = 7
                Properties.Items = <
                  item
                    Description = 'Aliniat la stanga'
                    Value = '0'
                  end
                  item
                    Description = 'Aliniat la dreapta'
                    Value = '1'
                  end
                  item
                    Description = 'Centrat'
                    Value = '2'
                  end>
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                MinWidth = 16
                Width = 91
              end
              object viewTemplateALIGN: TcxGridDBColumn
                Caption = 'Asezare'
                DataBinding.FieldName = 'ALIGN'
                PropertiesClassName = 'TcxImageComboBoxProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.DropDownRows = 7
                Properties.Items = <>
                Properties.ReadOnly = True
                Properties.ShowDescriptions = False
                Visible = False
                HeaderAlignmentHorz = taCenter
                MinWidth = 16
                Options.Editing = False
                Width = 62
              end
              object viewTemplateCOLOR: TcxGridDBColumn
                Caption = 'Culoare'
                DataBinding.FieldName = 'COLOR'
                PropertiesClassName = 'TcxButtonEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Buttons = <
                  item
                    Default = True
                    Kind = bkEllipsis
                  end>
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 71
              end
              object viewTemplateFONT_NAME: TcxGridDBColumn
                Caption = 'Nume Font'
                DataBinding.FieldName = 'FONT_NAME'
                PropertiesClassName = 'TcxComboBoxProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.DropDownRows = 7
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 85
              end
              object viewTemplateFONT_COLOR: TcxGridDBColumn
                Caption = 'Culoare Font'
                DataBinding.FieldName = 'FONT_COLOR'
                PropertiesClassName = 'TcxButtonEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Buttons = <
                  item
                    Default = True
                    Kind = bkEllipsis
                  end>
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 65
              end
              object viewTemplateEDIT_MASK: TcxGridDBColumn
                Caption = 'Masca editare'
                DataBinding.FieldName = 'EDIT_MASK'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 68
              end
              object viewTemplateFONT_SIZE: TcxGridDBColumn
                Caption = 'Marime Font'
                DataBinding.FieldName = 'FONT_SIZE'
                PropertiesClassName = 'TcxSpinEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.AssignedValues.MaxValue = True
                Properties.AssignedValues.MinValue = True
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Options.Editing = False
                Width = 55
              end
              object viewTemplateVISIBLE: TcxGridDBColumn
                Caption = 'Visible'
                DataBinding.FieldName = 'VISIBLE'
                PropertiesClassName = 'TcxCheckBoxProperties'
                Properties.Alignment = taLeftJustify
                Properties.NullStyle = nssUnchecked
                Properties.ValueChecked = 'True'
                Properties.ValueGrayed = ''
                Properties.ValueUnchecked = 'False'
                HeaderAlignmentHorz = taCenter
                MinWidth = 16
                Width = 67
              end
              object viewTemplateFORMULA_CALCUL: TcxGridDBColumn
                Caption = 'Formula Calcul'
                DataBinding.FieldName = 'FORMULA_CALCUL'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                Width = 75
              end
            end
            object nivelTemplate: TcxGridLevel
              GridView = viewTemplate
            end
          end
        end
        object inspTemplate: TcxDBVerticalGrid
          Left = 552
          Top = 2
          Width = 270
          Height = 396
          Align = alRight
          LookAndFeel.Kind = lfFlat
          OptionsView.CellTextMaxLineCount = 3
          OptionsView.AutoScaleBands = False
          OptionsView.GridLineColor = clBtnFace
          OptionsView.RowHeaderMinWidth = 30
          OptionsView.RowHeaderWidth = 137
          OptionsView.ValueWidth = 80
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          TabOrder = 1
          DataController.DataSource = DTTemplateCrid
          Version = 1
          object inspTemplateCAPTION: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Captura'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'CAPTION'
            ID = 0
            ParentID = -1
            Index = 0
            Version = 1
          end
          object inspTemplateVISIBLE: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Visibila'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'VISIBLE'
            ID = 1
            ParentID = -1
            Index = 1
            Version = 1
          end
          object inspTemplateFONT_NAME: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Nume Font'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'FONT_NAME'
            ID = 2
            ParentID = -1
            Index = 2
            Version = 1
          end
          object inspTemplateEDIT_MASK: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Masca Editare'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'EDIT_MASK'
            ID = 3
            ParentID = -1
            Index = 3
            Version = 1
          end
          object inspTemplateCLASS_NAME: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Tip Coloana'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.DropDownRows = 7
            Properties.EditProperties.Items = <>
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ShowDescriptions = False
            Properties.DataBinding.FieldName = 'CLASS_NAME'
            ID = 4
            ParentID = -1
            Index = 4
            Version = 1
          end
          object inspTemplateFIELD_NAME: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Nume Camp'
            Properties.EditPropertiesClassName = 'TcxComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.DropDownRows = 7
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.Revertable = True
            Properties.DataBinding.FieldName = 'FIELD_NAME'
            ID = 5
            ParentID = -1
            Index = 5
            Version = 1
          end
          object inspTemplateMIN_WIDTH: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Latime Minima'
            Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.AssignedValues.MaxValue = True
            Properties.EditProperties.AssignedValues.MinValue = True
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'MIN_WIDTH'
            ID = 6
            ParentID = -1
            Index = 6
            Version = 1
          end
          object inspTemplateMAX_WIDTH: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Latime Maxima'
            Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.AssignedValues.MaxValue = True
            Properties.EditProperties.AssignedValues.MinValue = True
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'MAX_WIDTH'
            ID = 7
            ParentID = -1
            Index = 7
            Version = 1
          end
          object inspTemplateCAPTION_ALIGN: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Asezare Captura'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.DropDownRows = 7
            Properties.EditProperties.Items = <
              item
                Description = 'Aliniat la stanga'
                Value = '0'
              end
              item
                Description = 'Aliniat la dreapta'
                Value = '1'
              end
              item
                Description = 'Centrat'
                Value = '2'
              end>
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'CAPTION_ALIGN'
            ID = 8
            ParentID = -1
            Index = 8
            Version = 1
          end
          object inspTemplateALIGN: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Asezare Date'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.DropDownRows = 7
            Properties.EditProperties.Items = <
              item
                Description = 'Aliniat la stanga'
                Value = '0'
              end
              item
                Description = 'Aliniat la dreapta'
                Value = '1'
              end
              item
                Description = 'Centrat'
                Value = '2'
              end>
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'ALIGN'
            ID = 9
            ParentID = -1
            Index = 9
            Version = 1
          end
          object inspTemplatePOS: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Pozitie'
            Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.AssignedValues.MaxValue = True
            Properties.EditProperties.AssignedValues.MinValue = True
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'POS'
            ID = 10
            ParentID = -1
            Index = 10
            Version = 1
          end
          object inspTemplateCOLOR: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Culoare'
            Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Buttons = <
              item
                Default = True
                Kind = bkEllipsis
              end>
            Properties.DataBinding.FieldName = 'COLOR'
            ID = 11
            ParentID = -1
            Index = 11
            Version = 1
          end
          object inspTemplateFONT_COLOR: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Culoare Font'
            Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Buttons = <
              item
                Default = True
                Kind = bkEllipsis
              end>
            Properties.DataBinding.FieldName = 'FONT_COLOR'
            ID = 12
            ParentID = -1
            Index = 12
            Version = 1
          end
          object inspTemplateFONT_SIZE: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Marime Font'
            Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.AssignedValues.MaxValue = True
            Properties.EditProperties.AssignedValues.MinValue = True
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'FONT_SIZE'
            ID = 13
            ParentID = -1
            Index = 13
            Version = 1
          end
          object inspTemplateREQUIRED: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Obligatoriu'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'REQUIRED'
            ID = 14
            ParentID = -1
            Index = 14
            Version = 1
          end
          object inspTemplateFORMULA_CALCUL: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Formula'
            Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Buttons = <
              item
                Default = True
                Kind = bkEllipsis
              end>
            Properties.DataBinding.FieldName = 'FORMULA_CALCUL'
            ID = 15
            ParentID = -1
            Index = 15
            Version = 1
          end
          object inspTemplateREADONLY: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Decat Citire'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'READONLY'
            ID = 16
            ParentID = -1
            Index = 16
            Version = 1
          end
          object inspTemplateSUM_TOTAL: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Se Insumeaza'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'SUM_TOTAL'
            ID = 17
            ParentID = -1
            Index = 17
            Version = 1
          end
          object inspTemplatePRECEDENTA: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Precedenta'
            Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.AssignedValues.MaxValue = True
            Properties.EditProperties.AssignedValues.MinValue = True
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'PRECEDENTA'
            ID = 18
            ParentID = -1
            Index = 18
            Version = 1
          end
          object inspTemplateautoCreate: TcxDBEditorRow
            Expanded = False
            Properties.Caption = 'Creare Automata'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'autoCreate'
            ID = 19
            ParentID = -1
            Index = 19
            Version = 1
          end
        end
      end
    end
    object tabTratareCODMAT: TcxTabSheet
      Caption = 'Descriere mecanisme automate'
      ImageIndex = 1
      DesignSize = (
        824
        400)
      object BtnModifica: TcxButton
        Left = 679
        Top = 48
        Width = 65
        Height = 22
        Anchors = [akTop, akRight]
        Caption = 'Modifica'
        TabOrder = 19
        OnClick = BtnModificaClick
      end
      object BtnDelete: TcxButton
        Left = 751
        Top = 48
        Width = 65
        Height = 22
        Anchors = [akTop, akRight]
        Caption = 'Sterge'
        TabOrder = 20
        OnClick = BtnDeleteClick
      end
      object BtnAdauga: TcxButton
        Left = 607
        Top = 48
        Width = 65
        Height = 22
        Anchors = [akTop, akRight]
        Caption = 'Adauga'
        TabOrder = 21
        OnClick = BtnAdaugaClick
      end
      object BtnModifyReport: TcxButton
        Left = 256
        Top = 200
        Width = 23
        Height = 22
        Caption = '...'
        TabOrder = 22
      end
      object Label5: TcxLabel
        Left = 8
        Top = 5
        Caption = 'Document &conex :'
        FocusControl = edDocumentConex
      end
      object Label6: TcxLabel
        Left = 8
        Top = 90
        Caption = 'Mod tratare stock'
        FocusControl = edTipDescarcare
      end
      object LbReportInfo: TcxLabel
        Left = 14
        Top = 201
        Caption = 'Raport: '
      end
      object LbZileGratie: TcxLabel
        Left = 32
        Top = 234
        Caption = 'Zile de gratie pentru validare :'
        Transparent = True
      end
      object Label8: TcxLabel
        Left = 296
        Top = 5
        Caption = 'Validari necesare pentru tiparie'
      end
      object Label9: TcxLabel
        Left = 8
        Top = 45
        Caption = 'Mod definire document conex :'
        FocusControl = edDocumentConex
      end
      object edDocumentConex: TcxDBImageComboBox
        Left = 8
        Top = 22
        DataBinding.DataField = 'ID_DOCUMENT_CONEX'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.Items = <>
        TabOrder = 0
        Width = 273
      end
      object edTipDescarcare: TcxDBImageComboBox
        Left = 8
        Top = 107
        DataBinding.DataField = 'TIP_DESCARCARE'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.Items = <
          item
            Description = 'FIFO'
            ImageIndex = 0
            Value = 1
          end
          item
            Description = 'LIFO'
            Value = 2
          end
          item
            Description = 'STD'
            Value = 3
          end
          item
            Description = 'CMP instantaneu'
            Value = 4
          end
          item
            Description = 'CMP global'
            Value = 5
          end>
        TabOrder = 1
        Width = 273
      end
      object edChkNumarAuto: TcxDBCheckBox
        Left = 4
        Top = 129
        Caption = 'Generare Automat &Numere ( Prefix - Start - End)'
        DataBinding.DataField = 'NUMAR_AUTOMAT'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.ValueChecked = 'True'
        Properties.ValueUnchecked = 'False'
        TabOrder = 2
      end
      object edPrefix: TcxDBMaskEdit
        Left = 8
        Top = 149
        DataBinding.DataField = 'NUMAR_PREFIX'
        DataBinding.DataSource = frmData.DTDefaDoc
        TabOrder = 3
        Width = 53
      end
      object edNumarStart: TcxDBSpinEdit
        Left = 72
        Top = 149
        DataBinding.DataField = 'NUMAR_START'
        DataBinding.DataSource = frmData.DTDefaDoc
        TabOrder = 4
        Width = 105
      end
      object edNumarEnd: TcxDBSpinEdit
        Left = 184
        Top = 149
        DataBinding.DataField = 'NUMAR_END'
        DataBinding.DataSource = frmData.DTDefaDoc
        TabOrder = 5
        Width = 97
      end
      object edTiparireAutomata: TcxCheckBox
        Left = 8
        Top = 174
        Caption = 'Se genereaza automat raport la tiparirea documentului'
        ParentFont = False
        State = cbsChecked
        TabOrder = 6
        Transparent = True
        OnClick = edTiparireAutomataClick
      end
      object edZileValabiltiate: TcxDBSpinEdit
        Left = 184
        Top = 233
        DataBinding.DataField = 'PERIOADA_EXPIRARE'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.OnChange = edZileValabiltiateChange
        TabOrder = 7
        Width = 97
      end
      object AtsDBCheckEdit1: TcxDBCheckBox
        Left = 32
        Top = 257
        Caption = 'Se valideaza automat de emitent'
        DataBinding.DataField = 'AUTO_VALIDARE_EMITENT'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.ValueChecked = 'True'
        Properties.ValueUnchecked = 'False'
        TabOrder = 8
        Transparent = True
      end
      object edModDescarcare: TcxDBImageComboBox
        Left = 8
        Top = 62
        DataBinding.DataField = 'TIP_DESCARCARE'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.Items = <
          item
            Description = 'Generare la fel'
            ImageIndex = 0
            Value = 0
          end
          item
            Description = 'Complementeaza Gestiuni'
            Value = 1
          end>
        TabOrder = 9
        Width = 273
      end
      object edListaFunctii: TcxPopupEdit
        Left = 294
        Top = 22
        Anchors = [akLeft, akTop, akRight]
        Properties.PopupControl = TreeFunctiuni
        Properties.ReadOnly = False
        Properties.OnCloseUp = edListaFunctiiPropertiesCloseUp
        TabOrder = 10
        Width = 307
      end
      object edZileGratie: TcxSpinEdit
        Left = 607
        Top = 22
        Anchors = [akTop, akRight]
        TabOrder = 11
        Value = 10
        Width = 41
      end
      object edPrioritate: TcxSpinEdit
        Left = 655
        Top = 22
        Anchors = [akTop, akRight]
        TabOrder = 12
        Value = 1
        Width = 41
      end
      object edTipValidare: TcxImageComboBox
        Left = 703
        Top = 22
        Anchors = [akTop, akRight]
        EditValue = 1
        Properties.Items = <>
        TabOrder = 13
        Width = 113
      end
      object edRaportGenerat: TcxPopupEdit
        Left = 57
        Top = 201
        Properties.PopupControl = TreeReportList
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseUp = edRaportGeneratPropertiesCloseUp
        TabOrder = 14
        Width = 193
      end
      object TreeFunctiuni: TdxDBTreeList
        Left = 304
        Top = 88
        Width = 329
        Height = 201
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_FUNCTIUNI'
        ParentField = 'ID_PARINTE'
        TabOrder = 15
        Visible = False
        OnDblClick = TreeFunctiuniDblClick
        OnKeyDown = TreeFunctiuniKeyDown
        DataSource = frmData.DTFunctiuni
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
        OptionsView = [etoAutoCalcPreviewLines, etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoPreview, etoUseBitmap, etoUseImageIndexForSelected]
        PreviewFieldName = 'DESCRIERE'
        TreeLineColor = clGrayText
        object TreeFunctiuniDENUMIRE: TdxDBTreeListMaskColumn
          Caption = 'Denumire Functie'
          HeaderAlignment = taCenter
          Sorted = csUp
          Width = 229
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DENUMIRE'
        end
        object TreeFunctiuniCOD_FUNCTIE: TdxDBTreeListMaskColumn
          Caption = 'Cod'
          HeaderAlignment = taCenter
          Width = 58
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_FUNCTIE'
        end
        object TreeFunctiuniDESCRIERE: TdxDBTreeListMaskColumn
          HeaderAlignment = taCenter
          Visible = False
          Width = 420
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DESCRIERE'
        end
      end
      object TreeReportList: TdxDBTreeList
        Left = 344
        Top = 129
        Width = 313
        Height = 201
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_REPORT'
        ParentField = 'ID_PARINTE'
        TabOrder = 16
        Visible = False
        OnDblClick = TreeFunctiuniDblClick
        OnKeyDown = TreeFunctiuniKeyDown
        DataSource = DTReportList
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
        OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
        RootValue = '0'
        TreeLineColor = clGrayText
        object TreeReportListCAPTURA: TdxDBTreeListMaskColumn
          Caption = 'Nume Raport'
          HeaderAlignment = taCenter
          Sorted = csUp
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CAPTURA'
        end
        object TreeReportListIS_REPORT: TdxDBTreeListCheckColumn
          Caption = 'Este Raport'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'IS_REPORT'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
      end
      object edtIsPrinting: TcxDBCheckBox
        Left = 8
        Top = 277
        Caption = 'Se tipareste Raport pe ecran'
        DataBinding.DataField = 'IS_PRINTING'
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.ValueChecked = 'True'
        Properties.ValueUnchecked = 'False'
        TabOrder = 17
        Transparent = True
      end
      object imgDMEdit: TcxDBImageComboBox
        Left = 6
        Top = 304
        DataBinding.DataSource = frmData.DTDefaDoc
        Properties.Items = <>
        TabOrder = 18
        Visible = False
        Width = 281
      end
      object gridValidari: TcxGrid
        Left = 294
        Top = 76
        Width = 523
        Height = 316
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 29
        LookAndFeel.Kind = lfFlat
        object viewValidari: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = DTValidari
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'ID_GEST_TEMPLATE_VALIDARI'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsSelection.HideFocusRectOnExit = False
          OptionsSelection.InvertSelect = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewValidariID_FUNCTIUNE: TcxGridDBColumn
            Caption = 'Functie'
            DataBinding.FieldName = 'ID_FUNCTIUNE'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Filtering = False
            Width = 128
          end
          object viewValidariZILE_GRATIE: TcxGridDBColumn
            Caption = 'Zile Gratie'
            DataBinding.FieldName = 'ZILE_GRATIE'
            PropertiesClassName = 'TcxSpinEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 54
          end
          object viewValidariPRIORITATE: TcxGridDBColumn
            Caption = 'Prioritate'
            DataBinding.FieldName = 'PRIORITATE'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Cea mai mica'
                ImageIndex = 0
                Value = '0'
              end
              item
                Description = 'Mica'
                ImageIndex = 1
                Value = '1'
              end
              item
                Description = 'Normala'
                ImageIndex = 2
                Value = '2'
              end
              item
                Description = 'Mare'
                ImageIndex = 3
                Value = '3'
              end
              item
                Description = 'Cea mai mare'
                ImageIndex = 4
                Value = '4'
              end>
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Filtering = False
            Width = 53
          end
          object viewValidariTIP_VALIDARE: TcxGridDBColumn
            Caption = 'Tip'
            DataBinding.FieldName = 'TIP_VALIDARE'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <
              item
                Description = 'Informare'
                ImageIndex = 0
                Value = '0'
              end
              item
                Description = 'Validare efectiva'
                ImageIndex = 1
                Value = '1'
              end
              item
                Description = 'Introducere'
                ImageIndex = 2
                Value = '2'
              end
              item
                Description = 'Introducere/Validare'
                ImageIndex = 3
                Value = '3'
              end>
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Filtering = False
            Width = 84
          end
        end
        object nivelValidari: TcxGridLevel
          GridView = viewValidari
        end
      end
    end
    object tabContabilitate: TcxTabSheet
      Caption = 'Generare note contabile'
      ImageIndex = 2
      DesignSize = (
        824
        400)
      object BtnAddNota: TcxButton
        Left = 425
        Top = 124
        Width = 73
        Height = 22
        Caption = 'Adauga'
        TabOrder = 5
        OnClick = BtnAddNotaClick
      end
      object BtnModifyNota: TcxButton
        Left = 502
        Top = 124
        Width = 73
        Height = 22
        Caption = 'Modifica'
        TabOrder = 6
        OnClick = BtnModifyNotaClick
      end
      object BtnDeleteNota: TcxButton
        Left = 579
        Top = 124
        Width = 73
        Height = 22
        Caption = 'Sterge'
        Enabled = False
        TabOrder = 7
        OnClick = BtnDeleteNotaClick
      end
      object BtnAddMaterial: TcxButton
        Left = 109
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Adauga'
        TabOrder = 8
        OnClick = BtnAddMaterialClick
      end
      object BtnModifyMaterial: TcxButton
        Left = 186
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Modifica'
        TabOrder = 9
        OnClick = BtnModifyMaterialClick
      end
      object BtnDeleteMaterial: TcxButton
        Left = 263
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Sterge'
        Enabled = False
        TabOrder = 10
        OnClick = BtnDeleteMaterialClick
      end
      object BtnCopy: TcxButton
        Left = 347
        Top = 124
        Width = 73
        Height = 22
        Caption = 'Copiaza'
        TabOrder = 11
        OnClick = BtnCopyClick
      end
      object Label7: TcxLabel
        Left = 345
        Top = 3
        Caption = 'Cont debitor : '
      end
      object Label11: TcxLabel
        Left = 501
        Top = 2
        Caption = 'Cont creditor :'
      end
      object Label12: TcxLabel
        Left = 345
        Top = 44
        Caption = 'Formula de contare : '
      end
      object Label13: TcxLabel
        Left = 345
        Top = 83
        Caption = 'Clasificatie Economica'
      end
      object Label3: TcxLabel
        Left = 9
        Top = 3
        Caption = 'Lista tipurilor de materiale suportate'
      end
      object edContDebitor: TcxPopupEdit
        Left = 345
        Top = 21
        Properties.PopupControl = TreePlan
        Properties.OnCloseUp = edContDebitorPropertiesCloseUp
        Properties.OnPopup = edContDebitorPropertiesPopup
        TabOrder = 0
        OnKeyPress = edContDebitorKeyPress
        Width = 145
      end
      object edContCreditor: TcxPopupEdit
        Left = 501
        Top = 21
        Properties.PopupControl = TreePlan
        Properties.OnCloseUp = edContCreditorPropertiesCloseUp
        Properties.OnPopup = edContCreditorPropertiesPopup
        TabOrder = 1
        OnKeyPress = edContDebitorKeyPress
        Width = 142
      end
      object edFormulaNota: TcxButtonEdit
        Left = 346
        Top = 59
        Properties.Buttons = <
          item
            Default = True
          end>
        TabOrder = 2
        Width = 296
      end
      object edClasificatieEcNota: TcxButtonEdit
        Left = 345
        Top = 98
        Properties.Buttons = <
          item
            Default = True
          end>
        Properties.OnButtonClick = edClasificatieEcNotaButtonClick
        TabOrder = 3
        Width = 296
      end
      object edTipuriMaterial: TcxImageComboBox
        Left = 8
        Top = 24
        Properties.Items = <>
        Properties.OnChange = edTipuriMaterialChange
        TabOrder = 4
        Width = 328
      end
      object gridTipuriMaterial: TcxGrid
        Left = 8
        Top = 80
        Width = 328
        Height = 310
        Anchors = [akLeft, akTop, akBottom]
        TabOrder = 17
        LookAndFeel.Kind = lfFlat
        object viewTipMaterial: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedRecordChanged = viewTipMaterialFocusedRecordChanged
          DataController.DataSource = DTTipuriMateriale
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'ID_GEST_ITEMSI_TIP_MATERIAL'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Inserting = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsSelection.InvertSelect = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewTipMaterialID_GEST_TIP_MATERIAL: TcxGridDBColumn
            Caption = 'Tip Material'
            DataBinding.FieldName = 'ID_GEST_TIP_MATERIAL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Editing = False
            Options.Filtering = False
            Width = 177
          end
          object viewTipMaterialGENEREAZA_CODMAT: TcxGridDBColumn
            Caption = 'Cod nou'
            DataBinding.FieldName = 'GENEREAZA_CODMAT'
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = False
            Properties.ValueGrayed = ''
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Filtering = False
            Width = 71
          end
          object viewTipMaterialACCEPT_STOCK_NEGATIV: TcxGridDBColumn
            Caption = 'Stock negativ'
            DataBinding.FieldName = 'ACCEPT_STOCK_NEGATIV'
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = False
            Properties.ValueGrayed = ''
            MinWidth = 16
            Options.Filtering = False
            Width = 79
          end
        end
        object nivelTipMaterial: TcxGridLevel
          GridView = viewTipMaterial
        end
      end
      object gridModContare: TcxGrid
        Left = 347
        Top = 153
        Width = 472
        Height = 237
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 18
        LookAndFeel.Kind = lfFlat
        object viewModContare: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedRecordChanged = viewModContareFocusedRecordChanged
          DataController.DataSource = DTModContare
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'ID_GEST_DEFA_NOTA_CONT'
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
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object viewModContareID_GEST_TIP_MATERIAL: TcxGridDBColumn
            Caption = 'Tip Docum.'
            DataBinding.FieldName = 'ID_GEST_TIP_MATERIAL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Filtering = False
            Width = 84
          end
          object viewModContareCONT_DEBITOR: TcxGridDBColumn
            Caption = 'Debitor'
            DataBinding.FieldName = 'CONT_DEBITOR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 67
          end
          object viewModContareCONT_CREDITOR: TcxGridDBColumn
            Caption = 'Creditor'
            DataBinding.FieldName = 'CONT_CREDITOR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Filtering = False
            Width = 89
          end
          object viewModContareCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Cod Economic'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Options.Filtering = False
            Width = 90
          end
          object viewModContareCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Cod Functional'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Alignment.Vert = taTopJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Options.Filtering = False
            Width = 160
          end
        end
        object nivelModContare: TcxGridLevel
          GridView = viewModContare
        end
      end
    end
  end
  object edtEsteActiv: TcxDBCheckBox
    Left = 12
    Top = 95
    Caption = 'Este Activ ?'
    DataBinding.DataField = 'STARE'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.ImmediatePost = True
    Properties.NullStyle = nssUnchecked
    TabOrder = 7
  end
  object edCod: TcxDBTextEdit
    Left = 130
    Top = 18
    DataBinding.DataField = 'COD_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 0
    Width = 71
  end
  object edNume: TcxDBTextEdit
    Left = 208
    Top = 18
    DataBinding.DataField = 'DEN_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 1
    Width = 273
  end
  object edDescriere: TcxDBMemo
    Left = 488
    Top = 19
    Anchors = [akLeft, akTop, akRight]
    DataBinding.DataField = 'DESC_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 2
    Height = 95
    Width = 344
  end
  object edSuportaFiliala: TcxDBCheckBox
    Left = 12
    Top = 74
    Caption = 'Document suportat la filiala'
    DataBinding.DataField = 'SUPORTA_FILIALA'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.NullStyle = nssUnchecked
    TabOrder = 3
  end
  object ChkComplementare: TcxDBCheckBox
    Left = 176
    Top = 74
    Caption = 'Complementeaza tipul de primitor in functie de predator'
    DataBinding.DataField = 'COMPLEMENTEAZA_GEST'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.NullStyle = nssUnchecked
    TabOrder = 5
    OnClick = ChkComplementeazaPredatorClick
  end
  object edPredator: TcxDBImageComboBox
    Left = 130
    Top = 46
    DataBinding.DataField = 'TIP_PREDATOR'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.Images = Imagini
    Properties.Items = <
      item
        Description = 'Nu accepta'
        ImageIndex = 0
        Value = 0
      end
      item
        Description = 'Accepta Intern'
        ImageIndex = 1
        Value = 1
      end
      item
        Description = 'Accepta Extern'
        ImageIndex = 2
        Value = 2
      end
      item
        Description = 'Accepta tot'
        ImageIndex = 3
        Value = 3
      end>
    Properties.OnChange = edPredatorPropertiesChange
    TabOrder = 8
    Width = 175
  end
  object edPrimitor: TcxDBImageComboBox
    Left = 306
    Top = 46
    DataBinding.DataField = 'TIP_PRIMITOR'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.Images = Imagini
    Properties.Items = <
      item
        Description = 'Nu accepta'
        ImageIndex = 0
        Value = 0
      end
      item
        Description = 'Accepta Intern'
        ImageIndex = 1
        Value = 1
      end
      item
        Description = 'Accepta Extern'
        ImageIndex = 2
        Value = 2
      end
      item
        Description = 'Accepta tot'
        ImageIndex = 3
        Value = 3
      end>
    Properties.OnChange = edPredatorPropertiesChange
    TabOrder = 9
    Width = 175
  end
  object btnModifyPosition: TcxButton
    Left = 14
    Top = 582
    Width = 183
    Height = 25
    Anchors = [akLeft, akBottom]
    Caption = 'Vizibilitate pe produs'
    TabOrder = 11
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = [fsBold]
    ParentFont = False
    OnClick = btnModifyPositionClick
  end
  object BtnOk: TcxButton
    Left = 678
    Top = 581
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
    TabOrder = 12
  end
  object BtnCancel: TcxButton
    Left = 748
    Top = 581
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
    TabOrder = 13
  end
  object TreePlan: TdxDBTreeList
    Left = 375
    Top = 325
    Width = 417
    Height = 205
    SearchType = stContain
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'CONT'
    ParentField = 'PARINTE'
    TabOrder = 10
    Visible = False
    OnDblClick = TreePlanDblClick
    OnKeyDown = TreePlanKeyDown
    DataSource = frmData.DTPlanCont
    LookAndFeel = lfFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
    OptionsCustomize = [etoBandMoving, etoBandSizing, etoColumnMoving, etoColumnSizing, etoExtCustomizing, etoKeepColumnWidth]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreePlanCONT: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Cont'
      HeaderAlignment = taCenter
      Visible = False
      Width = 137
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT'
    end
    object TreePlanROMANA: TdxDBTreeListMaskColumn
      Caption = 'Plan Cont'
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 330
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ROMANA'
      OnGetText = TreePlanROMANAGetText
    end
    object TreePlanFctCont: TdxDBTreeListColumn
      Caption = 'Funct.'
      Width = 85
      BandIndex = 0
      RowIndex = 0
      FieldName = 'FCTCONT'
    end
  end
  object Imagini: TImageList
    Left = 776
    Bitmap = {
      494C010104000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000D6000000D6000000A50000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000CE000000D6000000DE000000DE000000D6000000CE000000AD000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000ADAD0000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000031310000BDBD0000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      CE001010E7003131FF002121FF002121FF001818F7000000DE000000D6000000
      AD00000000000000000000000000000000000000000000000000000000000000
      000000000000A5A5000052520000ADAD00000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000039390000B5B5000042420000C6C600000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000004A4A0000E7E70000E7E70000E7E70000E7E70000E7E70000CECE
      00000000000000000000000000000000000000000000000000000808D6004A4A
      FF00B5B5FF00ADADFF003939FF002121FF002929FF005252FF009C9CFF001818
      F7000000AD000000000000000000000000000000000000000000000000000000
      00009C9C00005A5A0000E7DE0000BDAD000031310000DEDE0000DEDE0000DEDE
      0000DEDE0000DEDE0000D6D60000000000000000000000000000DEDE0000DEDE
      0000DEDE0000DEDE00009C9C000042390000F7E70000BDBD000039390000C6C6
      00000000000000000000000000000000000000000000000000009C9C42008484
      000042420000D6B51800FFA56300FFBD8400FFCE9C00FFCE9400FFB57300FFA5
      5200CECE000000000000000000000000000000000000000000001818EF006B6B
      FF00CECEFF00EFF7FF00ADADF7004A4AFF007B7BFF00DEE7FF00E7E7FF006B6B
      FF000000C6000000000000000000000000000000000000000000000000009494
      000063630000E7BD2100EF9C4A008C7B00000808000029290000292900002929
      0000292900002929000021210000000000000000000010100000292900002929
      000029290000292900001818000029290000C6A51000F79C5200BDBD00003939
      0000C6C600000000000000000000000000000000000000000000B5A58C00E7C6
      5A00CEAD1800FFA57300DEA594007B6B6B007B736B007B736B00947B7300FFBD
      9C00E7945200000000000000000000000000000000001818D6003939FF004A4A
      FF008C8CFF00E7E7FF00EFEFFF00BDC6FF00DEE7FF00EFF7FF00ADADFF003131
      FF000000D6000000A500000000000000000000000000000000008C8C00006B6B
      0000C6CE0000BDB50000B58C1000A59C0000949C0000CEB50000CECE0000CECE
      0000CECE0000BDA50000000000000000000000000000847B2100CEBD0000CECE
      0000CECE0000CEC60000BDAD0000949C0000AD940000BD8C1000BDCE0000BDBD
      000039390000BDBD000000000000000000000000000000000000B5A59400FFB5
      8C00FF9C6300BD7B630029211800000000000000000000000000101010003121
      210029181000000000000000000000000000000000002121EF005252FF006363
      FF006B6BFF00A5ADFF00F7FFFF00F7F7FF00EFEFFF00ADADFF005252FF002121
      FF001818F7000000CE00000000000000000000000000000000009C8C5A00C6AD
      5200B5BD0000AD940000AD840000AD840000AD8C0000BD940000BD9C0000C69C
      0800F79C5A00B5BD0000000000000000000000000000A58C6B00FFB57300E79C
      3900BD9C0000BD9C0000B5940000AD8C0000AD840000AD840000ADAD0000BDC6
      1000BD947B00737B390000000000000000000000000000000000B5A59400FFC6
      AD00FFBDA500EFB59C008C6B6300000000000000000000000000000000000000
      000000000000000000000000000000000000000000002929EF006363FF006B6B
      FF007373FF00ADADFF00FFFFFF00FFFFFF00E7E7FF008484FF004242FF002121
      FF002121FF000000CE000000000000000000000000000000000000000000AD8C
      6300EFAD6B00D6B55200ADA50000AD940000AD9C0000ADAD0000ADAD0000ADAD
      0000BDAD18009CB50000000000000000000000000000A58C7B00FFC66B00DEBD
      1000ADAD0000ADAD0000ADA50000AD940000AD9C0000B5AD1000EFB57B00DEA5
      5A00736B63000000000000000000000000000000000000000000000000004231
      2900423129004231290029212100000000000000000000000000000000000000
      000000000000000000000000000000000000000000003131EF006B6BFF007B73
      FF00B5A5FF00E7E7FF00F7F7FF00E7E7FF00F7F7FF00BDC6FF005252FF002121
      FF002121FF000000CE0000000000000000000000000000000000000000000000
      0000AD846B00F7AD8400D6B552008C840000393908006B7B39006B7B39006B7B
      39006B7B39006B7339003942390000000000000000006B6B5A00847B5A007B7B
      39006B7B39006B7B39006B7339007B7B2900B5A51800EFBD7B00EF9C84007363
      5200000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000005A5A3900B5B50000B5B5
      0000B5B50000ADAD0000000000000000000000000000000000004242EF009494
      FF00E7E7FF00F7F7FF00CECEFF009C9CFF00DED6FF00EFEFFF00A59CFF003131
      FF000808D6000000000000000000000000000000000000000000000000000000
      000000000000B5846B00E7AD63007B8400000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000006B733900BDB51800E79C84007B6352000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000006B6B0000EFDE
      0800FFB54A00EFCE6B00000000000000000000000000000000002929EF008484
      FF00C6C6FF00CECEFF009494FF007373FF009C9CFF00D6D6FF009494FF002929
      FF000000C6000000000000000000000000000000000000000000000000000000
      00000000000000000000A5845A00A57B39000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000007B735200CE9C52007B635200000000000000
      0000000000000000000000000000000000000000000000000000000000006363
      2100A5A500006B6B0000000000000000000000000000181800009C940800F7AD
      4A00FFA58400EFBDA50000000000000000000000000000000000000000004242
      F7008484FF008C8CFF007373FF006B6BFF006B6BFF007B7BFF005252FF000808
      D600000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000007B6352002129210000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000063635A007B63520000000000000000000000
      000000000000000000000000000000000000000000000000000000000000AD9C
      7300FFCE5A00DEAD29009C9C00009C9C00009C9C0000AD9C0800F7AD4A00EF94
      7300734A4200EFBDA50000000000000000000000000000000000000000000000
      00002929EF004242F7006B6BFF006363FF004A4AFF001818EF001818D6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000D6A59400FFBD7B00FFCE5200FFCE5200FFCE5200FFC65A00E79C84005A39
      3100000000009C847B0000000000000000000000000000000000000000000000
      000000000000000000002929EF002929EF001818CE0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000BD9C9400BD948C00BD948C00BD948C00B59484008C736B000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF0000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000FFFFFFFFFFFFFFFFFC7FFFFFFFFFFFFF
      F01FFC7FFE3FFC0FE00FF87FFE1FE807C007F001C00FC003C007E0018007C003
      8003C0018003C1C38003C0018003C0FF8003E0018007E1C38003F001800FFF81
      C007F87FFE1FE1C1C007FC7FFE3FE001E00FFE7FFE7FE001F01FFFFFFFFFF00B
      FC7FFFFFFFFFF81FFFFFFFFFFFFFFFFF00000000000000000000000000000000
      000000000000}
  end
  object Enabled: TImageList
    Left = 776
    Top = 40
    Bitmap = {
      494C010102000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000EFEFEF00C6C6C600A5A5A500A5A5A500ADADAD00D6D6D600FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000004ADE63008CB594000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00A5A5B5004A4A8C002929840029298C002929840029297300393963007B7B
      8400DEDEDE000000000000000000000000000000000000000000000000000000
      000000000000317B6B0000DE2900107B4A000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000F7F7F7007B7B
      AD002121A5002121AD002121AD002121AD002121AD002121A5002121A5002929
      7B005A5A6300CECECE0000000000000000000000000000000000000000000000
      00002139940000D6310000D6210000AD29002129AD0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF007373B5002121
      B5002121B5002121BD002121BD002121BD002121BD002121B5002121AD002121
      A500292984005A5A6300DEDEDE00000000000000000000000000000000002129
      AD0000B5390000DE290000DE290000B518001852840000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000A5A5C6002121BD002929
      BD00ADADE7007B7BE7002121CE002121CE002121C6006363CE00C6C6EF004242
      BD002121AD0029297B008C8C8C000000000000000000000000002129B500089C
      390000CE210008D6290029E75A0000B51800087B420000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000E7E7F7004242C6002121C6004242
      BD00E7E7DE00FFFFFF007B7BE7002121CE006363D600EFEFF700FFFFFF007373
      CE002121B5002929A50052526B000000000000000000395AA500009C290000BD
      180008CE210018D639004AE76B0000D62100009C1000C6DEC600000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000A5A5EF002929CE002121D6002121
      DE006363BD00EFEFDE00FFFFFF00ADADF700EFEFF700FFFFF7008C8CCE002929
      CE002121C6002121BD0039398400000000009CA5E70010BD290000B5100008C6
      210018C63900B5E7B5009CEFAD0039E75A0000A510004AAD4A00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000006B6BE7002929D6002929DE002121
      EF002121E7006B6BCE00F7F7EF00FFFFFF00FFFFFF008C8CDE002121D6002121
      DE002121D6002121CE0031319400000000000000000042948C0039D6520021B5
      42002142BD0000000000E7F7E7005AEF730000C61800008C08001831A5000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000006363E7002929E7002929EF002929
      F7002121EF00525AE700F7F7F700FFFFFF00FFFFFF006B6BEF002121E7002121
      E7002121DE002929D60031319C0000000000000000003139D600318C8C002931
      EF00000000000000000000000000A5F7B50039DE5200009C0800085A31000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008484F7003131EF003131FF002929
      FF005A5AEF00EFEFEF00FFFFF700C6C6DE00EFEFEF00FFFFFF007373F7002121
      EF002929E7002929DE0042429C00000000000000000000000000000000000000
      0000000000000000000000000000BDC6D6006BE7730008BD1800007300001039
      8C00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6FF004242F7004A4AFF005252
      FF00E7E7EF00FFFFF7008484D6002121EF006B6BC600EFEFDE00FFFFFF007373
      F7002121F7002929E7006B6BA500000000000000000000000000000000000000
      0000000000000000000000000000000000006394A5004ADE520000940000005A
      08002129D6000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000F7F7FF007373FF005A5AFF006B6B
      FF00C6C6CE008C8CDE001818FF002121FF002121FF006B6BCE00BDBDBD006363
      F7003131FF003939CE00C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000063C67B0029BD31000073
      0000084A42003939CE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000DEDEFF005A5AFF007373
      FF007B7BFF007373FF005A5AFF004A4AFF005252FF005A5AFF005A5AFF005A5A
      FF003939FF009494BD0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000005A6BEF005AD6630008A5
      0800005A00004A6B5A0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000CECEFF006B6B
      FF008C8CFF00A5A5FF00A5A5FF009C9CFF009494FF008484FF007373FF005252
      FF009494C600FFFFFF0000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000006B9CC6005ADE
      5A00009C0000005A0000B5CEB500000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000E7E7
      FF008484FF008484FF009C9CFF00A5A5FF009494FF007373FF006B6BFF00B5B5
      DE00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000008CCE
      9C0039CE39004AC64A00EFFFEF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000F7F7FF00CECEFF009C9CFF009494FF009494FF00BDBDFF00E7E7FF000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00A5E7A500F7FFF70000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00F80FFCFF00000000E007F8FF00000000
      C003F07F000000008001E07F000000008001C07F000000000001803F00000000
      0001003F000000000001841F0000000000018E1F000000000001FE0F00000000
      0001FF07000000000001FF83000000008003FF8300000000C003FFC100000000
      E00FFFE100000000F01FFFE30000000000000000000000000000000000000000
      000000000000}
  end
  object DTTemplateCrid: TDataSource
    DataSet = QryFieldItemsi
    Left = 592
    Top = 8
  end
  object Color: TColorDialog
    Left = 736
    Top = 40
  end
  object QryValidari: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_TEMPLATE_VALIDARI'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 560
    Top = 48
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTValidari: TDataSource
    DataSet = QryValidari
    Left = 528
    Top = 8
  end
  object DTTipuriMateriale: TDataSource
    DataSet = QryTipuriMateriale
    Left = 560
    Top = 8
  end
  object QryTipuriMateriale: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = QryTipuriMaterialeNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_ITEMSI_TIP_MATERIAL'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 624
    Top = 48
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTModContare: TDataSource
    DataSet = QryModContare
    Left = 496
    Top = 8
  end
  object QryModContare: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = QryTipuriMaterialeNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_NOTA_CONT'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 496
    Top = 48
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object QryFieldItemsi: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = QryFieldItemsiNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_DOCUM_ITEMSI'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 656
    Top = 8
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object QryFieldDocum: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_DOCUM_DOCUMENT'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 592
    Top = 48
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ppCopiereMenu: TPopupMenu
    Left = 735
    Top = 4
    object CmdCampuriLipsa: TMenuItem
      Caption = 'Campuri Lipsa'
      OnClick = CmdCampuriLipsaClick
    end
  end
  object qryTipProduse: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from GEST_DEFA_ITEMSI_TIP_PRODUSE'
      'where id_gest_defa_docum = :ID')
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 528
    Top = 47
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
        Size = -1
      end>
  end
  object DTReportList: TDataSource
    DataSet = QryReportList
    Left = 626
    Top = 7
  end
  object QryReportList: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_GET_REPORT_LIST_CONFIG')
    Params = <>
    Left = 659
    Top = 46
  end
end
