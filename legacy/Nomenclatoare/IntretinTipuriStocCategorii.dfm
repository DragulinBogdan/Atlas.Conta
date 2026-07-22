object frmIntertinStocCategorii: TfrmIntertinStocCategorii
  Left = 333
  Top = 122
  Caption = 'Intretinere Tip Stoc pe Tipuri de Categorii'
  ClientHeight = 527
  ClientWidth = 673
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
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 448
    Width = 673
    Height = 79
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    DesignSize = (
      673
      79)
    object btnOk: TcxButton
      Left = 585
      Top = 6
      Width = 75
      Height = 25
      Hint = 'Inchide Ecranul'
      Anchors = [akRight, akBottom]
      Caption = '&Inchide'
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
      OnClick = btnOkClick
    end
  end
  object pnContent: TPanel
    Left = 0
    Top = 41
    Width = 673
    Height = 407
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 456
    object GridTipProdus: TcxGrid
      Left = 0
      Top = 0
      Width = 673
      Height = 407
      Align = alClient
      TabOrder = 0
      ExplicitHeight = 456
      object GridTipProdusDBBandedTableView1: TcxGridDBBandedTableView
        OnMouseDown = GridTipProdusDBBandedTableView1MouseDown
        OnMouseMove = GridTipProdusDBBandedTableView1MouseMove
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        OnCustomDrawCell = GridTipProdusDBBandedTableView1CustomDrawCell
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsData.Editing = False
        OptionsView.CellAutoHeight = True
        OptionsView.DataRowHeight = 25
        OptionsView.GroupByBox = False
        Styles.StyleSheet = GridBandedTableViewStyleSheetUserFormat4
        Bands = <
          item
            Caption = 'STOC'
            FixedKind = fkLeft
            Width = 238
          end
          item
            Caption = 'FCT'
            Width = 131
          end
          item
            Caption = 'FEX'
            Width = 130
          end
          item
            Caption = 'PD'
            Position.BandIndex = 1
            Position.ColIndex = 0
            Styles.Header = cxStyle12
          end
          item
            Caption = 'PM'
            Position.BandIndex = 1
            Position.ColIndex = 1
            Styles.Header = cxStyle10
          end
          item
            Caption = 'PD'
            Position.BandIndex = 2
            Position.ColIndex = 0
          end
          item
            Caption = 'PM'
            Position.BandIndex = 2
            Position.ColIndex = 1
          end>
        object GridTipProdusBandedTableView1BandedColumn1: TcxGridBandedColumn
          Caption = '+'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <>
          HeaderAlignmentHorz = taCenter
          HeaderGlyphAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 29
          Position.BandIndex = 3
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridTipProdusBandedTableView1BandedColumn2: TcxGridBandedColumn
          Caption = '-'
          HeaderAlignmentHorz = taCenter
          HeaderGlyphAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 25
          Position.BandIndex = 3
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridTipProdusBandedTableView1BandedColumn3: TcxGridBandedColumn
          Caption = '+'
          HeaderAlignmentHorz = taCenter
          HeaderGlyphAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 29
          Position.BandIndex = 4
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridTipProdusBandedTableView1BandedColumn4: TcxGridBandedColumn
          Caption = '-'
          HeaderAlignmentHorz = taCenter
          HeaderGlyphAlignmentHorz = taCenter
          Options.Filtering = False
          Width = 25
          Position.BandIndex = 4
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridTipProdusBandedTableView1BandedColumn5: TcxGridBandedColumn
          Caption = 'Denumire'
          HeaderAlignmentHorz = taCenter
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridTipProdusDBBandedTableView1DBBandedColumn1: TcxGridDBBandedColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Position.BandIndex = 5
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
      end
      object GridTipProdusLevel1: TcxGridLevel
        GridView = GridTipProdusDBBandedTableView1
      end
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 673
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Influente Stocuri pe Categorii'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 2
    Indent = 10
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object cxStyleRepository1: TcxStyleRepository
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -16
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle2: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle3: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle4: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 15461355
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle5: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle6: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clGray
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle7: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle8: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle9: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle10: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle11: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle12: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object GridBandedTableViewStyleSheetUserFormat4: TcxGridBandedTableViewStyleSheet
      Caption = 'UserFormat4'
      Styles.Content = cxStyle2
      Styles.ContentEven = cxStyle3
      Styles.ContentOdd = cxStyle4
      Styles.Footer = cxStyle5
      Styles.Group = cxStyle6
      Styles.GroupByBox = cxStyle7
      Styles.Header = cxStyle8
      Styles.Inactive = cxStyle9
      Styles.Indicator = cxStyle10
      Styles.Preview = cxStyle11
      Styles.Selection = cxStyle12
      Styles.BandHeader = cxStyle1
      BuiltIn = True
    end
  end
  object MemStoc: TdxMemData
    Indexes = <>
    SortOptions = []
    AfterScroll = MemStocAfterScroll
    Left = 152
    Top = 16
  end
  object DTStoc: TDataSource
    DataSet = MemStoc
    Left = 120
    Top = 16
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = GridTipProdus
    PopupMenus = <
      item
        GridView = GridTipProdusDBBandedTableView1
        HitTypes = [gvhtCell]
        Index = 0
        PopupMenu = PopupMenu1
      end>
    Left = 416
    Top = 280
  end
  object PopupMenu1: TPopupMenu
    Left = 176
    Top = 137
    object DoubleClik1: TMenuItem
      Caption = '--------    Double Click  --------'
      Enabled = False
    end
    object Cmd_SetCrestere: TMenuItem
      Tag = 1
      Caption = 'Seteaza Crestere (+)'
      OnClick = Cmd_SetNullClick
    end
    object Cmd_SetDesCrestere: TMenuItem
      Tag = -1
      Caption = 'Seteaza Descrestere (-)'
      OnClick = Cmd_SetNullClick
    end
    object Cmd_SetNull: TMenuItem
      Caption = 'Seteaza Neinfluentare ( )'
      OnClick = Cmd_SetNullClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object Cmd_Produse: TMenuItem
      Caption = 'Produsele pentru care se face stoc'
      OnClick = Cmd_ProduseClick
    end
    object N2: TMenuItem
      Caption = '-'
    end
  end
end
