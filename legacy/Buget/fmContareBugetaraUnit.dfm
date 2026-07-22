object frmContareBugetara: TfrmContareBugetara
  Left = 179
  Top = 44
  Caption = 'Contare Executie Bugetara'
  ClientHeight = 635
  ClientWidth = 1104
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object pnParametrii: TPanel
    Left = 0
    Top = 0
    Width = 1104
    Height = 46
    Align = alTop
    TabOrder = 0
    OnResize = pnParametriiResize
    DesignSize = (
      1104
      46)
    object cmbTipContari: TcxImageComboBox
      Left = 8
      Top = 20
      Anchors = [akLeft, akTop, akRight]
      EditValue = '3'
      Properties.Items = <
        item
          Description = 'Consumuri Materiale'
          ImageIndex = 0
          Value = 1
        end
        item
          Description = 'Consumuri Servicii'
          Value = 2
        end
        item
          Description = 'Toate Consumurile'
          Value = 3
        end
        item
          Description = 'Note Banca'
          Value = 5
        end
        item
          Description = 'Note Consum'
          Value = 4
        end>
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      Width = 693
    end
    object LbTipContari: TcxLabel
      Left = 5
      Top = 4
      Caption = 'Tipuri de contari :'
    end
    object pnRight: TPanel
      Left = 710
      Top = 1
      Width = 393
      Height = 44
      Align = alRight
      BevelOuter = bvNone
      TabOrder = 2
      DesignSize = (
        393
        44)
      object Label1: TLabel
        Left = 15
        Top = 4
        Width = 58
        Height = 13
        Anchors = [akTop, akRight]
        Caption = 'De la data : '
      end
      object Label2: TLabel
        Left = 153
        Top = 4
        Width = 45
        Height = 13
        Anchors = [akTop, akRight]
        Caption = 'La data : '
      end
      object btnOpen: TcxButton
        Left = 287
        Top = 9
        Width = 98
        Height = 29
        Anchors = [akTop, akRight]
        Caption = 'Deschide'
        LookAndFeel.Kind = lfOffice11
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D560A00000000000036000000280000002400000012000000010020000000
          000000000000C40E0000C40E0000000000000000000000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000808000FFFFFFFF00808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000800000FF800000FF0080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000808000008080000080800000808000808080FF808080FFFFFFFFFF0080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000008080000080800000808000800000FF008000FF0080
          00FF800000FF0080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080008080
          80FF0080800000808000808080FFFFFFFFFF0080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000800000FF008000FF008000FF008000FF008000FF800000FF008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000808080FF008080000080800000808000008080008080
          80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
          8000008080000080800000808000800000FF008000FF008000FF008000FF0080
          00FF008000FF008000FF800000FF008080000080800000808000008080000080
          80000080800000808000008080000080800000808000808080FF008080000080
          800000808000008080000080800000808000808080FFFFFFFFFF008080000080
          8000008080000080800000808000008080000080800000808000800000FF0080
          00FF008000FF008000FF00FF00FF008000FF008000FF008000FF008000FF8000
          00FF008080000080800000808000008080000080800000808000008080000080
          8000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF008080000080
          800000808000808080FFFFFFFFFF008080000080800000808000008080000080
          80000080800000808000008000FF008000FF008000FF00FF00FF0080800000FF
          00FF008000FF008000FF008000FF800000FF0080800000808000008080000080
          800000808000008080000080800000808000808080FFFFFFFFFF008080008080
          80FF00808000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF0080
          800000808000008080000080800000808000008080000080800000FF00FF0080
          00FF00FF00FF00808000008080000080800000FF00FF008000FF008000FF0080
          00FF800000FF0080800000808000008080000080800000808000008080000080
          8000808080FFFFFFFFFF808080FF008080000080800000808000808080FFFFFF
          FFFF0080800000808000808080FFFFFFFFFF0080800000808000008080000080
          800000808000008080000080800000FF00FF0080800000808000008080000080
          80000080800000FF00FF008000FF008000FF008000FF800000FF008080000080
          80000080800000808000008080000080800000808000808080FF008080000080
          8000008080000080800000808000808080FFFFFFFFFF00808000008080008080
          80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
          800000808000008080000080800000808000008080000080800000FF00FF0080
          00FF008000FF008000FF800000FF008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000808080FFFFFFFFFF0080800000808000808080FFFFFFFFFF008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000808000008080000080800000FF00FF008000FF008000FF008000FF8000
          00FF008080000080800000808000008080000080800000808000008080000080
          80000080800000808000008080000080800000808000808080FFFFFFFFFF0080
          800000808000808080FFFFFFFFFF008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000FF00FF008000FF008000FF008000FF800000FF00808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000808080FFFFFFFFFF0080800000808000808080FFFFFF
          FFFF008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000FF00FF008000FF0080
          00FF008000FF800000FF00808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080008080
          80FFFFFFFFFF0080800000808000808080FFFFFFFFFF00808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000FF00FF008000FF008000FF800000FF008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000808000008080000080800000808000808080FFFFFFFFFF008080008080
          80FFFFFFFFFF0080800000808000008080000080800000808000008080000080
          80000080800000808000008080000080800000808000008080000080800000FF
          00FF008000FF008000FF00808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          800000808000808080FFFFFFFFFF808080FF0080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000008080000080800000FF00FF00808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000808080FF0080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          8000008080000080800000808000008080000080800000808000008080000080
          80000080800000808000008080000080800000808000}
        OptionsImage.NumGlyphs = 2
        TabOrder = 0
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        OnClick = btnOpenClick
      end
      object edDataStart: TcxDateEdit
        Left = 8
        Top = 20
        Anchors = [akTop, akRight]
        Properties.InputKind = ikMask
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        Width = 121
      end
      object edDataEnd: TcxDateEdit
        Left = 136
        Top = 20
        Anchors = [akTop, akRight]
        Properties.InputKind = ikMask
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        Width = 121
      end
    end
  end
  object pnClient: TGroupBox
    Left = 0
    Top = 46
    Width = 1104
    Height = 589
    Align = alClient
    Caption = 'Lista documentelor pentru clasificare'
    TabOrder = 1
    object gridDocumente: TcxGrid
      Left = 2
      Top = 15
      Width = 1100
      Height = 572
      Align = alClient
      TabOrder = 0
      LookAndFeel.Kind = lfOffice11
      object gridDocsView: TcxGridDBBandedTableView
        OnDblClick = ppIntroducereClasificClick
        Navigator.Buttons.CustomButtons = <>
        DataController.DataSource = DTDocList
        DataController.Filter.Active = True
        DataController.KeyFieldNames = 'RecId'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <
          item
            Kind = skSum
            FieldName = 'VALOARE'
          end
          item
            Kind = skSum
            Column = gridDocsViewDPrimar_Suma
          end>
        DataController.Summary.SummaryGroups = <>
        OptionsBehavior.DragHighlighting = False
        OptionsBehavior.DragOpening = False
        OptionsBehavior.DragScrolling = False
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ExpandMasterRowOnDblClick = False
        OptionsCustomize.ColumnHiding = True
        OptionsCustomize.ColumnsQuickCustomization = True
        OptionsCustomize.BandsQuickCustomization = True
        OptionsData.Deleting = False
        OptionsData.DeletingConfirmation = False
        OptionsData.Editing = False
        OptionsData.Inserting = False
        OptionsSelection.MultiSelect = True
        OptionsView.Footer = True
        OptionsView.GridLineColor = clSilver
        OptionsView.GroupByBox = False
        Bands = <
          item
            Caption = 'Executie Bugetara'
            Width = 485
          end
          item
            Caption = 'Nota Contabila'
            Width = 246
          end
          item
            Caption = 'Document primar'
            Width = 451
          end
          item
            Caption = 'Contract'
            Width = 126
          end>
        object gridDocsViewid_EB_Contare: TcxGridDBBandedColumn
          DataBinding.FieldName = 'id_EB_Contare'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_ID: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_ID'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_Status: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_Status'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_UpdateUser: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_UpdateUser'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_Date: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_Date'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_ID_Utilizatori: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_ID_Utilizatori'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object gridDocsViewInternal_TimeImport: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Internal_TimeImport'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Id: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Nota_Id'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Modul: TcxGridDBBandedColumn
          Caption = 'Modul'
          DataBinding.FieldName = 'Nota_Modul'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 39
          Position.BandIndex = 1
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Jurnal: TcxGridDBBandedColumn
          Caption = 'Jurnal'
          DataBinding.FieldName = 'Nota_Jurnal'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 38
          Position.BandIndex = 1
          Position.ColIndex = 9
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Nr: TcxGridDBBandedColumn
          Caption = 'Nr'
          DataBinding.FieldName = 'Nota_Nr'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 29
          Position.BandIndex = 1
          Position.ColIndex = 10
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Data: TcxGridDBBandedColumn
          Caption = 'Data'
          DataBinding.FieldName = 'Nota_Data'
          Width = 41
          Position.BandIndex = 1
          Position.ColIndex = 11
          Position.RowIndex = 0
        end
        object gridDocsViewNota_Cont: TcxGridDBBandedColumn
          Caption = 'Cont'
          DataBinding.FieldName = 'Nota_Cont'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 49
          Position.BandIndex = 1
          Position.ColIndex = 12
          Position.RowIndex = 0
        end
        object gridDocsViewNota_ContCrsp: TcxGridDBBandedColumn
          Caption = 'ContCrsp'
          DataBinding.FieldName = 'Nota_ContCrsp'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 50
          Position.BandIndex = 1
          Position.ColIndex = 13
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Id: TcxGridDBBandedColumn
          DataBinding.FieldName = 'DPrimar_Id'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 14
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Id_Defalcare: TcxGridDBBandedColumn
          DataBinding.FieldName = 'DPrimar_Id_Defalcare'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 15
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Tip: TcxGridDBBandedColumn
          Caption = 'Tip'
          DataBinding.FieldName = 'DPrimar_Tip'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 62
          Position.BandIndex = 2
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Nr: TcxGridDBBandedColumn
          Caption = 'Nr'
          DataBinding.FieldName = 'DPrimar_Nr'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 44
          Position.BandIndex = 2
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Data: TcxGridDBBandedColumn
          Caption = 'Data'
          DataBinding.FieldName = 'DPrimar_Data'
          Width = 56
          Position.BandIndex = 2
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Gestiune: TcxGridDBBandedColumn
          Caption = 'Gestiune'
          DataBinding.FieldName = 'DPrimar_Gestiune'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 43
          Position.BandIndex = 2
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Furnizor: TcxGridDBBandedColumn
          Caption = 'Furnizor'
          DataBinding.FieldName = 'DPrimar_Furnizor'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 42
          Position.BandIndex = 2
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Suma: TcxGridDBBandedColumn
          Caption = 'Suma'
          DataBinding.FieldName = 'DPrimar_Suma'
          Width = 44
          Position.BandIndex = 2
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Explicatie: TcxGridDBBandedColumn
          Caption = 'Explicatie'
          DataBinding.FieldName = 'DPrimar_Explicatie'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 43
          Position.BandIndex = 2
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object gridDocsViewDPrimar_Operator: TcxGridDBBandedColumn
          Caption = 'Operator'
          DataBinding.FieldName = 'DPrimar_Operator'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 42
          Position.BandIndex = 2
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Tip: TcxGridDBBandedColumn
          Caption = 'Tip'
          DataBinding.FieldName = 'EB_Tip'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 26
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Nr: TcxGridDBBandedColumn
          Caption = 'Nr'
          DataBinding.FieldName = 'EB_Nr'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 47
          Position.BandIndex = 0
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Data: TcxGridDBBandedColumn
          Caption = 'Data'
          DataBinding.FieldName = 'EB_Data'
          Width = 48
          Position.BandIndex = 0
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object gridDocsViewEB_CF: TcxGridDBBandedColumn
          Caption = 'Cod Functional'
          DataBinding.FieldName = 'EB_CF'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 142
          Position.BandIndex = 0
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Unitate: TcxGridDBBandedColumn
          Caption = 'Unitate'
          DataBinding.FieldName = 'EB_Unitate'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 88
          Position.BandIndex = 0
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object gridDocsViewEB_CE: TcxGridDBBandedColumn
          Caption = 'Cod Economic'
          DataBinding.FieldName = 'EB_CE'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 79
          Position.BandIndex = 0
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Proiect: TcxGridDBBandedColumn
          Caption = 'Proiect'
          DataBinding.FieldName = 'EB_Proiect'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 55
          Position.BandIndex = 0
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Id_Ang: TcxGridDBBandedColumn
          DataBinding.FieldName = 'EB_Id_Ang'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 16
          Position.RowIndex = 0
        end
        object gridDocsViewEB_Id_Ord: TcxGridDBBandedColumn
          DataBinding.FieldName = 'EB_Id_Ord'
          Visible = False
          Position.BandIndex = 1
          Position.ColIndex = 17
          Position.RowIndex = 0
        end
        object gridDocsViewContract_Id: TcxGridDBBandedColumn
          DataBinding.FieldName = 'Contract_Id'
          Visible = False
          Position.BandIndex = 3
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object gridDocsViewContract_Nr: TcxGridDBBandedColumn
          Caption = 'Nr'
          DataBinding.FieldName = 'Contract_Nr'
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Width = 50
          Position.BandIndex = 3
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object gridDocsViewContract_Data: TcxGridDBBandedColumn
          Caption = 'Data'
          DataBinding.FieldName = 'Contract_Data'
          Width = 77
          Position.BandIndex = 3
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
      end
      object gridDocsLevel: TcxGridLevel
        GridView = gridDocsView
      end
    end
  end
  object DTDocList: TDataSource
    DataSet = memDocList
    Left = 32
    Top = 225
  end
  object memDocList: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 72
    Top = 225
  end
  object ppDetaliiMenu: TPopupMenu
    Left = 73
    Top = 262
    object ppIntroducereClasific: TMenuItem
      Caption = 'Introducere Clasificatie'
      ShortCut = 16416
      OnClick = ppIntroducereClasificClick
    end
    object ppAnulareClasificatie: TMenuItem
      Caption = 'Anuleaza Clasificatia'
      ShortCut = 16430
      OnClick = ppAnulareClasificatieClick
    end
    object N2: TMenuItem
      Caption = '-'
    end
    object IntroducereProiect1: TMenuItem
      Caption = 'Introducere Proiect'
      ShortCut = 16424
      OnClick = IntroducereProiect1Click
    end
    object AnuleazaProiect1: TMenuItem
      Caption = 'Anuleaza Proiect'
      OnClick = AnuleazaProiect1Click
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object AsociereContract1: TMenuItem
      Caption = 'Asociere Contract'
      OnClick = AsociereContract1Click
    end
    object DezasociereContract1: TMenuItem
      Caption = 'Dezasociere Contract'
      OnClick = DezasociereContract1Click
    end
  end
  object popupGrid: TcxGridPopupMenu
    Grid = gridDocumente
    PopupMenus = <
      item
        GridView = gridDocsView
        HitTypes = [gvhtCell, gvhtRecord]
        Index = 0
        PopupMenu = ppDetaliiMenu
      end>
    AlwaysFireOnPopup = True
    Left = 72
    Top = 299
  end
  object qryDocProvider: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spEBGetListValori :DATA_START, :DATA_SFARSIT, :TIPURI, :ID_' +
        'UTILIZATORI, :Status')
    Params = <
      item
        DataType = ftDateTime
        Name = 'DATA_START'
        ParamType = ptInput
      end
      item
        DataType = ftDateTime
        Name = 'DATA_SFARSIT'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'TIPURI'
        ParamType = ptInput
        Value = 2
      end
      item
        DataType = ftInteger
        Name = 'ID_UTILIZATORI'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'Status'
        ParamType = ptInput
      end>
    Left = 32
    Top = 297
    ParamData = <
      item
        DataType = ftDateTime
        Name = 'DATA_START'
        ParamType = ptInput
      end
      item
        DataType = ftDateTime
        Name = 'DATA_SFARSIT'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'TIPURI'
        ParamType = ptInput
        Value = 2
      end
      item
        DataType = ftInteger
        Name = 'ID_UTILIZATORI'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'Status'
        ParamType = ptInput
      end>
  end
end
