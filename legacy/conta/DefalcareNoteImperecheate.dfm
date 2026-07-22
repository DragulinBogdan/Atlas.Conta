object frmDefalcareNote: TfrmDefalcareNote
  Left = 315
  Top = 113
  Caption = 'Defalcare note imperecheate'
  ClientHeight = 495
  ClientWidth = 725
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  DesignSize = (
    725
    495)
  PixelsPerInch = 96
  TextHeight = 13
  object Label2: TLabel
    Left = 23
    Top = 426
    Width = 80
    Height = 13
    Anchors = [akLeft, akBottom]
    Caption = 'Total de defalcat'
  end
  object Label3: TLabel
    Left = 236
    Top = 426
    Width = 71
    Height = 13
    Anchors = [akLeft, akBottom]
    Caption = 'Total defalcat :'
  end
  object Panel1: TPanel
    Left = 1
    Top = 2
    Width = 728
    Height = 41
    Anchors = [akLeft, akTop, akRight]
    BevelInner = bvLowered
    TabOrder = 0
    DesignSize = (
      728
      41)
    object Label1: TLabel
      Left = 6
      Top = 6
      Width = 202
      Height = 13
      Caption = 'Specificati defalcarea pentru documentul : '
    end
    object LbInfoDocum: TLabel
      Left = 6
      Top = 22
      Width = 712
      Height = 13
      Anchors = [akLeft, akTop, akRight]
      AutoSize = False
    end
  end
  object cxGridDefalcare: TcxGrid
    Left = 4
    Top = 48
    Width = 724
    Height = 369
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 3
    LookAndFeel.Kind = lfOffice11
    object GridDefalcare: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = DTDefalcareDecontare
      DataController.Filter.MaxValueListCount = 1000
      DataController.Filter.Options = [fcoCaseInsensitive]
      DataController.KeyFieldNames = 'RecId'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      Filtering.ColumnPopup.MaxDropDownItemCount = 12
      OptionsData.CancelOnExit = False
      OptionsSelection.HideFocusRectOnExit = False
      OptionsView.Footer = True
      OptionsView.GroupByBox = False
      OptionsView.GroupFooters = gfVisibleWhenExpanded
      Preview.AutoHeight = False
      Preview.MaxLineCount = 2
      object GridDefalcareID_CNOTE_IMPERECHERE: TcxGridDBColumn
        Caption = 'Id '
        DataBinding.FieldName = 'ID_CNOTE_IMPERECHERE'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Options.Filtering = False
        Width = 20
      end
      object GridDefalcareID_CNOTE_ITEMSI: TcxGridDBColumn
        DataBinding.FieldName = 'ID_CNOTE_ITEMSI'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Options.Filtering = False
        Width = 105
      end
      object GridDefalcareTOTAL: TcxGridDBColumn
        Caption = 'Total'
        DataBinding.FieldName = 'TOTAL'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 65
      end
      object GridDefalcareANTERIOR: TcxGridDBColumn
        Caption = 'Anterior'
        DataBinding.FieldName = 'ANTERIOR'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 71
      end
      object GridDefalcareCURENT: TcxGridDBColumn
        Caption = 'Curent'
        DataBinding.FieldName = 'CURENT'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 72
      end
      object GridDefalcareEXPLICATIE: TcxGridDBColumn
        Caption = 'Explicatie'
        DataBinding.FieldName = 'EXPLICATIE'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 92
      end
      object GridDefalcareNRDOC: TcxGridDBColumn
        Caption = 'Nr doc'
        DataBinding.FieldName = 'NRDOC'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 76
      end
      object GridDefalcareDATA: TcxGridDBColumn
        Caption = 'Data Doc'
        DataBinding.FieldName = 'DATA'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 105
      end
      object GridDefalcareCONT_DEBT: TcxGridDBColumn
        Caption = 'ContD'
        DataBinding.FieldName = 'CONT_DEBT'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 73
      end
      object GridDefalcareCONT_CRED: TcxGridDBColumn
        Caption = 'ContC'
        DataBinding.FieldName = 'CONT_CRED'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 71
      end
      object GridDefalcareVALOARE: TcxGridDBColumn
        Caption = 'Valoare'
        DataBinding.FieldName = 'VALOARE'
        PropertiesClassName = 'TcxTextEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Options.Filtering = False
        Width = 86
      end
    end
    object cxGridDefalcareLevel1: TcxGridLevel
      GridView = GridDefalcare
    end
  end
  object BtnCancel: TcxButton
    Left = 634
    Top = 423
    Width = 83
    Height = 27
    Anchors = [akRight, akBottom]
    Caption = 'Abandon'
    LookAndFeel.Kind = lfOffice11
    ModalResult = 2
    TabOrder = 4
  end
  object BtnOk: TcxButton
    Left = 546
    Top = 423
    Width = 65
    Height = 27
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    LookAndFeel.Kind = lfOffice11
    ModalResult = 1
    TabOrder = 5
    OnClick = BtnOkClick
  end
  object edInainte: TcxCurrencyEdit
    Left = 109
    Top = 423
    Anchors = [akLeft, akBottom]
    EditValue = 0.000000000000000000
    Properties.DisplayFormat = ',0.00;-,0.00'
    TabOrder = 1
    Width = 100
  end
  object edTotal: TcxCurrencyEdit
    Left = 313
    Top = 423
    Anchors = [akLeft, akBottom]
    EditValue = 0.000000000000000000
    Properties.DisplayFormat = ',0.00;-,0.00'
    TabOrder = 2
    Width = 100
  end
  object DTDefalcareDecontare: TDataSource
    DataSet = DBDefalcareDecont1
    Left = 16
    Top = 80
  end
  object DBDefalcareDecont1: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 48
    Top = 80
  end
end
