object frmNoteEronate: TfrmNoteEronate
  Left = 255
  Top = 194
  Caption = 'Note Eronate'
  ClientHeight = 464
  ClientWidth = 631
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  DesignSize = (
    631
    464)
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 631
    Height = 57
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'LISTA NOTE CONTABILE ERONATE'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -21
    Font.Name = 'Arial'
    Font.Style = []
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 1
    Indent = 15
    StartColor = clRed
    EndColor = 12615680
    DegradeType = dtHorizontal
  end
  object BtnOk: TcxButton
    Left = 270
    Top = 399
    Width = 75
    Height = 25
    Anchors = [akBottom]
    Caption = 'Ok'
    ModalResult = 1
    TabOrder = 0
  end
  object gridNoteEronate: TcxGrid
    Left = 0
    Top = 57
    Width = 631
    Height = 336
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 2
    LookAndFeel.Kind = lfFlat
    object viewNoteEronate: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = DTNoteEronate
      DataController.Filter.MaxValueListCount = 1000
      DataController.Filter.Active = True
      DataController.KeyFieldNames = 'NR'
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
      Styles.Content = cxStyle1
      Styles.Footer = cxStyle3
      Styles.Header = cxStyle2
      Styles.Indicator = cxStyle2
      Styles.Preview = cxStyle4
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
      object viewNoteEronateCOD: TcxGridDBColumn
        Caption = 'Cod'
        DataBinding.FieldName = 'COD'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 25
      end
      object viewNoteEronateJURNAL: TcxGridDBColumn
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
      end
      object viewNoteEronateNRDOC: TcxGridDBColumn
        Caption = 'Nr. Nota'
        DataBinding.FieldName = 'NRDOC'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 46
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
        Width = 98
      end
      object viewNoteEronateECL: TcxGridDBColumn
        Caption = 'Ecl.'
        DataBinding.FieldName = 'ECL'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 24
      end
      object viewNoteEronateCONTD: TcxGridDBColumn
        Caption = 'Debit'
        DataBinding.FieldName = 'CONTD'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 92
      end
      object viewNoteEronateREPARTITOR_DEBIT: TcxGridDBColumn
        Caption = 'Repartitor Debit'
        DataBinding.FieldName = 'REPARTITOR_DEBIT'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 194
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
        Width = 94
      end
      object viewNoteEronateCONTC: TcxGridDBColumn
        Caption = 'Credit'
        DataBinding.FieldName = 'CONTC'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 88
      end
      object viewNoteEronateREPARTITOR_CREDIT: TcxGridDBColumn
        Caption = 'Repartitor Credit'
        DataBinding.FieldName = 'REPARTITOR_CREDIT'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 194
      end
      object viewNoteEronateEXPLICATIE: TcxGridDBColumn
        Caption = 'Explicatie'
        DataBinding.FieldName = 'EXPLICATIE'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 213
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
            Description = 'Salarizare'
            ImageIndex = 3
            Value = '8'
          end>
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        MinWidth = 16
      end
      object viewNoteEronateC_O: TcxGridDBColumn
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
      end
      object viewNoteEronateID_PARINTE: TcxGridDBColumn
        Caption = 'Parinte'
        DataBinding.FieldName = 'ID_PARINTE'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        HeaderAlignmentHorz = taCenter
        Width = 43
      end
      object viewNoteEronateDATA_OPERARE: TcxGridDBColumn
        Caption = 'Operare'
        DataBinding.FieldName = 'DATA_OPERARE'
        PropertiesClassName = 'TcxDateEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.DateButtons = [btnClear, btnToday]
        Properties.DateOnError = deToday
        Properties.InputKind = ikRegExpr
        HeaderAlignmentHorz = taCenter
        Width = 112
      end
      object viewNoteEronateCOD_ECONOMIC: TcxGridDBColumn
        Caption = 'Clasa Ec.'
        DataBinding.FieldName = 'COD_ECONOMIC'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 100
      end
      object viewNoteEronateCOD_FUNCTIONAL: TcxGridDBColumn
        Caption = 'Clasa Func.'
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.Alignment.Vert = taTopJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 100
      end
    end
    object nivelNoteEronate: TcxGridLevel
      GridView = viewNoteEronate
    end
  end
  object QryNoteErr: TZReadOnlyQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_GET_NOTE_ERONATE :DATA_START, :DATA_END')
    Params = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_START'
        ParamType = ptUnknown
        Size = 16
        Value = 37257d
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_END'
        ParamType = ptUnknown
        Size = 16
        Value = 37622d
      end>
    Left = 104
    Top = 248
    ParamData = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_START'
        ParamType = ptUnknown
        Size = 16
        Value = 37257d
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_END'
        ParamType = ptUnknown
        Size = 16
        Value = 37622d
      end>
  end
  object DTNoteEronate: TDataSource
    DataSet = QryNoteErr
    Left = 32
    Top = 248
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
end
