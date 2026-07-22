object frmGenerarePlata: TfrmGenerarePlata
  Left = 282
  Top = 161
  Caption = 'Generare Ordin de Plata'
  ClientHeight = 593
  ClientWidth = 938
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  WindowState = wsMaximized
  OnClose = FormClose
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnClient: TPanel
    Left = 0
    Top = 0
    Width = 938
    Height = 593
    Align = alClient
    BevelOuter = bvNone
    BorderWidth = 4
    TabOrder = 0
    OnResize = pnClientResize
    object GRTop: TGroupBox
      Left = 4
      Top = 4
      Width = 930
      Height = 209
      Align = alTop
      Caption = 'Cautare Facturi Fiscale '
      TabOrder = 0
      object pnTop: TPanel
        Left = 2
        Top = 15
        Width = 926
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
          Left = 313
          Top = 32
          Width = 45
          Height = 13
          Caption = 'Nr &Unic : '
          FocusControl = edNrNota
        end
        object edNrUnic: TcxTextEdit
          Left = 390
          Top = 29
          Properties.ValidateOnEnter = True
          Properties.OnChange = edNrUnicPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 5
          Width = 88
        end
        object edData: TcxDateEdit
          Left = 202
          Top = 29
          Properties.InputKind = ikMask
          Properties.OnChange = edNrUnicPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 1
          Width = 103
        end
        object edListaLuni: TcxImageComboBox
          Left = 202
          Top = 5
          Properties.Items = <>
          Properties.OnChange = edNrUnicPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 0
          Width = 103
        end
        object edOperator: TcxImageComboBox
          Left = 390
          Top = 5
          Properties.Items = <>
          Properties.OnChange = edNrUnicPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 3
          Width = 137
        end
        object edListaAni: TcxImageComboBox
          Left = 133
          Top = 5
          Properties.Items = <>
          Properties.OnChange = edListaAniPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 4
          Width = 65
        end
        object edNrNota: TcxTextEdit
          Left = 133
          Top = 29
          Properties.ValidateOnEnter = True
          Properties.OnChange = edNrUnicPropertiesChange
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 2
          Width = 65
        end
      end
      object cxGridIstoricNote: TcxGrid
        Left = 2
        Top = 72
        Width = 926
        Height = 135
        Align = alClient
        TabOrder = 1
        LookAndFeel.Kind = lfOffice11
        object GridIstoricNote: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = DTFacturi
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.Filter.OnBeforeChange = GridIstoricNoteDataControllerFilterBeforeChange
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
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.Editing = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object GridIstoricNoteJURNAL: TcxGridDBColumn
            Caption = 'Jurnal'
            DataBinding.FieldName = 'JURNAL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 48
          end
          object GridIstoricNoteNRDOC: TcxGridDBColumn
            Caption = 'Nr.'
            DataBinding.FieldName = 'NRDOC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 43
          end
          object GridIstoricNoteDATA: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 45
          end
          object GridIstoricNoteEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 86
          end
          object GridIstoricNoteCONT_DEBT: TcxGridDBColumn
            Caption = 'Cont Deb.'
            DataBinding.FieldName = 'CONT_DEBT'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 27
          end
          object GridIstoricNoteREPARTITOR_DEBIT: TcxGridDBColumn
            Caption = 'Repartitor Debit'
            DataBinding.FieldName = 'REPARTITOR_DEBIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 66
          end
          object GridIstoricNoteCONT_CRED: TcxGridDBColumn
            Caption = 'Cont Cred.'
            DataBinding.FieldName = 'CONT_CRED'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 31
          end
          object GridIstoricNoteREPARTITOR_CREDIT: TcxGridDBColumn
            Caption = 'Repartitor Credit'
            DataBinding.FieldName = 'REPARTITOR_CREDIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 58
          end
          object GridIstoricNoteVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 33
          end
          object GridIstoricNoteMODUL: TcxGridDBColumn
            Caption = 'Modul'
            DataBinding.FieldName = 'MODUL'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
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
            Width = 21
          end
          object GridIstoricNoteBUGET: TcxGridDBColumn
            Caption = 'Buget'
            DataBinding.FieldName = 'BUGET'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 25
          end
          object GridIstoricNoteCOD: TcxGridDBColumn
            Caption = 'Cod'
            DataBinding.FieldName = 'COD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object GridIstoricNotePOZ: TcxGridDBColumn
            Caption = 'Poz'
            DataBinding.FieldName = 'POZ'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object GridIstoricNoteECL: TcxGridDBColumn
            Caption = 'Ecl'
            DataBinding.FieldName = 'ECL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 28
          end
          object GridIstoricNoteCOMPUSA: TcxGridDBColumn
            Caption = 'Comp'
            DataBinding.FieldName = 'COMPUSA'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 84
          end
          object GridIstoricNoteCONTD: TcxGridDBColumn
            Caption = 'ContD'
            DataBinding.FieldName = 'CONTD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 121
          end
          object GridIstoricNoteCONTC: TcxGridDBColumn
            Caption = 'ContC'
            DataBinding.FieldName = 'CONTC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Width = 121
          end
          object GridIstoricNoteC_O: TcxGridDBColumn
            Caption = 'Operator'
            DataBinding.FieldName = 'C_O'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.Items = <>
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 20
          end
          object GridIstoricNoteDATA_OPERARE: TcxGridDBColumn
            Caption = 'Data Operare'
            DataBinding.FieldName = 'DATA_OPERARE'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Width = 29
          end
          object GridIstoricNoteID_INITIAL: TcxGridDBColumn
            Caption = 'Id Initial'
            DataBinding.FieldName = 'ID_INITIAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object GridIstoricNoteID_PARINTE: TcxGridDBColumn
            Caption = 'Parinte'
            DataBinding.FieldName = 'ID_PARINTE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object GridIstoricNoteSTARE: TcxGridDBColumn
            Caption = 'Stare'
            DataBinding.FieldName = 'STARE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 76
          end
          object GridIstoricNoteCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Clas. Func.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 66
          end
          object GridIstoricNoteCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Clas. Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Width = 38
          end
          object GridIstoricNoteDATA_OP: TcxGridDBColumn
            Caption = 'Data Doc'
            DataBinding.FieldName = 'DATA_OP'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Width = 52
          end
          object GridIstoricNoteNR_OP: TcxGridDBColumn
            Caption = 'Nr Unic'
            DataBinding.FieldName = 'NR_OP'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 38
          end
        end
        object cxGridIstoricNoteL: TcxGridLevel
          GridView = GridIstoricNote
        end
      end
    end
    object Panel1: TPanel
      Left = 4
      Top = 504
      Width = 930
      Height = 85
      Align = alBottom
      TabOrder = 1
      DesignSize = (
        930
        85)
      object BtnSalvare: TcxButton
        Tag = 1
        Left = 756
        Top = 6
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Salvare'
        LookAndFeel.Kind = lfOffice11
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
        TabOrder = 1
        OnClick = BtnOkClick
      end
      object btnCancel: TcxButton
        Left = 837
        Top = 6
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Inchide'
        LookAndFeel.Kind = lfOffice11
        ModalResult = 2
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
        TabOrder = 0
        OnClick = btnCancelClick
      end
    end
    object GroupBox2: TGroupBox
      Left = 4
      Top = 221
      Width = 930
      Height = 283
      Align = alClient
      Caption = 'Pozitii Ordin de Plata'
      TabOrder = 2
      ExplicitHeight = 334
      DesignSize = (
        930
        283)
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
      object cxGridOP: TcxGrid
        Left = 2
        Top = 73
        Width = 926
        Height = 208
        Align = alBottom
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 6
        LookAndFeel.Kind = lfOffice11
        ExplicitHeight = 259
        object GridOP: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedRecordChanged = GridOPFocusedRecordChanged
          DataController.DataSource = DTOp
          DataController.Filter.MaxValueListCount = 1000
          DataController.Filter.Active = True
          DataController.KeyFieldNames = 'RecId'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <
            item
              Format = ',0.00;-,0.00'
              Kind = skSum
              FieldName = 'VALOARE'
              Column = GridOPVALOARE
            end>
          DataController.Summary.SummaryGroups = <
            item
              Links = <
                item
                  Column = GridOPCOD_FUNCTIONAL
                end>
              SummaryItems = <
                item
                  Format = ',0.00;-,0.00'
                  Kind = skSum
                  Position = spFooter
                  FieldName = 'VALOARE'
                  Column = GridOPVALOARE
                end>
            end
            item
              Links = <
                item
                  Column = GridOPCOD_ECONOMIC
                end>
              SummaryItems = <
                item
                  Format = ',0.00;-,0.00'
                  Kind = skSum
                  Position = spFooter
                  FieldName = 'VALOARE'
                  Column = GridOPVALOARE
                end>
            end>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.FocusCellOnTab = True
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.Appending = True
          OptionsData.DeletingConfirmation = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsSelection.InvertSelect = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object GridOPNRDOC: TcxGridDBColumn
            Caption = 'Nr. Doc'
            DataBinding.FieldName = 'NRDOC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 74
          end
          object GridOPTIP_DOCUMENT: TcxGridDBColumn
            Caption = 'Tip Docum'
            DataBinding.FieldName = 'TIP_DOCUMENT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.ImmediatePost = True
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
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Editing = False
            Width = 121
          end
          object GridOPDATA: TcxGridDBColumn
            Caption = 'Data Nota'
            DataBinding.FieldName = 'DATA'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikMask
            Properties.ReadOnly = False
            Properties.SaveTime = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 94
          end
          object GridOPEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.CharCase = ecLowerCase
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 93
          end
          object GridOPVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.Nullable = False
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 53
          end
          object GridOPCONTD: TcxGridDBColumn
            Caption = 'Debit'
            DataBinding.FieldName = 'CONTD'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 60
          end
          object GridOPREPARTITOR_DEBIT: TcxGridDBColumn
            Caption = 'Repartitor Debit'
            DataBinding.FieldName = 'REPARTITOR_DEBIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.IncrementalFiltering = True
            Properties.Items = <>
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Options.Editing = False
            Width = 24
          end
          object GridOPCONTC: TcxGridDBColumn
            Caption = 'Credit'
            DataBinding.FieldName = 'CONTC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 60
          end
          object GridOPREPARTITOR_CREDIT: TcxGridDBColumn
            Caption = 'Repartitor Credit'
            DataBinding.FieldName = 'REPARTITOR_CREDIT'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DropDownRows = 7
            Properties.IncrementalFiltering = True
            Properties.Items = <>
            HeaderAlignmentHorz = taCenter
            MinWidth = 16
            Width = 24
          end
          object GridOPCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Cod Funct.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxPopupEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.ImmediateDropDownWhenKeyPressed = False
            Properties.PopupControl = cxTreeFunctional
            Properties.PopupSysPanelStyle = True
            Properties.OnCloseUp = GridOPCOD_FUNCTIONALPropertiesCloseUp
            Properties.OnPopup = GridOPCOD_FUNCTIONALPropertiesPopup
            HeaderAlignmentHorz = taCenter
            Width = 60
          end
          object GridOPCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Cod Ec.'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxPopupEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.ImmediateDropDownWhenKeyPressed = False
            Properties.PopupControl = cxTreeEconomic
            Properties.PopupSysPanelStyle = True
            Properties.OnCloseUp = GridOPCOD_ECONOMICPropertiesCloseUp
            Properties.OnPopup = GridOPCOD_ECONOMICPropertiesPopup
            HeaderAlignmentHorz = taCenter
            Width = 44
          end
          object GridOPNR_OP: TcxGridDBColumn
            Caption = 'Nr. Unic'
            DataBinding.FieldName = 'NR_OP'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 40
          end
          object GridOPDATA_OP: TcxGridDBColumn
            Caption = 'Data Fact.'
            DataBinding.FieldName = 'DATA_OP'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            Properties.ReadOnly = False
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 29
          end
          object GridOPTIP_NOTA: TcxGridDBColumn
            Caption = 'Tip Nota'
            DataBinding.FieldName = 'TIP_NOTA'
            PropertiesClassName = 'TcxTextEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = False
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 46
          end
          object GridOPNR: TcxGridDBColumn
            DataBinding.FieldName = 'NR'
            Visible = False
          end
        end
        object cxGridOPL: TcxGridLevel
          GridView = GridOP
        end
      end
      object BtnAdaugaFactura: TcxButton
        Left = 386
        Top = 40
        Width = 73
        Height = 22
        Caption = 'Adauga'
        LookAndFeel.Kind = lfOffice11
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
          F800F8F8F800E7E7E7FFB1B7B1FF809980FF679467FF679467FF839C83FFB3BB
          B3FFE7E8E7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F6F6
          F6FFBDC3BDFF579058FF20B523FF09DA0EFF01E907FF01E907FF09DB0EFF1CBB
          1FFF519753FFBDC5BDFFF6F6F6FFF8F8F800F8F8F800F8F8F800F7F7F7FFAEB7
          AEFF359C38FF03E00AFF00F308FF01F609FF16E21CFF16E01CFF08EE10FF00F4
          08FF01E609FF2CA52FFFADBCAEFFF8F8F800F8F8F800F8F8F800D0D4D0FF3B96
          3CFF02DD0AFF00F408FF00F808FF0AE912FFD3DAD3FFCECECEFF45CE49FF00F8
          08FF00F508FF00E408FF2FA332FFCED5CEFFF8F8F800F5F5F5FF769C76FF06CA
          0DFF00EB08FF00F808FF00F808FF0BE912FFE7F0E7FFEAEAEAFF48CF4BFF00F8
          08FF00F808FF00EE08FF02D30AFF659F66FFF4F4F4FFDDDFDDFF309A32FF00DA
          08FF00EF08FF00F808FF00F808FF0BE912FFE7F0E7FFEAEAEAFF48CF4BFF00F8
          08FF00F808FF00F308FF00DF08FF1FA522FFD4DBD4FFB9C5B9FF15A719FF00DB
          08FF46D64AFF82C684FF80C282FF88C589FFECEEECFFF0F0F0FF9FC59FFF80C2
          82FF80C282FF68BB6CFF00DB08FF0AB60FFFA9BEA9FFB0C1B0FF0EAA13FF00D6
          08FF77DF7AFFF4F4F4FFF4F4F4FFF4F4F4FFF7F7F7FFF8F8F800F4F4F4FFF4F4
          F4FFF3F3F3FFB5C9B5FF00D307FF02B808FF90B490FFB4C4B4FF0EA213FF00CC
          08FF4FD753FFA5E4A6FFA5E6A6FFA8E2A9FFF2F5F2FFF5F5F5FFC1E5C1FFA5E6
          A6FFA5E4A6FF87D589FF00CC08FF02AF08FF93B593FFCCD5CCFF18951AFF04BF
          0CFF01D709FF00EB08FF00F508FF0BE911FFE7F0E7FFEBEBEBFF49D34DFF00F6
          08FF00EE08FF00DC08FF05C30CFF0C9E0FFFB3C6B3FFF1F2F1FF459345FF0CAE
          11FF0CC712FF00D908FF00E708FF0BE012FFE7F0E7FFEAEAEAFF48CB4BFF00E9
          08FF00DC08FF08CA0FFF10B414FF269027FFE0E6E0FFF8F8F800A6BDA6FF1296
          14FF2EBB32FF08C210FF00D008FF0BCE12FFE7EFE7FFF2F2F2FF4EC752FF00D2
          08FF05C50DFF2BBF2FFF129F14FF81AB81FFF8F8F800F8F8F800F2F4F2FF659C
          65FF2DA42FFF4AC04EFF15BA1CFF05BB0BFF23B926FF25B928FF10B915FF13BA
          16FF43BF47FF36AC39FF479347FFE8ECE8FFF8F8F800F8F8F800F8F8F800E8EC
          E8FF659C65FF45A546FF80CB81FF65C767FF49BF4CFF49BF4BFF64C665FF83CD
          85FF4EAD4FFF4D944DFFD9E2D9FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F2F4F2FFA5BFA5FF579A57FF68AA6AFF83BD84FF84BD84FF6DAD6DFF5198
          51FF92B493FFECF0ECFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F2F4F2FFCFDBCFFFB4C8B4FFB3C8B3FFCDD9CDFFF0F3
          F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
        TabOrder = 7
        OnClick = BtnAdaugaFacturaClick
      end
      object BtnRemoveFactura: TcxButton
        Left = 466
        Top = 40
        Width = 68
        Height = 22
        Caption = 'Sterge'
        LookAndFeel.Kind = lfOffice11
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
        TabOrder = 8
        OnClick = BtnRemoveFacturaClick
      end
      object BtnAnulareOP: TcxButton
        Left = 541
        Top = 40
        Width = 87
        Height = 22
        Caption = 'Anulare OP'
        LookAndFeel.Kind = lfOffice11
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
          88FF0707B8FF0000E1FF0000EEFF0000F6FF0000F8FF0000F8FF0000F6FF0000
          EFFF0000E3FF0505BEFF333386FFE1E1EAFFF8F8F800F8F8F8007B7BABFF0C0C
          A4FF0000D9FF0000E9FF0000F2FF0000F8FF0000F8FF0000F8FF0000F8FF0000
          F3FF0000EBFF0000DBFF0909ADFF68689EFFF7F7F8FFE5E5EDFF242483FF0000
          C4FF0000DDFF0000EBFF0000F2FF0000F8FF0000F8FF0000F8FF0000F8FF0000
          F3FF0000EBFF0000DEFF0000C9FF1B1B84FFD9D9E5FFB4B4CEFF111190FF0000
          C6FF4B4BC4FF5050C5FF5050CAFF5050CCFF5050CCFF5050CCFF5050CCFF5050
          CAFF5050C6FF4545BFFF0000C8FF0E0E95FFA4A4C4FF8C8CB4FF0C0C97FF0101
          BFFFD9D9EBFFF0F0F0FFF0F0F0FFF0F0F0FFF0F0F0FFF0F0F0FFF0F0F0FFF0F0
          F0FFF0F0F0FFBDBDCFFF0000C1FF09099DFF8787B2FF8B8BB4FF0C0C93FF0101
          B7FFDCDCEDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800C3C3D4FF0000BAFF09099AFF8787B2FFB2B2CDFF111188FF0000
          B1FF8080D0FF8F8FD9FF8B8BDBFF8B8BDEFF8B8BDFFF8B8BDFFF8B8BDEFF8B8B
          DBFF8E8ED9FF7D7DCDFF0000B3FF0E0E8DFFA3A3C3FFE5E5ECFF23237FFF0000
          A8FF1B1BBDFF1C1CC6FF0606C6FF0000CBFF0000CFFF0000CFFF0000CCFF0505
          C7FF1A1AC7FF1C1CBEFF0000AAFF1C1C7FFFD9D9E4FFF8F8F8007A7AA9FF0C0C
          8EFF1919B2FF4C4CC9FF2929C3FF0D0DC0FF0404C1FF0303C1FF0D0DC0FF2828
          C2FF4C4CC9FF1C1CB4FF080893FF65659DFFF7F7F8FFF8F8F800E9E9EFFF3C3C
          86FF080895FF4F4FC0FF8383D5FF6C6CD0FF5555CBFF5454CAFF6969CFFF8282
          D5FF5353C2FF070797FF323281FFDFDFE9FFF8F8F800F8F8F800F8F8F800D8D8
          E5FF404088FF12128BFF4F4FB9FFA4A4DBFFBFBFE4FFC0C0E4FFA7A7DDFF5454
          BCFF13138FFF373784FFCDCDDDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800EDEDF2FF8484AFFF2E2E82FF1A1A85FF2E2E95FF2F2F95FF1B1B86FF2929
          80FF7A7AAAFFE6E6EDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800EBEBF0FFBCBCD3FF9191B7FF8D8DB6FFB7B7D1FFE8E8
          EEFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
        TabOrder = 9
        OnClick = BtnAnulareOPClick
      end
      object edContDebit: TcxPopupEdit
        Left = 64
        Top = 40
        Properties.PopupControl = cxTreeDebit
        Properties.OnCloseUp = edContDebitPropertiesCloseUp
        Properties.OnPopup = edContDebitPropertiesPopup
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 3
        Width = 121
      end
      object edContCredit: TcxPopupEdit
        Left = 256
        Top = 40
        Properties.PopupControl = cxTreeCredit
        Properties.OnCloseUp = edContCreditPropertiesCloseUp
        Properties.OnPopup = edContCreditPropertiesPopup
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 4
        Width = 121
      end
      object edDataOrdin: TcxDateEdit
        Left = 279
        Top = 16
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        Properties.OnChange = edDataOrdinPropertiesChange
        Properties.OnEditValueChanged = edDataOrdinPropertiesEditValueChanged
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        Width = 89
      end
      object edNrOrdin: TcxImageComboBox
        Left = 80
        Top = 16
        Properties.Items = <>
        Properties.OnChange = edNrOrdinPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 0
        Width = 129
      end
      object edJurnal: TcxImageComboBox
        Left = 424
        Top = 16
        Anchors = [akLeft, akTop, akRight]
        Properties.Items = <>
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        Width = 818
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
            Value = '0'
          end
          item
            Description = 'Grupat Functional'
            Value = '1'
          end
          item
            Description = 'Grupat Economic'
            Value = '2'
          end>
        Properties.OnChange = edGrupGridOpPropertiesChange
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 5
        Width = 200
      end
    end
    object Splitter1: TcxSplitter
      Left = 4
      Top = 213
      Width = 930
      Height = 8
      Cursor = crVSplit
      HotZoneClassName = 'TcxMediaPlayer9Style'
      AlignSplitter = salTop
      AutoSnap = True
      Control = GRTop
    end
  end
  object cxTreeEconomic: TcxDBTreeList
    Left = 48
    Top = 304
    Width = 250
    Height = 150
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = frmData.DTBGPlanEconomic
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_ECONOMIC'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ImmediateEditor = False
    OptionsBehavior.ConfirmDelete = False
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.CancelOnExit = False
    OptionsData.Editing = False
    OptionsData.AnsiSort = True
    OptionsData.CaseInsensitive = True
    OptionsData.Deleting = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 1
    Visible = False
    OnDblClick = cxTreeDebitDblClick
    OnKeyDown = cxTreeDebitKeyDown
    object cxTreeEconomicDESCRIERE: TcxDBTreeListColumn
      Caption.Text = 'Buget'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeEconomicDESCRIEREGetDisplayText
    end
    object cxTreeEconomicDENUMIRE: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Denumire'
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicCOD_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_ECONOMIC'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxTreeFunctional: TcxDBTreeList
    Left = 104
    Top = 304
    Width = 250
    Height = 150
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = frmData.DTBGPlanFunctional
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ImmediateEditor = False
    OptionsBehavior.ConfirmDelete = False
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.CancelOnExit = False
    OptionsData.Editing = False
    OptionsData.AnsiSort = True
    OptionsData.CaseInsensitive = True
    OptionsData.Deleting = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 2
    Visible = False
    OnDblClick = cxTreeDebitDblClick
    OnKeyDown = cxTreeDebitKeyDown
    object cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn
      Caption.Text = 'Buget'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeFunctionalDESCRIEREGetDisplayText
    end
    object cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Denumire'
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCOD_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxTreeCredit: TcxDBTreeList
    Left = 168
    Top = 320
    Width = 250
    Height = 150
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = DTCredit
    DataController.ParentField = 'PARINTE'
    DataController.KeyField = 'CONT'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.GoToNextCellOnTab = True
    OptionsBehavior.AutoDragCopy = True
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.Editing = False
    OptionsData.AnsiSort = True
    OptionsData.CaseInsensitive = True
    OptionsData.Deleting = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 3
    Visible = False
    OnDblClick = cxTreeDebitDblClick
    OnKeyDown = cxTreeDebitKeyDown
    object cxTreeCreditCONT: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Cont'
      DataBinding.FieldName = 'CONT'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeCreditROMANA: TcxDBTreeListColumn
      Caption.Text = 'Explicatie'
      DataBinding.FieldName = 'ROMANA'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeCreditROMANAGetDisplayText
    end
  end
  object cxTreeDebit: TcxDBTreeList
    Left = 272
    Top = 328
    Width = 250
    Height = 150
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = DTDebit
    DataController.ParentField = 'PARINTE'
    DataController.KeyField = 'CONT'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.GoToNextCellOnTab = True
    OptionsBehavior.AutoDragCopy = True
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.Editing = False
    OptionsData.AnsiSort = True
    OptionsData.CaseInsensitive = True
    OptionsData.Deleting = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 4
    Visible = False
    OnDblClick = cxTreeDebitDblClick
    OnKeyDown = cxTreeDebitKeyDown
    object cxTreeDebitCONT: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Cont'
      DataBinding.FieldName = 'CONT'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeDebitROMANA: TcxDBTreeListColumn
      Caption.Text = 'Explicatie'
      DataBinding.FieldName = 'ROMANA'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeDebitROMANAGetDisplayText
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
        DataType = ftUnknown
        Name = 'START_DEBIT'
        ParamType = ptUnknown
      end>
    Left = 122
    Top = 114
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'START_DEBIT'
        ParamType = ptUnknown
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
        DataType = ftUnknown
        Name = 'START_CREDIT'
        ParamType = ptUnknown
      end>
    Left = 122
    Top = 146
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'START_CREDIT'
        ParamType = ptUnknown
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
    AfterOpen = QryOPHAfterOpen
    SQL.Strings = (
      'SELECT * FROM CNOTE'
      ''
      'ORDER BY NR')
    Params = <>
    Left = 122
    Top = 178
  end
  object pmGridIstoricNote: TcxGridPopupMenu
    Grid = cxGridIstoricNote
    PopupMenus = <>
    Left = 496
    Top = 112
  end
  object pmGridOP: TcxGridPopupMenu
    Grid = cxGridOP
    PopupMenus = <>
    Left = 648
    Top = 312
  end
end
