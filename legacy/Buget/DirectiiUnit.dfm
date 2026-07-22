object frmBugetDirectii: TfrmBugetDirectii
  Left = 312
  Top = 190
  Caption = 'Nomenclator Departamente'
  ClientHeight = 683
  ClientWidth = 872
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Splitter1: TSplitter
    Left = 620
    Top = 71
    Width = 4
    Height = 529
    Align = alRight
    ExplicitHeight = 593
  end
  object PageDeps: TPageControl
    Left = 0
    Top = 71
    Width = 620
    Height = 529
    ActivePage = tabTree
    Align = alClient
    TabOrder = 0
    ExplicitHeight = 573
    object tabTree: TTabSheet
      Caption = 'Arbore'
      ExplicitHeight = 545
      object TreeDepartamente: TdxDBTreeList
        Left = 0
        Top = 0
        Width = 612
        Height = 501
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_BUGET_DIRECTII'
        ParentField = 'ID_PARINTE'
        Align = alClient
        DragMode = dmAutomatic
        TabOrder = 0
        DataSource = DTBugetDirectii
        Images = Imagini
        LookAndFeel = lfUltraFlat
        OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabs, etoTabThrough]
        OptionsDB = [etoCanDelete, etoCanInsert, etoCanNavigation, etoCheckHasChildren, etoConfirmDelete, etoLoadAllRecords]
        OptionsView = [etoAutoCalcPreviewLines, etoAutoWidth, etoBandHeaderWidth, etoIndicator, etoPreview, etoUseBitmap, etoUseImageIndexForSelected]
        PreviewFieldName = 'DESCRIERE'
        TreeLineColor = clGrayText
        OnGetImageIndex = TreeDepartamenteGetImageIndex
        OnGetSelectedIndex = TreeDepartamenteGetSelectedIndex
        ExplicitHeight = 545
        object TreeDepartamenteDENUMIRE: TdxDBTreeListMaskColumn
          Caption = 'Denumire'
          DisableEditor = True
          HeaderAlignment = taCenter
          Width = 126
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DENUMIRE'
        end
        object TreeDepartamenteDATA_INTRARE: TdxDBTreeListDateColumn
          Caption = 'Data Intr.'
          HeaderAlignment = taCenter
          Width = 76
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DATA_START_FUNCTIONARE'
        end
        object TreeDepartamenteDATA_IESIRE: TdxDBTreeListDateColumn
          Caption = 'Data Ies.'
          HeaderAlignment = taCenter
          Width = 71
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DATA_STOP_FUNCTIONARE'
        end
        object TreeDepartamenteSTARE: TdxDBTreeListMaskColumn
          Visible = False
          Width = 691
          BandIndex = 0
          RowIndex = 0
          FieldName = 'STARE'
        end
      end
    end
    object tabOrganigrama: TTabSheet
      Caption = 'Organigrama'
      ImageIndex = 1
      ExplicitHeight = 545
      object Organigrama: TdxDbOrgChart
        Left = 0
        Top = 0
        Width = 612
        Height = 501
        DataSource = DTBugetDirectii
        KeyFieldName = 'ID_BUGET_DIRECTII'
        ParentFieldName = 'ID_PARINTE'
        TextFieldName = 'DENUMIRE'
        OrderFieldName = 'POS_ID'
        OnLoadNode = OrganigramaLoadNode
        DefaultNodeWidth = 80
        DefaultNodeHeight = 60
        Options = [ocSelect, ocFocus, ocButtons, ocDblClick, ocEdit, ocCanDrag, ocShowDrag, ocInsDel]
        EditMode = [emCenter, emVCenter, emWrap]
        Images = Imagini
        DefaultImageAlign = iaLT
        Align = alClient
        Color = clDefault
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = []
        ParentFont = False
        ExplicitHeight = 545
      end
    end
  end
  object pnTools: TPanel
    Left = 0
    Top = 39
    Width = 872
    Height = 32
    Align = alTop
    TabOrder = 1
    object BtnAddDir: TSpeedButton
      Left = 8
      Top = 5
      Width = 121
      Height = 22
      Caption = 'Adauga Directie'
      Flat = True
      OnClick = BtnAddDirClick
    end
    object BtnAddSubDep: TSpeedButton
      Left = 136
      Top = 5
      Width = 129
      Height = 22
      Caption = 'SubDirectie'
      Flat = True
      OnClick = BtnAddSubDepClick
    end
    object BtnDelDepartament: TSpeedButton
      Left = 272
      Top = 5
      Width = 113
      Height = 22
      Caption = 'Sterge Directie'
      Flat = True
      OnClick = BtnDelDepartamentClick
    end
  end
  object pnRight: TPanel
    Left = 624
    Top = 71
    Width = 248
    Height = 529
    Align = alRight
    BevelOuter = bvNone
    TabOrder = 2
    ExplicitHeight = 573
    object Splitter2: TSplitter
      Left = 0
      Top = 137
      Width = 248
      Height = 4
      Cursor = crVSplit
      Align = alTop
    end
    object pnTopRight: TPanel
      Left = 0
      Top = 0
      Width = 248
      Height = 137
      Align = alTop
      BevelOuter = bvNone
      TabOrder = 0
      DesignSize = (
        248
        137)
      object SpeedButton1: TSpeedButton
        Left = 224
        Top = 24
        Width = 23
        Height = 22
        Anchors = [akTop, akRight]
        Caption = '+'
        Flat = True
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -16
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        OnClick = SpeedButton1Click
      end
      object Label1: TLabel
        Left = 8
        Top = 6
        Width = 62
        Height = 13
        Caption = 'Ordonantator'
      end
      object ieOrdonantatori: TcxDBImageComboBox
        Left = 2
        Top = 25
        Anchors = [akLeft, akTop, akRight]
        DataBinding.DataField = 'ID_BUGET_ORDONANTATORI'
        DataBinding.DataSource = DTBugetDirectii
        Properties.Items = <>
        Properties.OnChange = ieOrdonantatoriChange
        TabOrder = 0
        Width = 220
      end
      object inspOrdonantatori: TcxDBVerticalGrid
        Left = 0
        Top = 52
        Width = 248
        Height = 85
        Align = alBottom
        LookAndFeel.Kind = lfStandard
        OptionsView.CellTextMaxLineCount = 3
        OptionsView.AutoScaleBands = False
        OptionsView.GridLineColor = clBtnFace
        OptionsView.RowHeaderMinWidth = 30
        OptionsView.RowHeaderWidth = 120
        OptionsView.ValueWidth = 118
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 1
        DataController.DataSource = dtBugetOrdonantatori
        Version = 1
        object inspOrdonantatoriNUME: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Nume'
          Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.MaxLength = 0
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'NUME'
          ID = 0
          ParentID = -1
          Index = 0
          Version = 1
        end
        object inspOrdonantatoriPRENUME: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Prenume'
          Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.MaxLength = 0
          Properties.EditProperties.ReadOnly = False
          Properties.DataBinding.FieldName = 'PRENUME'
          ID = 1
          ParentID = -1
          Index = 1
          Version = 1
        end
        object inspOrdonantatoriDATA: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Data'
          Properties.EditPropertiesClassName = 'TcxDateEditProperties'
          Properties.EditProperties.Alignment.Horz = taLeftJustify
          Properties.EditProperties.DateButtons = [btnClear, btnToday]
          Properties.EditProperties.DateOnError = deToday
          Properties.EditProperties.InputKind = ikRegExpr
          Properties.DataBinding.FieldName = 'DATA'
          ID = 2
          ParentID = -1
          Index = 2
          Version = 1
        end
        object inspOrdonantatoriTIP: TcxDBEditorRow
          Expanded = False
          Properties.Caption = 'Tip'
          Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.EditProperties.Items = <>
          Properties.DataBinding.FieldName = 'TIP'
          ID = 3
          ParentID = -1
          Index = 3
          Version = 1
        end
      end
    end
    object InspFunctii: TcxDBVerticalGrid
      Left = 0
      Top = 141
      Width = 248
      Height = 388
      Align = alClient
      LookAndFeel.Kind = lfStandard
      OptionsView.CellTextMaxLineCount = 3
      OptionsView.AutoScaleBands = False
      OptionsView.GridLineColor = clBtnFace
      OptionsView.RowHeaderMinWidth = 30
      OptionsView.RowHeaderWidth = 119
      OptionsView.ValueWidth = 106
      OptionsBehavior.GoToNextCellOnEnter = True
      OptionsBehavior.RowSizing = True
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      TabOrder = 1
      DataController.DataSource = DTBugetDirectii
      ExplicitHeight = 432
      Version = 1
      object InspFunctiiID_BUGET_DIRECTII: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Id Functie'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ID_BUGET_DIRECTII'
        ID = 0
        ParentID = -1
        Index = 0
        Version = 1
      end
      object InspFunctiiID_PARINTE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Id Parinte'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = True
        Properties.DataBinding.FieldName = 'ID_PARINTE'
        ID = 1
        ParentID = -1
        Index = 1
        Version = 1
      end
      object InspFunctiiID_UTILIZATORI: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Creator Functie'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ID_UTILIZATORI'
        ID = 2
        ParentID = -1
        Index = 2
        Version = 1
      end
      object InspFunctiiDENUMIRE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Denumire'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'DENUMIRE'
        ID = 3
        ParentID = -1
        Index = 3
        Version = 1
      end
      object InspFunctiiATRIBUTII: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Atributii'
        Properties.EditPropertiesClassName = 'TcxMemoProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ATRIBUTII'
        ID = 4
        ParentID = -1
        Index = 4
        Version = 1
      end
      object InspFunctiiDATA_START_FUNCTIONARE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Data Intrare'
        Properties.EditPropertiesClassName = 'TcxDateEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.DateButtons = [btnClear, btnToday]
        Properties.EditProperties.DateOnError = deToday
        Properties.EditProperties.InputKind = ikRegExpr
        Properties.DataBinding.FieldName = 'DATA_START_FUNCTIONARE'
        ID = 5
        ParentID = -1
        Index = 5
        Version = 1
      end
      object InspFunctiiDATA_STOP_FUNCTIONARE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Data Incetare'
        Properties.EditPropertiesClassName = 'TcxDateEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.DateButtons = [btnClear, btnToday]
        Properties.EditProperties.DateOnError = deToday
        Properties.EditProperties.InputKind = ikRegExpr
        Properties.DataBinding.FieldName = 'DATA_STOP_FUNCTIONARE'
        ID = 6
        ParentID = -1
        Index = 6
        Version = 1
      end
      object InspFunctiiSHAPE_TYPE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Tip Forma'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_TYPE'
        ID = 7
        ParentID = -1
        Index = 7
        Version = 1
      end
      object InspFunctiiSHAPE_LEFT_TOP: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Latime'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_LEFT_TOP'
        ID = 8
        ParentID = -1
        Index = 8
        Version = 1
      end
      object InspFunctiiSHAPE_RIGHT_BOTT: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Inaltime'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_RIGHT_BOTT'
        ID = 9
        ParentID = -1
        Index = 9
        Version = 1
      end
      object InspFunctiiSHAPE_COLOR: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Culoare'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_COLOR'
        ID = 10
        ParentID = -1
        Index = 10
        Version = 1
      end
      object InspFunctiiSHAPE_FONT_COL: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Culoare Font'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_FONT_COL'
        ID = 11
        ParentID = -1
        Index = 11
        Version = 1
      end
      object InspFunctiiSHAPE_FONT_NAME: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Nume Font'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'SHAPE_FONT_NAME'
        ID = 12
        ParentID = -1
        Index = 12
        Version = 1
      end
      object InspFunctiiCategoryRow1: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Atribute Directie'
        ID = 13
        ParentID = -1
        Index = 13
        Version = 1
      end
      object InspFunctiiCategoryRow2: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Descriere Directie'
        ID = 14
        ParentID = -1
        Index = 14
        Version = 1
      end
      object InspFunctiiCategoryRow3: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Existenta Directie'
        ID = 15
        ParentID = -1
        Index = 15
        Version = 1
      end
      object InspFunctiiSTARE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Stare'
        Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.NullStyle = nssUnchecked
        Properties.EditProperties.ReadOnly = True
        Properties.EditProperties.ValueChecked = '1'
        Properties.EditProperties.ValueGrayed = ''
        Properties.EditProperties.ValueUnchecked = '0'
        Properties.DataBinding.FieldName = 'STARE'
        ID = 16
        ParentID = -1
        Index = 16
        Version = 1
      end
      object InspFunctiiCategoryRow4: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Descriere Vizuala'
        ID = 17
        ParentID = -1
        Index = 17
        Version = 1
      end
      object InspFunctiiPOS_ID: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Pozitie'
        Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'POS_ID'
        ID = 18
        ParentID = -1
        Index = 18
        Version = 1
      end
      object InspFunctiiDESCRIERE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Descriere'
        Properties.EditPropertiesClassName = 'TcxMemoProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'DESCRIERE'
        ID = 19
        ParentID = -1
        Index = 19
        Version = 1
      end
      object InspFunctiiID_BUGET_TIP_ORDONATOR: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Tip Ordonator'
        Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.DropDownRows = 7
        Properties.EditProperties.Items = <
          item
            Description = 'Principal'
            ImageIndex = 0
            Value = '1'
          end
          item
            Description = 'Secundar'
            ImageIndex = 1
            Value = '2'
          end
          item
            Description = 'Tertiar'
            ImageIndex = 2
            Value = '3'
          end
          item
            Description = 'Altul'
            ImageIndex = 3
            Value = '4'
          end>
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ID_BUGET_TIP_ORDONATOR'
        ID = 20
        ParentID = -1
        Index = 20
        Version = 1
      end
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 872
    Height = 39
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Intretinere Organigrama Directii'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -21
    Font.Name = 'Arial'
    Font.Style = []
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 3
    Indent = 15
    StartColor = 15444592
    EndColor = 15788249
    DegradeType = dtHorizontal
  end
  object pnBotomSelect: TcxGroupBox
    Left = 0
    Top = 600
    Align = alBottom
    PanelStyle.Active = True
    TabOrder = 4
    Visible = False
    DesignSize = (
      872
      83)
    Height = 83
    Width = 872
    object btnOkSelect: TcxButton
      Left = 690
      Top = 14
      Width = 80
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'Ok'
      LookAndFeel.Kind = lfOffice11
      ModalResult = 1
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F4F4F4FFD9DDD9FFA9BDA9FF7BA57BFF639C64FF639C64FF7EA87EFFABC0
        ABFFD9DED9FFF5F5F5FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EDEE
        EDFFB8C5B8FF58A459FF27B429FF11D015FF06E00DFF06E00CFF10D016FF24B8
        27FF55AB57FFB9C8B9FFEEEFEEFFF8F8F800F8F8F800F8F8F800F0F0F0FFACBC
        ACFF43A846FF0BD711FF03EA0BFF16E11DFF06F20EFF00F708FF00F608FF00F2
        08FF08DD0FFF3DB040FFAAC3AAFFF0F1F0FFF8F8F800F7F7F7FFC6CFC6FF4BA7
        4CFF0AD410FF01F009FF36DF3BFF8FC590FF2EDC34FF00F808FF00F808FF00F8
        08FF00F408FF07DB0EFF42B045FFC4D2C4FFF7F7F7FFE8EAE8FF75AE76FF0EC3
        14FF06E50DFF46DA4BFFC1DAC2FFE9EAE9FFA9CAAAFF12E819FF00F708FF00F8
        08FF00F808FF00ED08FF0ACB10FF69B26BFFE5EAE5FFCFDACFFF3CA43EFF21CF
        27FF5ED662FFCCDBCCFFDFECDFFFC5E9C5FFE0E7E0FF77CB79FF12EA19FF00F8
        08FF00F808FF00F108FF00DB08FF2EAB30FFC9D9C9FFB4CAB4FF1FA622FF31CC
        37FFB0DCB0FFF3F4F3FFA9E7ABFF44DF48FFC7EEC8FFD4DFD4FF67D069FF0EEC
        14FF00F808FF00F108FF00DE08FF13B118FFA4C5A4FFA9C5A9FF16A81AFF06D3
        0EFF25E12DFF86E288FF5BE760FF04F30CFF6EE571FFF4F4F4FFCEDCCFFF64CD
        67FF0BED12FF00EE08FF00DA08FF08B40FFF8DBA8EFFADC9AEFF17A11BFF01CA
        09FF00E308FF00F408FF00F708FF00F808FF05F00DFFB9E9BAFFF5F5F5FFE2E2
        E2FF92C593FF12D519FF00D009FF09AB0EFF91BC91FFC4D6C4FF259928FF04BD
        0BFF02D60AFF00EB08FF00F308FF00F708FF00F808FF3CE942FFCEEFD0FFF6F6
        F6FFD9D9D9FF19C120FF04C20CFF169D1AFFADCAADFFDFE7DFFF53A455FF10AC
        14FF0BC613FF00D908FF00E708FF00EE08FF00F108FF08EE0FFF5BE25FFFD7EE
        D8FFD9D9D9FF0CB612FF0FB214FF399C3BFFD2DFD2FFF2F4F2FF9ABF9BFF229B
        25FF28B92BFF09C410FF00D108FF00DC08FF00E008FF00DF08FF0AD811FF59CF
        5DFFCDDFCEFF2FAC31FF1E9F21FF81B481FFEDF0EDFFF8F8F800E4E9E4FF76AC
        76FF38A33AFF44BE48FF20BD25FF0BBE12FF05C10CFF05C10BFF0BBE10FF1EBB
        21FF57BA59FF43A445FF5FA55FFFD9E2D9FFF8F8F800F8F8F800F8F8F800DAE2
        DAFF78AE78FF53A955FF63BF66FF60C563FF54C457FF54C457FF60C562FF65C0
        67FF54AC56FF66A867FFCBD9CCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8
        F800E4EAE4FFA0C3A0FF7AB07BFF70AC71FF74B274FF74B275FF71AD71FF77AD
        77FF96BD96FFDCE4DCFFF7F7F7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F2F4F2FFE0E8E0FFCADACAFFB8CFB8FFB7CEB7FFC9D9C9FFDEE7
        DEFFF0F2F0FFF8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 0
      OnClick = btnOkSelectClick
    end
    object btnCancelSelect: TcxButton
      Left = 776
      Top = 14
      Width = 80
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'Abandon'
      LookAndFeel.Kind = lfOffice11
      ModalResult = 2
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F8F8F800F8F8F800EDEDF1FFBFBFD5FF9191B9FF8F8FB7FFBBBBD3FFEAEA
        F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EFEFF2FF8787B2FF303089FF12129CFF0E0EACFF0D0DADFF12129EFF2B2B
        89FF7E7EACFFE9E9EFFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DADA
        E5FF42428BFF0D0DA9FF0101DBFF0000E9FF0000EFFF0000F0FF0000EAFF0000
        DEFF0C0CAFFF393987FFCFCFDFFFF8F8F800F8F8F800F8F8F800EAEAF0FF3E3E
        88FF0707B8FF0000E1FF0101EEFF0000F6FF0000F8FF0000F8FF0000F6FF0202
        EFFF0000E3FF0505BEFF333386FFE1E1EAFFF8F8F800F8F8F8007B7BABFF0C0C
        A4FF0000D9FF1010E9FFACACF0FF4444F1FF0101F8FF0000F8FF4343F6FFA6A6
        EDFF1010EAFF0000DAFF0909ADFF68689EFFF7F7F8FFE5E5EDFF242483FF0000
        C4FF0101DBFFB6B6F1FFF8F8F800E6E6F3FF4545F1FF4343F6FFEAEAF6FFF7F7
        F8FFAFAFEDFF0202DEFF0000C9FF1B1B84FFD9D9E5FFB4B4CEFF111190FF0000
        C8FF0000D9FF4242E0FFEDEDF6FFF8F8F800EBEBF4FFEFEFF6FFF8F8F800EEEE
        F7FF4545E8FF0000DBFF0000CAFF0E0E95FFA4A4C4FF8C8CB4FF0C0C97FF0000
        C4FF0000D4FF0000E0FF4343E4FFEFEFF7FFF8F8F800F8F8F800F0F0F7FF4747
        EBFF0000E1FF0000D5FF0000C6FF09099DFF8787B2FF8B8BB4FF0C0C93FF0000
        BDFF0000CBFF0000D7FF5050E7FFF2F2F7FFF8F8F800F8F8F800EDEDF4FF4F4F
        E2FF0101D7FF0000CDFF0000BEFF09099AFF8787B2FFB2B2CDFF111188FF0000
        B3FF0A0AC2FF5555D9FFF0F0F6FFF8F8F800EDEDF6FFEBEBF5FFF8F8F800EBEB
        F2FF5353D7FF0A0AC4FF0000B5FF0E0E8DFFA3A3C3FFE5E5ECFF23237FFF0000
        A8FF1B1BBAFFB6B6E2FFF8F8F800E8E8F5FF3A3AD5FF3737CEFFE6E6F3FFF8F8
        F800B4B4E4FF1C1CBEFF0000AAFF1C1C7FFFD9D9E4FFF8F8F8007A7AA9FF0C0C
        8EFF1919B2FF5353C6FFB1B1DFFF4545CBFF0404C0FF0303C0FF4040C4FFB1B1
        E2FF5454C9FF1C1CB4FF080893FF65659DFFF7F7F8FFF8F8F800E9E9EFFF3C3C
        86FF080895FF4F4FC0FF8282D4FF6C6CD0FF5555CBFF5454CAFF6969CFFF8282
        D5FF5353C2FF070797FF323281FFDFDFE9FFF8F8F800F8F8F800F8F8F800D8D8
        E5FF404088FF12128BFF4F4FB9FFA4A4DBFFBFBFE4FFC0C0E4FFA7A7DDFF5454
        BCFF13138FFF373784FFCDCDDDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EDEDF2FF8484AFFF2E2E82FF1A1A85FF2E2E95FF2F2F95FF1B1B86FF2929
        80FF7A7AAAFFE6E6EDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F8F8F800EBEBF0FFBCBCD3FF9191B7FF8D8DB6FFB7B7D1FFE8E8
        EEFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 1
      OnClick = btnCancelSelectClick
    end
  end
  object ppTipDepartament: TPopupMenu
    Left = 60
    Top = 273
    object ppDreptunghi: TMenuItem
      Caption = 'Dreptunghi'
    end
    object ppRoundedRect: TMenuItem
      Caption = 'Dreptunghi Rotunjit'
    end
    object ppEllipse: TMenuItem
      Caption = 'Elipsa'
    end
    object ppDiamond: TMenuItem
      Caption = 'Romb'
    end
  end
  object Imagini: TImageList
    Left = 60
    Top = 217
    Bitmap = {
      494C010103000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000FCFAFA00AE4E4B00AB5E5C0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000F8F6F600D0C0C100B9A1A100E6DCDC000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000F9F6F700B8636000E09E9C00EEA9A700AFA0A000AA817F00984C49000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000F7F5F500DACECF00B89A9A00C19C9C00B5A8A800957E7E00C0AC
      AC00E3DADA000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000005F675F00A85E
      5A00E6B8B400ECBAB800EAB1AF00EB928F00B77D7B00BB929000C08A8F009BA9
      82009B828200000000000000000000000000F0FFFF00BEEDED00555F5500555F
      5500555F55007A9F9F00C7A9A900E0C7C700E0C3C300B9A7A700B1A9A900AC9D
      9D009D848400C1AEAE00EBE5E40000000000000000008000000080000000FF00
      0000FF000000FF000000FF000000FF0000008000000080000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000D4F8F80092616000DFB8B500EBD0
      D000E8C6C400E9939100EA7F7C00EBA19F00BF737100BB484400BE706E00C881
      83005E705E008A908100BFA9AB0000000000D4FBFB00555F5500BEFFFF00555F
      5500C6FFFF00555F5500E3CECE00D8B6B600D3A9A900AD848400A4838300B196
      9800B1AFA6009EA59300C1AAAC000000000000000000FF000000FF000000FF00
      0000FF00000080000000C0C0C000800000008000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BC4E4A00EEEEEE00EACCCB00EA8B
      8800EA9C9A00EBBBB900EAAEAC00EBA6A400BF524F00C6646000CB757300C053
      500059C7770034934C00A997970000000000B7D9D90000000000BAECEC00555F
      5500CCEBEB0000000000D4ADAD00D3AAAA00D8B4B400BA9A9A00A57D7D00A379
      7900A8868400A78B8800C5B0B00000000000000000000000000080000000C0C0
      C000C0C0C000C0C0C000C0C0C000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000B9504C00EB8F8C00E9C7C500E9D1
      D100E9C2C000EAB8B600EAAEAC00EBA6A400C4514D00EE7B7700EB757100CC59
      52004BC36B0046B763009D89890000000000958E8F00A9B1B200E1F2F200555F
      5500C8EAEA00A1A2A200DDC5C500DDC4C400DCC0C000BB959500AE808000B188
      8800AB7F8000A2717200C3ABAB0000000000000000000000000000000000C0C0
      C000C0C0C000C0C0C00080808000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BA514E00EAE5E500E9D6D600E9CC
      CC00E9C2C000EAB8B600EAAEAC00EBA6A400CA4E4A00C7514D00EB787400D152
      4B0053C57200369D510062454500DCDBDB00B69A9900F1F1F100000000000000
      000000000000AAAAAA00DFCACA00DCC2C200DBBDBD00BA8F8F00BE8B8B00CA96
      9600BA898900AF818000C6AFAE00000000000000000000000000000000000000
      0000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C00000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BA514E00EAE3E300E9D6D600E9CC
      CC00E9C2C000EAB8B600EAAEAC00EBA6A400CE484500D35D5900E16E6A00D44F
      48005BC878003DB35C00809A82007A535400BC9A9A00EBE0E000555F5500EDFF
      FF00555F5500CBC7C700DFC9C900DCC2C200DBBDBD00BE8D8D00B8848400C895
      9500CE9A9A00B17D7B00C7AEAD00000000000000000000000000000000000000
      000000000000C0C0C000C0C0C000C0C0C000C0C0C00000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BA514E00EAE3E300E9D6D600E9CC
      CC00E9C2C000EAB8B600EAAEAC00EBA6A400CF423E00E5757100F27D7400D748
      420061C97D003CAE590082605F0078565400BB9C9C00EFEBEB00F1F1F100555F
      5500E0DEDE00D9C9C900DFC9C900DCC2C200DBBDBD00C08D8D00B9838300B17F
      7F00BC898900B0797800C7ADAC00000000000000000000000000000000000000
      000000000000C0C0C000C0C0C000C0C0C000C0C0C00000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BB535000EAE3E300E9D6D600E9CC
      CC00E9C2C000EAB8B600EAAEAC00EAA5A300F53F3900F5433A00E13B3400DC40
      3B0056C674003CAE5900926C6C007C595700D4C3C300F2F1F100E8E1E100E7DB
      DB00E4D5D500E1CFCF00DFC9C900DCC2C200DBBDBD00C0888800BF8B8B00BD8B
      8B00BE8B8B00B3787700C7ABAB00000000000000000000000000000000000000
      0000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000BB524E00EAE3E300E9D6D600E9CC
      CC00E9C2C000EABAB800EEBCBA00F4D7D700F89B9400F54E4700F5383200F541
      3B006CCD87003CAD5900A27C7B007B575500D4C3C300F2F1F100E8E1E100E7DB
      DB00E4D5D500E1CFCF00DFC9C900DCC2C200DBBDBD00C3858500B2797900BF8F
      8E00C8979600B4777600C7ABAA00000000000000000000000000000000000000
      000000000000C0C0C000C0C0C000C0C0C000C0C0C00000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C1635F00EDEAEA00ECCCCB00EFBD
      BB00F2B9B700F4B8B600F3B8B300F4B9B400F9BCB700F5BCBA00DFA9A700D147
      43003EBD60003CB05A009A7472007C555300D4C3C300F2F1F100E8E1E100E7DB
      DB00E4D5D500E1CFCF00DFC9C900DBC0C000D9BABA00C87C7C00BD646400B56B
      6A00B5797800B16F6F00C9ABAB00000000000000000000000000000000000000
      0000000000000000000000000000C0C0C000C0C0C00000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C8BABB00CB9F9E00C67D7900D66B
      6600E3767100DB6C6700C4685E00A8745F00A59B8100AFD7B400B1E4BF0082D4
      980053C572003DB35C00A07B79007C535300D4C2C200F2F1F100E8E1E100E7DB
      DB00E4D5D500E1CFCF00DFC8C800E0C8C800E4D0D000DEADAC00D1828100C96A
      6900C8656500C0656500C8AAAA00000000000000000000000000000000000000
      0000000000000000000000000000C0C0C000C0C0C000C0C0C000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000B0E4
      BF0093DAA6008CD8A00089D69E0089D69E008CD8A00089D69E007AD2920044C0
      650035824600455E3D00A86463007D535200D7C8C800F6F8F800ECE6E600E8DC
      DC00E5D5D500E5D2D200E7D4D400EEDDDD00F3E4E400F6EAE900F3E0DF00E6BC
      BC00DF9C9D00C5707000C3A2A200000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000D4B3
      B300B3A59B00586D51005B9C6D00558A62007D987F00A5A19900C8BBBB00DACB
      CB00E6DCDB00E9CDCD00D89C9B007C4E4D00CCB9B900DECACA00DDC0C000E2BF
      BF00E2C0BF00E3C1C100E3C1C000DFBBBA00D5B1B000C8A3A200C1A09F00C0A5
      A500BBA1A100B99F9F00DCD0D000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000008F777600A3828100C19A9800D1AAA800D3ACAA00C59F9D00A078
      76008B6E6D0094828200B2AAAA00E1E0E000EFEBEB00D9CDCD00CCBCBC00C6AC
      AB00C09D9C00BB919000C09E9D00C6ABAA00CAB4B200D5C4C400E6DDDD00F4F1
      F100FDFCFC000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00FC7FFE1FFFBF0000F01FF807FFD90000
      C0070001801D000000010001806F000000010001C0C7000000010001C0E30000
      00000001803F000000000001803F000000000001003F000000000001003F0000
      00000001003F000000000001003F000000000001003F0000E0000001801F0000
      E0000001C03F0000F8000007E07F000000000000000000000000000000000000
      000000000000}
  end
  object QryBugetDirectii: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    AfterScroll = QryBugetDirectiiAfterScroll
    OnNewRecord = QryBugetDirectiiNewRecord
    SQL.Strings = (
      'SELECT * FROM BUGET_DIRECTII')
    Params = <>
    Left = 384
    Top = 160
  end
  object DTBugetDirectii: TDataSource
    DataSet = QryBugetDirectii
    Left = 263
    Top = 160
  end
  object dtBugetOrdonantatori: TDataSource
    DataSet = QryBugetOrdonantatori
    Left = 266
    Top = 219
  end
  object QryBugetOrdonantatori: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    AfterPost = QryBugetOrdonantatoriAfterPost
    SQL.Strings = (
      
        'select * from buget_ordonantatori where id_buget_ordonantatori =' +
        ' :id_buget_ordonantatori')
    Params = <
      item
        DataType = ftUnknown
        Name = 'id_buget_ordonantatori'
        ParamType = ptUnknown
      end>
    Left = 387
    Top = 219
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'id_buget_ordonantatori'
        ParamType = ptUnknown
      end>
  end
end
