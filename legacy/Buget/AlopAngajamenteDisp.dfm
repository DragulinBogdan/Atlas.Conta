object frmAlopAngajamenteDisp: TfrmAlopAngajamenteDisp
  Left = 273
  Top = 155
  Caption = 'Introducere Angajament'
  ClientHeight = 604
  ClientWidth = 1014
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnActivate = FormActivate
  OnClose = FormClose
  OnCreate = FormCreate
  OnDblClick = btnAnuleazaAngClick
  OnDestroy = FormDestroy
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object pnClient: TPanel
    Left = 0
    Top = 175
    Width = 1014
    Height = 337
    Align = alClient
    TabOrder = 1
    object gridAngajamentDetaliu: TcxGrid
      Left = 1
      Top = 1
      Width = 1012
      Height = 335
      Align = alClient
      BevelInner = bvNone
      BevelOuter = bvNone
      TabOrder = 0
      LookAndFeel.Kind = lfOffice11
      ExplicitLeft = 154
      ExplicitTop = -137
      object viewAngajamentDetaliu: TcxGridDBBandedTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        OnFocusedRecordChanged = viewAngajamentDetaliuFocusedRecordChanged
        DataController.DataSource = DTAngajamente
        DataController.Filter.MaxValueListCount = 1000
        DataController.KeyFieldNames = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
        DataController.Options = [dcoAssignGroupingValues, dcoAssignMasterDetailKeys, dcoSaveExpanding, dcoImmediatePost]
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <
          item
            Kind = skSum
            Column = viewAngajamentDetaliuANGAJAT
          end
          item
            Kind = skSum
            Column = viewAngajamentDetaliuANGAJAT_VALUTA
          end>
        DataController.Summary.SummaryGroups = <>
        Filtering.ColumnPopup.MaxDropDownItemCount = 12
        OptionsBehavior.GoToNextCellOnEnter = True
        OptionsData.Appending = True
        OptionsData.CancelOnExit = False
        OptionsData.DeletingConfirmation = False
        OptionsSelection.MultiSelect = True
        OptionsSelection.HideFocusRectOnExit = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.ExpandButtonsForEmptyDetails = False
        OptionsView.Footer = True
        OptionsView.GroupByBox = False
        OptionsView.GroupFooters = gfVisibleWhenExpanded
        OptionsView.Indicator = True
        OptionsView.FixedBandSeparatorWidth = 1
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        Styles.Background = cxStyle5
        Styles.OnGetContentStyle = viewAngajamentDetaliuStylesGetContentStyle
        Bands = <
          item
            Caption = 'Proiect'
            FixedKind = fkLeft
            Width = 247
          end
          item
            Caption = 'Buget'
            FixedKind = fkLeft
            Width = 387
          end
          item
            Caption = 'Anterior'
            Width = 138
          end
          item
            Caption = 'Curent'
            Width = 225
          end>
        object viewAngajamentDetaliuid_oi_proiecte: TcxGridDBBandedColumn
          Caption = 'Proiect'
          DataBinding.FieldName = 'ID_OI_PROIECTE'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <>
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 85
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliueste_procentual: TcxGridDBBandedColumn
          Caption = 'Este %'
          DataBinding.FieldName = 'este_procentual'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Position.BandIndex = 0
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuprocProiect: TcxGridDBBandedColumn
          Caption = '%'
          DataBinding.FieldName = 'procProiect'
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Position.BandIndex = 0
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuangProiect: TcxGridDBBandedColumn
          Caption = 'Val Proiect'
          DataBinding.FieldName = 'angProiect'
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Position.BandIndex = 0
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuBUGET: TcxGridDBBandedColumn
          Caption = 'Titlu/Art/Alin'
          DataBinding.FieldName = 'COD_ECRAN'
          PropertiesClassName = 'TcxPopupEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.PopupSysPanelStyle = True
          Properties.PopupWidth = 640
          Properties.ReadOnly = True
          Properties.OnCloseUp = viewAngajamentDetaliuBUGETPropertiesCloseUp
          Properties.OnInitPopup = viewAngajamentDetaliuBUGETPropertiesInitPopup
          Properties.OnPopup = viewAngajamentDetaliuBUGETPropertiesPopup
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 99
          Position.BandIndex = 1
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuDESC_BUGET: TcxGridDBBandedColumn
          Caption = 'Desc. Ttitlu/Art/Alin'
          DataBinding.FieldName = 'COD_ECRAN'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          OnGetDisplayText = viewAngajamentDetaliuDESC_BUGETGetDisplayText
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 135
          Position.BandIndex = 1
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuDESCRIERE: TcxGridDBBandedColumn
          Caption = 'Descriere'
          DataBinding.FieldName = 'DESCRIERE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = False
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 135
          Position.BandIndex = 1
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuAPROBATE: TcxGridDBBandedColumn
          Caption = 'Aprobate'
          DataBinding.FieldName = 'APROBATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 78
          Position.BandIndex = 1
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuTOTAL_ANGAJATE: TcxGridDBBandedColumn
          Caption = 'Total Ang'
          DataBinding.FieldName = 'TOTAL_ANGAJATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 79
          Position.BandIndex = 2
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuDISPONIBIL: TcxGridDBBandedColumn
          Caption = 'Disponibil'
          DataBinding.FieldName = 'DISPONIBIL'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 79
          Position.BandIndex = 2
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuID_VALUTA: TcxGridDBBandedColumn
          Caption = 'Valuta'
          DataBinding.FieldName = 'ID_VALUTA'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.ImmediatePost = True
          Properties.Items = <>
          Properties.ReadOnly = False
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Options.Filtering = False
          Width = 42
          Position.BandIndex = 3
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuANGAJAT_VALUTA: TcxGridDBBandedColumn
          Caption = 'Ang. Valuta'
          DataBinding.FieldName = 'ANGAJAT_VALUTA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = False
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 60
          Position.BandIndex = 3
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuCURS_VALUTAR: TcxGridDBBandedColumn
          Caption = 'Curs'
          DataBinding.FieldName = 'CURS_VALUTAR'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.0000;-,0.0000'
          Properties.Nullable = False
          Properties.ReadOnly = False
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 40
          Position.BandIndex = 3
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuANGAJAT: TcxGridDBBandedColumn
          Caption = 'Angajat'
          DataBinding.FieldName = 'ANGAJAT'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 43
          Position.BandIndex = 3
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuRAMAS_DE_ANGAJAT: TcxGridDBBandedColumn
          Caption = 'Sold'
          DataBinding.FieldName = 'RAMAS_DE_ANGAJAT'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 4
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 40
          Position.BandIndex = 3
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object viewAngajamentDetaliuVALIDAT: TcxGridDBBandedColumn
          Caption = 'Validat'
          DataBinding.FieldName = 'VALIDAT'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          Properties.ShowDescriptions = False
          Visible = False
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Options.Editing = False
          Options.Filtering = False
          Width = 48
          Position.BandIndex = 3
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
      end
      object nivelAngajamentDetaliu: TcxGridLevel
        GridView = viewAngajamentDetaliu
      end
    end
  end
  object pnDocument: TPanel
    Left = 0
    Top = 0
    Width = 1014
    Height = 175
    Align = alTop
    TabOrder = 0
    OnResize = pnDocumentResize
    DesignSize = (
      1014
      175)
    object lbDocument: TLabel
      Left = 434
      Top = 3
      Width = 73
      Height = 13
      Anchors = [akTop]
      Caption = 'Nr. Angajament'
    end
    object LbDataNota: TLabel
      Left = 660
      Top = 3
      Width = 51
      Height = 13
      Anchors = [akTop]
      Caption = 'Din Data : '
    end
    object LbPredator: TLabel
      Left = 9
      Top = 3
      Width = 120
      Height = 13
      Caption = 'Departament Angajament'
    end
    object LbPrimitor: TLabel
      Left = 959
      Top = 3
      Width = 47
      Height = 13
      Anchors = [akTop, akRight]
      Caption = 'Beneficiar'
    end
    object Label1: TLabel
      Left = 9
      Top = 90
      Width = 117
      Height = 13
      Caption = 'Clasificatie Functionala : '
    end
    object Label2: TLabel
      Left = 9
      Top = 65
      Width = 83
      Height = 13
      Caption = 'Tip Angajament : '
    end
    object lbContract: TLabel
      Left = 416
      Top = 43
      Width = 71
      Height = 13
      Caption = 'Detalii contract'
    end
    object Label3: TLabel
      Left = 10
      Top = 42
      Width = 72
      Height = 13
      Caption = 'Detalii rectificat'
    end
    object LbScopul: TLabel
      Left = 676
      Top = 3
      Width = 46
      Height = 13
      Anchors = [akTop]
      Caption = 'Nr proiect'
      Visible = False
    end
    object Label4: TLabel
      Left = 9
      Top = 114
      Width = 34
      Height = 13
      Caption = 'Scop : '
    end
    object lbLegal: TLabel
      Left = 306
      Top = 66
      Width = 40
      Height = 13
      Caption = 'in baza :'
    end
    object lbNumar: TLabel
      Left = 737
      Top = 41
      Width = 66
      Height = 13
      Anchors = [akTop, akRight]
      Caption = 'Detalii Dosar :'
    end
    object edtDetaliiContract: TcxPopupEdit
      Left = 493
      Top = 38
      Hint = 'Detalile depre contractul de investitii'
      Anchors = [akLeft, akTop, akRight]
      Properties.HideSelection = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupMinHeight = 350
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = edtDetaliiContractPropertiesCloseUp
      Properties.OnInitPopup = edtDetaliiContractPropertiesInitPopup
      Properties.OnPopup = edtDetaliiContractPropertiesPopup
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 5
      OnEnter = edPredatorEnter
      OnKeyDown = edtDetaliiContractKeyDown
      Width = 238
    end
    object edPredator: TcxPopupEdit
      Left = 9
      Top = 15
      Hint = 
        'Departamentul/Biroul care reprezinta partea tehnica a angajament' +
        'ului'
      AutoSize = False
      ParentFont = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = edPredatorPropertiesCloseUp
      Properties.OnInitPopup = edPredatorPropertiesInitPopup
      Properties.OnPopup = edPredatorPropertiesPopup
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      OnEnter = edPredatorEnter
      OnKeyDown = edPredatorKeyDown
      Height = 21
      Width = 192
    end
    object edPrimitor: TcxPopupEdit
      Left = 757
      Top = 15
      Hint = 'Furnizorul/entitatea externa cu care se incheie angajamentul'
      Anchors = [akTop]
      AutoSize = False
      ParentFont = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = edPredatorPropertiesCloseUp
      Properties.OnInitPopup = edPredatorPropertiesInitPopup
      Properties.OnPopup = edPredatorPropertiesPopup
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 4
      OnEnter = edPredatorEnter
      OnKeyDown = edPredatorKeyDown
      Height = 21
      Width = 251
    end
    object edDataDoc: TcxDateEdit
      Left = 650
      Top = 16
      Hint = 'Data Angajamentului'
      Anchors = [akTop]
      Properties.DateOnError = deNull
      Properties.ImmediatePost = True
      Properties.InputKind = ikMask
      Properties.SaveTime = False
      Properties.ShowTime = False
      Properties.OnEditValueChanged = edDataDocPropertiesEditValueChanged
      Properties.OnValidate = edDataDocPropertiesValidate
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 2
      Width = 89
    end
    object edTipAngajament: TcxImageComboBox
      Left = 128
      Top = 63
      Hint = 'Tipul de angajament'
      Properties.Items = <>
      Properties.OnChange = edTipAngajamentPropertiesChange
      Properties.OnValidate = edTipAngajamentPropertiesValidate
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 3
      Width = 169
    end
    object edNumarDoc: TcxButtonEdit
      Left = 430
      Top = 16
      Hint = 'Numarul de angajament'
      Anchors = [akTop]
      Properties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      Properties.OnButtonClick = edNumarDocPropertiesButtonClick
      Properties.OnEditValueChanged = edNumarDocPropertiesEditValueChanged
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 1
      OnKeyDown = edNumarDocKeyDown
      Width = 89
    end
    object edRectificat: TcxButtonEdit
      Left = 128
      Top = 38
      Properties.Buttons = <
        item
          Default = True
          Hint = 'Adaugare rectificat'
          Kind = bkEllipsis
        end
        item
          Caption = 'X'
          Hint = 'Stergere rectificat'
          Kind = bkText
        end>
      Properties.OnButtonClick = edRectificatPropertiesButtonClick
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 6
      Width = 284
    end
    object cxFunctionalBar: TcxPopupEdit
      Left = 128
      Top = 86
      Anchors = [akLeft, akTop, akRight]
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 450
      Properties.PopupMinHeight = 450
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = cxFunctionalBarPropertiesCloseUp
      Properties.OnInitPopup = cxFunctionalBarPropertiesPopup
      Properties.OnPopup = cxFunctionalBarPropertiesPopup
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 7
      Width = 880
    end
    object edNrProiect: TcxButtonEdit
      Left = 662
      Top = 14
      Hint = 'Numar proiect de angajament'
      Anchors = [akTop]
      Properties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      Properties.OnButtonClick = edNrProiectPropertiesButtonClick
      Properties.OnEditValueChanged = edNrProiectPropertiesEditValueChanged
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 8
      Visible = False
      OnKeyDown = edNumarDocKeyDown
      Width = 89
    end
    object edtScop: TcxTextEdit
      Left = 128
      Top = 109
      Hint = 'Descriere Angajament'
      Anchors = [akLeft, akTop, akRight]
      AutoSize = False
      Properties.OnEditValueChanged = edtScopPropertiesEditValueChanged
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 9
      OnKeyDown = aKeyDown
      Height = 21
      Width = 880
    end
    object edLegal: TcxButtonEdit
      Left = 352
      Top = 62
      Anchors = [akLeft, akTop, akRight]
      Properties.Buttons = <
        item
          Default = True
          Hint = 'Adaugare rectificat'
          Kind = bkEllipsis
        end
        item
          Caption = 'X'
          Hint = 'Stergere rectificat'
          Kind = bkText
        end>
      Properties.OnButtonClick = edLegalPropertiesButtonClick
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 10
      Width = 656
    end
    object btnAdaugaPozitii: TcxButton
      Left = 8
      Top = 136
      Width = 140
      Height = 30
      Action = Cmd_AdaugaDetaliuAngajament
      Anchors = [akLeft, akBottom]
      LookAndFeel.Kind = lfOffice11
      TabOrder = 11
    end
    object cxButton2: TcxButton
      Left = 153
      Top = 136
      Width = 140
      Height = 30
      Action = Cmd_StergeDetaliuAngajament
      Anchors = [akLeft, akBottom]
      LookAndFeel.Kind = lfOffice11
      TabOrder = 12
    end
    object edSumaProiect: TcxCurrencyEdit
      Left = 397
      Top = 139
      Properties.Alignment.Horz = taRightJustify
      Properties.DisplayFormat = ',0.00;-,0.00'
      Properties.UseDisplayFormatWhenEditing = True
      Properties.UseLeftAlignmentOnEditing = False
      Properties.UseThousandSeparator = True
      Properties.OnValidate = edSumaProiectPropertiesValidate
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 13
      Visible = False
      OnKeyUp = edSumaProiectKeyUp
      Width = 121
    end
    object lbSumaProiect: TcxLabel
      Left = 304
      Top = 141
      Caption = 'Suma Pe Proiect'
      Transparent = True
      Visible = False
    end
    object chkRectificareSold: TcxCheckBox
      Left = 864
      Top = 142
      Anchors = [akTop, akRight]
      Caption = 'Rectificare Sold Initial'
      Properties.OnEditValueChanged = cxCheckBox1PropertiesEditValueChanged
      TabOrder = 15
      Visible = False
    end
    object edtDetaliiDosar: TcxPopupEdit
      Left = 808
      Top = 38
      Hint = 'Detalii despre dosarul asociat angajamentului'
      Anchors = [akTop, akRight]
      Properties.HideSelection = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupMinHeight = 350
      Properties.PopupMinWidth = 500
      Properties.PopupSysPanelStyle = True
      Properties.PopupWidth = 500
      Properties.OnCloseUp = edtDetaliiDosarPropertiesCloseUp
      Properties.OnInitPopup = edtDetaliiContractPropertiesInitPopup
      Properties.OnPopup = edtDetaliiDosarPropertiesPopup
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 16
      OnEnter = edPredatorEnter
      OnKeyDown = edtDetaliiDosarKeyDown
      Width = 198
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 512
    Width = 1014
    Height = 92
    Align = alBottom
    TabOrder = 2
    OnResize = pnBottomResize
    DesignSize = (
      1014
      92)
    object BtnOk: TcxButton
      Left = 660
      Top = 6
      Width = 83
      Height = 30
      Anchors = [akRight, akBottom]
      Caption = 'Salvare'
      LookAndFeel.Kind = lfOffice11
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
    object btnCancel: TcxButton
      Left = 892
      Top = 6
      Width = 75
      Height = 30
      Hint = 'Inchide ecran'
      Anchors = [akRight, akBottom]
      Caption = 'Inchide'
      LookAndFeel.Kind = lfOffice11
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
      OnClick = btnCancelClick
    end
    object BtnModificare: TcxButton
      Left = 278
      Top = 5
      Width = 134
      Height = 30
      Hint = 'Modificare unui angajament deja salvat'
      Anchors = [akLeft, akBottom]
      Caption = 'Modifica Angajament'
      LookAndFeel.Kind = lfOffice11
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360900000000000036000000280000001800000018000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00EFEFEFFFC6C6C6FFE7E7E7FFFFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CECECEFF8C8C8CFF4A4A4AFFB5B5B5FFFFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DEE7
        E7FF9C9CA5FF525A5AFF292929FF525252FFEFEFEFFFFFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00B5BDBDFF7B7B
        7BFFA59494FF635A52FF212121FFD6D6D6FFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00D6D6D6FF8C9494FF737373FFAD94
        8CFFFFE7DEFFD6B5A5FFA5A59CFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00B5B5B5FF737373FF8C847BFFCEB5ADFFFFE7
        DEFFDEC6BDFFC6B5ADFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00CECECEFF313939FF524A4AFFEFCEBDFFF7DECEFFEFCE
        C6FFA59484FFDEDEDEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CECEC6FF424A4AFF101821FFB58C84FFF7D6C6FFF7DED6FFB594
        8CFFBDBDB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00C6C6C6FF424A4AFF081818FFC69C94FFFFC6B5FFFFCEADFF9C7B73FF8C7B
        7BFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00CECE
        CEFF424A4AFF081821FFB59C94FFFFCEB5FFFFCEB5FFA57363FF212121FFBDBD
        BDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00CECECEFF4242
        4AFF102121FFC69C8CFFFFC6B5FFFFCEB5FF9C7363FF000810FFBDC6CEFFFFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00C6C6C6FF394242FF1018
        18FFCE9C94FFFFCEBDFFFFCEB5FF735A42FF102121FFCED6DEFFFFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00CECECEFF394A4AFF081818FFB59C
        94FFFFD6BDFFFFCEADFF7B5A4AFF212931FFBDC6C6FFFFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00CEC6C6FF394242FF102121FFCE9C8CFFFFCE
        B5FFFFCEADFF735239FF182129FFDEE7E7FFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00C6BDBDFF313939FF101818FFCE9C94FFFFD6BDFFFFC6
        ADFF523929FF293942FFEFEFEFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00F7F7F7FFBDBDBDFF313939FF102121FFC69C94FFFFDEBDFFFFC6ADFF5A42
        31FF394A52FFE7EFEFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00E7E7
        E7FF949494FF293131FF182929FFD6AD9CFFFFD6B5FFFFC6A5FF4A3129FF394A
        52FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00EFEFEFFF8484
        84FF182121FF293931FFDEB59CFFFFDEB5FFFFC6A5FF422929FF42525AFFFFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7F7F7FF848484FF4A52
        52FF8C8C8CFFDEA58CFFFFD6ADFFF7C6ADFF423129FF525A63FFFFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00ADADB5FF393942FFADAD
        ADFFF7EFF7FFEFBDADFFF7B594FF392929FF394A52FFFFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF009C947BFF948C8CFFCEC6
        CEFFDED6D6FFFFFFFF005A4A4AFF313939FFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DEC69CFFB58C63FFC6CE
        DEFFE7E7E7FF7B7373FF63636BFFE7EFEFFFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFF7FFDEAD6BFFBD94
        6BFF636363FF6B6B6BFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00DEBD
        A5FFC6B5ADFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
      OptionsImage.Spacing = 0
      TabOrder = 2
      OnClick = BtnModificareClick
    end
    object btnAnuleazaAng: TcxButton
      Left = 132
      Top = 6
      Width = 140
      Height = 30
      Hint = 'Anularea angajamentului din ecran'
      Anchors = [akLeft, akBottom]
      Caption = 'Anuleaza Angajament'
      LookAndFeel.Kind = lfOffice11
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
      TabOrder = 3
      OnClick = btnAnuleazaAngClick
    end
    object btnNewAng: TcxButton
      Left = 8
      Top = 6
      Width = 118
      Height = 30
      Hint = 'Anularea angajamentului din ecran'
      Anchors = [akLeft, akBottom]
      Caption = 'Angajament Nou'
      LookAndFeel.Kind = lfOffice11
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360900000000000036000000280000001800000018000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00F7F7EFFFEFE7DEFFEFE7E7FFEFE7E7FFEFE7E7FFEFE7E7FFEFE7E7FFEFE7
        E7FFEFE7E7FFFFEFEFFFF7EFEFFFB5CEADFF73A563FF529C4AFF5A944AFF739C
        6BFFBDCEB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00D6B57BFFB56B21FFC67B4AFFC67B42FFC67B42FFC67B42FFC67B42FFBD7B
        42FFE78452FFC67B4AFF316B18FF007B10FF009429FF00AD21FF00B531FF0894
        29FF006300FF52944AFFEFF7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00D6AD73FFD68431FFDEA55AFFDE9C52FFDE9C52FFDE9C52FFD69C52FFF79C
        63FFDE944AFF186B00FF189C4AFF31CE73FF21D67BFF5AC68CFF5ABD84FF31CE
        84FF39CE84FF007B18FF317B31FFF7FFF7FFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00DEB584FFDE9C5AFFE7B57BFFDEB57BFFDEB57BFFDEB57BFFE7B57BFFFFBD
        94FF427B21FF089442FF73E7B5FF21C66BFF39BD73FFFFEFFFFFD6C6CEFF29AD
        63FF4ADE8CFF73DEADFF087B18FF63A55AFFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00DEBD84FFDEA563FFE7BD8CFFE7B58CFFE7B58CFFDEB584FFFFC69CFFBDAD
        6BFF108429FF6BDEA5FF4ACE84FF08BD5AFF31BD73FFFFFFFF00CED6D6FF21AD
        5AFF29C66BFF6BDEA5FF6BD694FF006B00FFCEE7CEFFFFFFFF00FFFFFF00FFFF
        FF00DEBD84FFE7AD7BFFEFC69CFFEFCE94FFEFC694FFEFCE9CFFFFD6ADFF638C
        39FF21A552FF84D6B5FF73AD8CFF52A57BFF6BAD94FFFFF7FFFFD6DEDEFF5AA5
        7BFF63AD84FF73AD94FF84D6ADFF188429FF73AD73FFFFFFFF00FFFFFF00FFFF
        FF00DEBD8CFFEFBD8CFFF7DEADFFF7D6ADFFF7D6ADFFFFD6B5FFFFE7BDFF428C
        29FF10A54AFF9CDEBDFFFFEFFFFFFFEFFFFFFFEFFFFFFFFFFF00FFFFFF00FFEF
        F7FFFFEFFFFFEFC6E7FF84C6A5FF29AD52FF428C39FFFFFFFF00FFFFFF00FFFF
        F7FFDEC694FFF7CE9CFFFFE7BDFFF7DEB5FFF7DEB5FFFFE7BDFFFFEFD6FF4A84
        39FF109C4AFF84DEB5FFDEEFDEFFD6E7DEFFDEEFDEFFFFFFFF00F7F7F7FFDEE7
        E7FFDEEFE7FFDEDEDEFF84D6ADFF29AD4AFF428C42FFFFFFFF00FFFFFF00F7FF
        F7FFE7C694FFFFD6A5FFFFE7CEFFFFE7CEFFFFE7CEFFFFEFCEFFFFFFEFFF84AD
        63FF108C31FF63D6A5FF52BD84FF52BD84FF7BB594FFFFF7FFFFCED6D6FF6BAD
        84FF63C694FF5AC68CFF73DEA5FF088418FF7BB584FFFFFFFF00FFFFFF00FFFF
        F7FFDEC694FFF7DEB5FFFFF7DEFFFFEFD6FFFFF7D6FFFFEFCEFFFFFFEFFFD6DE
        C6FF087308FF4AC684FF63D694FF63C68CFF94CEADFFFFFFFF00D6CED6FF7BB5
        94FF7BD69CFF7BDEA5FF4AC684FF006300FFDEE7DEFFFFFFFF00FFFFFF00FFF7
        F7FFDEC694FFF7E7C6FFFFF7E7FFFFF7D6FFFFF7DEFFFFF7D6FFFFF7DEFFFFFF
        FF00639C52FF007310FF7BEFADFF94D6B5FFB5C6BDFFF7EFF7FFE7DEDEFFA5BD
        B5FFA5DEBDFF7BE7ADFF007310FF73A573FFFFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDEC694FFF7E7C6FFFFFFE7FFFFF7DEFFFFF7DEFFFFF7DEFFFFF7DEFFFFF7
        E7FFF7F7EFFF528C4AFF086B10FF5AC684FF9CD6C6FFB5D6CEFFB5D6C6FF94D6
        B5FF5AC68CFF007310FF528C42FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE94FFF7EFCEFFFFFFEFFFFFF7DEFFFFF7DEFFFFF7DEFFFFF7DEFFFFF7
        DEFFFFFFFF00FFFFF7FF6B9463FF187B18FF108C29FF189439FF189439FF108C
        29FF186B10FF73945AFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFCEFFFFFFEFFFFFF7E7FFFFFFE7FFFFFFE7FFFFFFE7FFFFFF
        E7FFFFF7DEFFFFFFEFFFFFFFFF00DEEFDEFF9CBD94FF63A552FF63A55AFF9CC6
        9CFFDECEADFFFFE7D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFD6FFFFFFEFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFF
        EFFFFFFFEFFFFFFFE7FFFFFFEFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFE7CEFFE7CEB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFD6FFFFFFF7FFFFFFEFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFF
        EFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFFEFFFFFFFF7FFFFFFF7FFFFFF
        FF00EFD6BDFFDECEB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFD6FFFFFFFF00FFFFEFFFFFFFF7FFFFFFF7FFFFFFF7FFFFFF
        F7FFFFFFF7FFFFFFF7FFFFFFF7FFFFFFEFFFFFF7EFFFFFF7EFFFFFFFEFFFFFFF
        FF00EFDEB5FFDECEB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFD6FFFFFFFF00FFFFF7FFFFFFF7FFFFFFF7FFFFFFF7FFFFFF
        F7FFFFFFF7FFFFFFF7FFFFFFF7FFF7F7E7FFE7E7DEFFE7DED6FFE7DED6FFF7EF
        E7FFDECEADFFD6C6ADFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFDEFFFFFFFF00FFFFF7FFFFFFF7FFFFFFF7FFFFFFF7FFFFFF
        F7FFFFFFF7FFFFFFF7FFFFFFF7FFEFEFDEFFC6BDA5FFAD9C84FFB5AD94FFD6CE
        BDFFD6B58CFFDECEB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFDEFFFFFFFF00FFFFF7FFFFFFF7FFFFFFF7FFFFFFF7FFFFFF
        F7FFFFFFF7FFFFFFF7FFFFFFF7FFDEDED6FFD6A56BFFCEA552FFB59C73FFBD94
        5AFFCEB573FFF7F7F7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFF7
        F7FFDECE9CFFF7EFDEFFFFFFFF00FFFFF7FFFFFFF7FFFFFFF7FFFFFFF7FFFFFF
        F7FFFFFFF7FFFFFFF7FFFFF7FFFFD6CEB5FFE7C694FFFFFFE7FFF7DE9CFFD6AD
        6BFFEFEFE7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7F7
        EFFFE7C694FFFFEFDEFFFFFFFF00FFF7F7FFFFF7F7FFFFF7F7FFFFF7F7FFFFF7
        F7FFFFF7F7FFFFF7F7FFFFFFFF00E7D6B5FFE7D6B5FFFFFFE7FFDEC694FFE7DE
        D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        F7FFE7C694FFE7CEA5FFEFE7C6FFEFDEC6FFEFDEC6FFEFDEC6FFEFDEC6FFEFDE
        C6FFEFDEC6FFEFDEC6FFEFDEBDFFE7CEB5FFDEB584FFDEBD8CFFE7E7D6FFFFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFF7F7FFF7EFE7FFF7EFDEFFF7EFDEFFF7EFDEFFF7EFDEFFF7EFDEFFF7EF
        DEFFF7EFDEFFF7EFDEFFF7EFDEFFF7F7E7FFF7EFDEFFF7EFE7FFFFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
      TabOrder = 4
      OnClick = btnNewAngClick
    end
    object btnRapoarte: TcxButton
      Left = 749
      Top = 6
      Width = 137
      Height = 30
      Hint = 'Tiparire rapoarte'
      Anchors = [akRight, akBottom]
      Caption = 'Rapoarte'
      Kind = cxbkDropDown
      LookAndFeel.Kind = lfOffice11
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
      TabOrder = 5
    end
    object btnOkGenerare: TcxButton
      Left = 520
      Top = 6
      Width = 134
      Height = 30
      Anchors = [akRight, akBottom]
      Caption = 'Generare'
      LookAndFeel.Kind = lfOffice11
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
      TabOrder = 6
      Visible = False
    end
  end
  object DTAngajamente: TDataSource
    DataSet = QryAngajamenteDefalcate
    Left = 40
    Top = 225
  end
  object QryAngajamenteDefalcate: TZQuery
    AfterOpen = QryAngajamenteDefalcateAfterOpen
    AfterPost = QryAngajamenteDefalcateAfterPost
    BeforeDelete = QryAngajamenteDefalcateBeforeDelete
    OnNewRecord = QryAngajamenteDefalcateNewRecord
    SQL.Strings = (
      'SELECT *  FROM ALOP_ANGAJAMENTE_DEFALCARE '
      'WHERE ID_ALOP_ANGAJAMENTE = :ID')
    Params = <
      item
        DataType = ftInteger
        Name = 'ID'
        ParamType = ptInputOutput
      end>
    Left = 153
    Top = 225
    ParamData = <
      item
        DataType = ftInteger
        Name = 'ID'
        ParamType = ptInputOutput
      end>
  end
  object QryAngajamente: TZQuery
    OnNewRecord = QryAngajamenteNewRecord
    SQL.Strings = (
      'SELECT * FROM ALOP_ANGAJAMENTE WHERE ID_ALOP_ANGAJAMENTE = :ID')
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
      end>
    Left = 89
    Top = 320
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
      end>
  end
  object cxStyleRepository: TcxStyleRepository
    Left = 184
    Top = 320
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle2: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle4: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clActiveCaption
      TextColor = 11075583
    end
    object cxStyle5: TcxStyle
      AssignedValues = [svColor]
      Color = clBtnFace
    end
  end
  object cxStyleRepository1: TcxStyleRepository
    Left = 288
    Top = 320
    PixelsPerInch = 96
    object cxStyle3: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
    object stilProiectCurent: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clAqua
      TextColor = clNavy
    end
    object stilNormal: TcxStyle
    end
  end
  object ppMenu: TPopupMenu
    Left = 88
    Top = 376
    object ppAdauga: TMenuItem
      Action = Cmd_AdaugaDetaliuAngajament
    end
    object ppSterge: TMenuItem
      Action = Cmd_StergeDetaliuAngajament
    end
  end
  object Actiuni: TActionList
    Left = 184
    Top = 376
    object Cmd_AdaugaDetaliuAngajament: TAction
      Caption = 'Adauga Pozitie Angajament'
      ShortCut = 16429
      OnExecute = Cmd_AdaugaDetaliuAngajamentExecute
    end
    object Cmd_StergeDetaliuAngajament: TAction
      Caption = 'Sterge Pozitii Selectate'
      ShortCut = 16430
      OnExecute = Cmd_StergeDetaliuAngajamentExecute
      OnUpdate = Cmd_StergeDetaliuAngajamentUpdate
    end
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = gridAngajamentDetaliu
    PopupMenus = <
      item
        GridView = viewAngajamentDetaliu
        HitTypes = [gvhtGridNone, gvhtNone, gvhtCell, gvhtRecord, gvhtIndicator, gvhtRowIndicator]
        Index = 0
        PopupMenu = ppMenu
      end>
    Left = 288
    Top = 376
  end
end
