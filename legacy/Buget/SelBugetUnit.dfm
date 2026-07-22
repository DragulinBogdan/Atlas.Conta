object frmSelBuget: TfrmSelBuget
  Left = 375
  Top = 160
  Caption = 'Selectie clasificatie'
  ClientHeight = 366
  ClientWidth = 576
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 288
    Width = 576
    Height = 78
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    OnResize = pnBottomResize
    DesignSize = (
      576
      78)
    object btnOk: TcxButton
      Left = 427
      Top = 6
      Width = 50
      Height = 28
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
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object btnCancel: TcxButton
      Left = 483
      Top = 6
      Width = 82
      Height = 28
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
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
  end
  object PaginaClasificatii: TcxPageControl
    Left = 0
    Top = 0
    Width = 576
    Height = 288
    Align = alClient
    TabOrder = 1
    Properties.ActivePage = cxTabEconomic
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabSlants.Positions = [spLeft, spRight]
    LookAndFeel.Kind = lfOffice11
    ExplicitHeight = 328
    ClientRectBottom = 288
    ClientRectRight = 576
    ClientRectTop = 20
    object cxTabFunctional: TcxTabSheet
      Caption = 'Clasificatie Functionala'
      ImageIndex = 2
      ExplicitHeight = 308
      object pnFunctClient: TPanel
        Left = 0
        Top = 0
        Width = 576
        Height = 268
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 0
        ExplicitHeight = 308
        object cxTreeFunctional: TcxDBTreeList
          Left = 0
          Top = 0
          Width = 576
          Height = 268
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
            end>
          DataController.DataSource = DTPlanFunctional
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
          DragMode = dmAutomatic
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsBehavior.ImmediateEditor = False
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.BandVertSizing = False
          OptionsCustomizing.ColumnVertSizing = False
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Appending = True
          OptionsData.Deleting = False
          OptionsData.Inserting = True
          OptionsSelection.HideFocusRect = False
          OptionsSelection.InvertSelect = False
          OptionsView.CellTextMaxLineCount = -1
          OptionsView.ShowEditButtons = ecsbFocused
          OptionsView.ColumnAutoWidth = True
          OptionsView.ExtPaintStyle = True
          ParentColor = False
          Preview.AutoHeight = False
          Preview.MaxLineCount = 0
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.TreeListStyleSheetHighContrastWhite
          Styles.IncSearch = frmData.cxStyle12
          TabOrder = 0
          OnDblClick = cxTreeFunctionalDblClick
          OnFocusedNodeChanged = cxTreeFunctionalFocusedNodeChanged
          OnKeyDown = cxTreeFunctionalKeyDown
          ExplicitHeight = 308
          object cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Capitol'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            Width = 94
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.CharCase = ecUpperCase
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 107
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Caption.AlignHorz = taCenter
            Caption.Text = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            Width = 230
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeFunctionalDESCRIEREGetDisplayText
          end
          object cxTreeFunctionalCLASA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'CLASA'
            Options.Editing = False
            Width = 80
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalCAPITOL: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Capitol'
            DataBinding.FieldName = 'CAPITOL'
            Width = 80
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = True
            Properties.ValueChecked = 'True'
            Properties.ValueGrayed = ''
            Properties.ValueUnchecked = 'False'
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Lucrare'
            DataBinding.FieldName = 'ESTE_LUCRARE'
            MinWidth = 16
            Width = 50
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalESTE_NIVEL_RAPORTARE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = True
            Properties.ValueChecked = 'True'
            Properties.ValueGrayed = ''
            Properties.ValueUnchecked = 'False'
            Visible = False
            Caption.Text = 'Nivel Raportare'
            DataBinding.FieldName = 'ESTE_NIVEL_RAPORTARE'
            MinWidth = 16
            Width = 46
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_OI_UNITATI'
            Width = 100
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
            Width = 100
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
            Width = 100
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalNUMAR_RAND: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'NUMAR_RAND'
            Width = 100
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_PARINTE'
            Width = 100
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'TIP_BUGET'
            Width = 100
            Position.ColIndex = 12
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ESTE_STANDARD'
            Width = 100
            Position.ColIndex = 13
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalBOLD: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'BOLD'
            Width = 100
            Position.ColIndex = 14
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalTIP_REFLECTARE: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'TIP_REFLECTARE'
            Width = 100
            Position.ColIndex = 15
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_OI_PROIECTE: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_OI_PROIECTE'
            Width = 100
            Position.ColIndex = 16
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalcod_ecran: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'cod_ecran'
            Width = 100
            Position.ColIndex = 17
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalclasa_ecran: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'clasa_ecran'
            Width = 100
            Position.ColIndex = 18
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalTIP: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'TIP'
            Width = 100
            Position.ColIndex = 19
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalID_ANALITIC: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_ANALITIC'
            Width = 100
            Position.ColIndex = 20
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
    object cxTabEconomic: TcxTabSheet
      Caption = 'Clasificatie Economica'
      ImageIndex = 3
      ExplicitHeight = 308
      object pnEcoClient: TPanel
        Left = 0
        Top = 0
        Width = 576
        Height = 268
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 0
        ExplicitHeight = 308
        object cxTreeEconomic: TcxDBTreeList
          Left = 0
          Top = 0
          Width = 576
          Height = 268
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
            end>
          DataController.DataSource = DTPlanEconomic
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_BG_PLAN_ECONOMIC'
          DragMode = dmAutomatic
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsBehavior.AutoDragCopy = True
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = cxTreeEconomicDESCRIERE
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.BandVertSizing = False
          OptionsCustomizing.ColumnVertSizing = False
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Appending = True
          OptionsData.Deleting = False
          OptionsData.Inserting = True
          OptionsSelection.HideFocusRect = False
          OptionsSelection.InvertSelect = False
          OptionsView.CellTextMaxLineCount = -1
          OptionsView.ShowEditButtons = ecsbFocused
          OptionsView.ColumnAutoWidth = True
          ParentColor = False
          Preview.AutoHeight = False
          Preview.MaxLineCount = 0
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.TreeListStyleSheetHighContrastWhite
          Styles.IncSearch = frmData.cxStyle12
          TabOrder = 0
          OnDblClick = cxTreeFunctionalDblClick
          OnFocusedNodeChanged = cxTreeFunctionalFocusedNodeChanged
          OnKeyDown = cxTreeFunctionalKeyDown
          ExplicitHeight = 308
          object cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Titlu'
            DataBinding.FieldName = 'COD_ECONOMIC'
            Width = 76
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicDENUMIRE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.CharCase = ecUpperCase
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 209
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicDESCRIERE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Caption.AlignHorz = taCenter
            Caption.Text = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            Width = 96
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeEconomicDESCRIEREGetDisplayText
          end
          object cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taCenter
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Nr. Rand'
            DataBinding.FieldName = 'NUMAR_RAND'
            Width = 55
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicCLASA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'CLASA'
            Options.Editing = False
            Width = 81
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'ID_BG_PLAN_ECONOMIC'
            Width = 100
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_oi_proiecte: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_oi_proiecte'
            Width = 100
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_oi_unitati: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_oi_unitati'
            Width = 100
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_parinte: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_parinte'
            Width = 100
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiceste_local: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'este_local'
            Width = 100
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiceste_standard: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'este_standard'
            Width = 100
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicbold: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'bold'
            Width = 100
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomictip_reflectare: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'tip_reflectare'
            Width = 100
            Position.ColIndex = 12
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiccod_ecran: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'cod_ecran'
            Width = 100
            Position.ColIndex = 13
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicclasa_ecran: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'clasa_ecran'
            Width = 100
            Position.ColIndex = 14
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomictip: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'tip'
            Width = 100
            Position.ColIndex = 15
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_analitic: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_analitic'
            Width = 100
            Position.ColIndex = 16
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiccod_functional: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'cod_functional'
            Width = 100
            Position.ColIndex = 17
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
  end
  object DTPlanFunctional: TDataSource
    DataSet = qryPlanFunctional
    Left = 28
    Top = 57
  end
  object DTPlanEconomic: TDataSource
    DataSet = qryPlanEconomic
    Left = 311
    Top = 64
  end
  object qryPlanFunctional: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spBGPlanFunctionalComplet')
    Params = <>
    Left = 59
    Top = 55
  end
  object qryPlanEconomic: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from .dbo.fnPlanEconomic(:codFunctional, :idUnitate)')
    Params = <
      item
        DataType = ftString
        Name = 'codFunctional'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Name = 'idUnitate'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 341
    Top = 64
    ParamData = <
      item
        DataType = ftString
        Name = 'codFunctional'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Name = 'idUnitate'
        ParamType = ptUnknown
        Size = -1
      end>
  end
end
