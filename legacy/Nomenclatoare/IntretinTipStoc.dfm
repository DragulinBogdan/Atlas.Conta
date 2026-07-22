object frmIntretinTipStoc: TfrmIntretinTipStoc
  Left = 325
  Top = 114
  ClientHeight = 444
  ClientWidth = 758
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
  OnResize = FormResize
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 363
    Width = 758
    Height = 81
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    OnResize = pnBottomResize
    DesignSize = (
      758
      81)
    object btnOk: TcxButton
      Left = 670
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
    Left = 258
    Top = 41
    Width = 500
    Height = 322
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Color = clWhite
    TabOrder = 1
    ExplicitHeight = 372
    DesignSize = (
      500
      322)
    object Bevel1: TBevel
      Left = 2
      Top = 32
      Width = 497
      Height = -205
      Anchors = [akLeft, akTop, akRight, akBottom]
      Shape = bsBottomLine
      ExplicitHeight = -155
    end
    object pnBot: TPanel
      Left = 2
      Top = 182
      Width = 496
      Height = 138
      Align = alBottom
      Color = clWhite
      TabOrder = 0
      ExplicitTop = 232
      DesignSize = (
        496
        138)
      object Label1: TLabel
        Left = 3
        Top = 7
        Width = 54
        Height = 13
        Anchors = [akLeft, akBottom]
        Caption = 'Denumire'
        FocusControl = edtDenumire
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label2: TLabel
        Left = 3
        Top = 47
        Width = 55
        Height = 13
        Anchors = [akLeft, akBottom]
        Caption = 'Descriere'
        FocusControl = edtDescriere
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label3: TLabel
        Left = 3
        Top = 87
        Width = 60
        Height = 13
        Anchors = [akLeft, akBottom]
        Caption = 'Nivel Stoc'
        FocusControl = edtNivelStoc
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label4: TLabel
        Left = 267
        Top = 87
        Width = 65
        Height = 13
        Anchors = [akLeft, akBottom]
        Caption = 'Grupa Stoc'
        FocusControl = edtGrupaStoc
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edtDenumire: TcxDBTextEdit
        Left = 3
        Top = 23
        Anchors = [akLeft, akRight, akBottom]
        DataBinding.DataField = 'DENUMIRE'
        DataBinding.DataSource = DTTipStoc
        TabOrder = 0
        Width = 485
      end
      object edtDescriere: TcxDBTextEdit
        Left = 3
        Top = 63
        Anchors = [akLeft, akRight, akBottom]
        DataBinding.DataField = 'DESCRIERE'
        DataBinding.DataSource = DTTipStoc
        TabOrder = 1
        Width = 485
      end
      object edtNivelStoc: TcxDBImageComboBox
        Left = 3
        Top = 103
        Anchors = [akLeft, akBottom]
        DataBinding.DataField = 'id_gest_nivel_stoc'
        DataBinding.DataSource = DTTipStoc
        Properties.Alignment.Horz = taLeftJustify
        Properties.Items = <>
        TabOrder = 2
        OnKeyDown = edtNivelStocKeyDown
        Width = 209
      end
      object edtGrupaStoc: TcxDBImageComboBox
        Left = 267
        Top = 103
        Anchors = [akLeft, akRight, akBottom]
        DataBinding.DataField = 'id_gest_grupa_stoc'
        DataBinding.DataSource = DTTipStoc
        Properties.Alignment.Horz = taLeftJustify
        Properties.Items = <>
        TabOrder = 3
        OnKeyDown = edtGrupaStocKeyDown
        Width = 218
      end
    end
    object Panel1: TPanel
      Left = 2
      Top = 2
      Width = 496
      Height = 180
      Align = alClient
      BevelOuter = bvNone
      Color = clWhite
      TabOrder = 1
      ExplicitHeight = 230
      object cxGridTipStoc: TcxGrid
        Left = 0
        Top = 0
        Width = 496
        Height = 180
        Hint = 'sageata sus/sageata jos permite deplasarea prin nomenclator'
        Align = alClient
        BevelInner = bvNone
        BevelOuter = bvNone
        TabOrder = 0
        ExplicitHeight = 230
        object cxGridTipStocDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnCustomDrawCell = cxGridTipStocDBTableView1CustomDrawCell
          DataController.DataSource = DTTipStoc
          DataController.KeyFieldNames = 'ID_GEST_TIP_STOC'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          Styles.StyleSheet = GridTableViewStyleSheetUserFormat4
          object cxGridTipStocDBTableView1ID_GEST_TIP_STOC: TcxGridDBColumn
            Caption = 'Identificator'
            DataBinding.FieldName = 'ID_GEST_TIP_STOC'
            Visible = False
          end
          object cxGridTipStocDBTableView1DENUMIRE: TcxGridDBColumn
            Caption = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 164
          end
          object cxGridTipStocDBTableView1DESCRIERE: TcxGridDBColumn
            Caption = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            Width = 173
          end
          object cxGridTipStocDBTableView1TIP_DESCARCARE: TcxGridDBColumn
            Caption = 'Tip Descarcare'
            DataBinding.FieldName = 'TIP_DESCARCARE'
            Visible = False
            Width = 144
          end
          object cxGridTipStocDBTableView1DATA_CALCUL: TcxGridDBColumn
            Caption = 'Data Calcul'
            DataBinding.FieldName = 'DATA_CALCUL'
            Visible = False
          end
          object cxGridTipStocDBTableView1ID_PARINTE: TcxGridDBColumn
            Caption = 'Id Parinte'
            DataBinding.FieldName = 'ID_PARINTE'
            Visible = False
          end
          object cxGridTipStocDBTableView1id_gest_nivel_stoc: TcxGridDBColumn
            Caption = 'Nivel Stoc'
            DataBinding.FieldName = 'id_gest_nivel_stoc'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Items = <>
            Width = 79
          end
          object cxGridTipStocDBTableView1id_gest_grupa_stoc: TcxGridDBColumn
            Caption = 'Grupa Stoc'
            DataBinding.FieldName = 'id_gest_grupa_stoc'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.Items = <>
            Width = 78
          end
        end
        object cxGridTipStocLevel1: TcxGridLevel
          GridView = cxGridTipStocDBTableView1
        end
      end
    end
  end
  object pnLeft: TPanel
    Left = 0
    Top = 41
    Width = 250
    Height = 322
    Align = alLeft
    BevelOuter = bvNone
    TabOrder = 2
    ExplicitHeight = 372
    object grpBoxNivStoc: TcxGroupBox
      Left = 0
      Top = 249
      Align = alClient
      Caption = 'Modificare Nivel Stoc'
      Ctl3D = True
      ParentBackground = False
      ParentColor = False
      ParentCtl3D = False
      ParentFont = False
      Style.BorderStyle = ebsThick
      Style.Color = clWhite
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.Shadow = False
      Style.IsFontAssigned = True
      TabOrder = 0
      ExplicitHeight = 123
      Height = 73
      Width = 250
      object TreeListNivStoc: TcxDBTreeList
        Left = 2
        Top = 18
        Width = 246
        Height = 53
        Align = alClient
        Bands = <
          item
            Caption.Text = 'Band1'
            Width = 300
          end>
        DataController.DataSource = DTNivelStoc
        DataController.ParentField = 'PARENT_ID'
        DataController.KeyField = 'id_gest_nivel_stoc'
        Navigator.Buttons.CustomButtons = <>
        OptionsData.Editing = False
        OptionsView.ColumnAutoWidth = True
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        Styles.StyleSheet = TreeListStyleSheetSlate
        TabOrder = 0
        OnCustomDrawDataCell = TreeListNivStocCustomDrawDataCell
        OnFocusedNodeChanged = TreeListNivStocFocusedNodeChanged
        ExplicitHeight = 103
        object TreeListNivStocid_gest_nivel_stoc: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'id_gest_nivel_stoc'
          Width = 100
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeListNivStocdenumire: TcxDBTreeListColumn
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'denumire'
          Width = 331
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeListNivStocPARENT_ID: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'PARENT_ID'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object grpBoxGrupaStoc: TcxGroupBox
      Left = 0
      Top = 0
      Align = alTop
      Caption = 'Modificare Grupei de Stoc'
      Ctl3D = True
      ParentBackground = False
      ParentColor = False
      ParentCtl3D = False
      ParentFont = False
      Style.BorderStyle = ebsThick
      Style.Color = clWhite
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.Shadow = False
      Style.IsFontAssigned = True
      TabOrder = 1
      Height = 249
      Width = 250
      object GridGrupaStoc: TcxGrid
        Left = 2
        Top = 18
        Width = 246
        Height = 229
        Align = alClient
        TabOrder = 0
        object GridGrupaStocDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnCustomDrawCell = GridGrupaStocDBTableView1CustomDrawCell
          OnFocusedRecordChanged = GridGrupaStocDBTableView1FocusedRecordChanged
          DataController.DataSource = DTGrupaStoc
          DataController.KeyFieldNames = 'id_gest_grupa_stoc'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsData.Editing = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          OptionsView.Header = False
          Styles.StyleSheet = GridTableViewStyleSheetUserFormat4
          object GridGrupaStocDBTableView1id_gest_grupa_stoc: TcxGridDBColumn
            DataBinding.FieldName = 'id_gest_grupa_stoc'
            Visible = False
          end
          object GridGrupaStocDBTableView1denumire: TcxGridDBColumn
            DataBinding.FieldName = 'denumire'
          end
        end
        object GridGrupaStocLevel1: TcxGridLevel
          GridView = GridGrupaStocDBTableView1
        end
      end
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 758
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Intretinere nomenclator Tip Stoc'
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWhite
    Font.Height = -20
    Font.Name = 'Verdana'
    Font.Style = [fsBold]
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 3
    OnResize = pnTopResize
    Indent = 40
    StartColor = clBlack
    DegradeType = dtHorizontal
    DesignSize = (
      758
      41)
    object btnAddProdus: TcxButton
      Left = 578
      Top = 14
      Width = 85
      Height = 25
      Hint = 'Adauga un nou tip de stoc'
      Anchors = [akTop, akRight]
      Caption = '&Adauga'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360900000000000036000000280000001800000018000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FBFAFAFFEFEEEEFFE1DF
        E0FFD6D4D4FFCBC9C9FFD0CECFFFE2E1E2FFE9EAE9FFB7CFB8FF93C597FF8DC0
        91FFACD1AFFFEBF4ECFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FEFE
        FEFFF8F8F7FFE9E8E8FFDBDADAFFD2CFCFFFCBC7C8FFC3BFC0FFC2BDBEFFC5C0
        C1FFC9C3C4FFC9C4C5FFDDD4D9FF6A806AFF13681EFF1B9C3CFF0BA83FFF0AA9
        40FF1DA244FF2D933DFF99C79DFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00D0CF
        CEFF93918EFFA4A19FFFB4B0AFFFC3BEBEFFCBC6C7FFCCC8C8FFCDC8C9FFCEC9
        CAFFCEC9CAFFD5CAD0FF97B196FF28923AFF37BE6AFF29CB71FF54C587FF62BE
        8CFF38C476FF43C679FF2F9A47FF83BB88FFFFFFFF00FFFFFF00FFFFFF00C5C4
        C2FF969390FFADAAA8FFBDB9B8FFC8C4C4FFCDC8C9FFCDC8C9FFCDC8C9FFCDC8
        C9FFD2CACEFFB5BDB2FF298935FF4FBD79FF49CE84FF2EC66EFFCCE6D8FFEFE1
        E9FF4BBA7AFF44CD80FF64CC92FF2F9541FFC0DDC3FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCDC8
        C9FFD7CCD2FF6EA16FFF379F50FF6BCC98FF47C27CFF39BF73FFCFE9DAFFECE9
        EBFF52B57DFF46C47CFF65CA92FF5CB87CFF66AC6DFFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCEC8
        CAFFD0C9CBFF4E9654FF4DB26EFF99CCB1FFBBCEC3FFB2CCBDFFECF1EFFFF6F6
        F6FFBACEC3FFB6CCBFFFA1BEAEFF69C18EFF399747FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCFC9
        CAFFCEC8C9FF4B9652FF44B26BFFB2DDC6FFFFFFFF00FCFBFCFFFFFEFEFFFFFF
        FF00FEFCFDFFFFFEFFFFD4DED9FF63C18BFF329542FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C4FFCEC9CAFFCEC9CAFFCEC9CAFFD0CA
        CCFFD6CDD1FF5D9D61FF39AD5DFF66CB93FF71C696FF81C49EFFE1EBE5FFF0EE
        EFFF92BFA6FF7FC79EFF73C898FF5DC488FF449C50FFFFFFFF00FFFFFF00C6C5
        C3FF969390FFADAAA8FFBBB7B6FFC6C2C2FFCBC6C8FFCBC6C7FFC7C2C3FFC2BD
        BEFFC2B8BEFF7A957AFF1E8E33FF5BCA8BFF70CC99FF8BC9A5FFE2EBE6FFEEEA
        EDFF95BDA6FF82CDA2FF71D09CFF3EA95DFF8ABF8FFFFFFFFF00FFFFFF00C1C1
        C0FF8C8986FF9B9896FFA09E9DFFA29F9FFF9D9A9AFF8F8C8DFF797677FF5B5A
        5AFF3B393AFF272326FF1C5821FF33A54EFF74CD9BFF9ECFB5FFD0D9D5FFDADE
        DCFFA5C9B6FF89D2ABFF53B776FF3F984BFFEDF5EEFFFFFFFF00FFFFFF006969
        68FF4F4E4EFF5A5959FF676666FF716F6FFF6C6A6BFF5D5C5CFF4C4B4BFF3D3D
        3DFF343434FF302F30FF3D3C3DFF246129FF25933BFF55B675FF7CC398FF7EC3
        9AFF65BE86FF35A352FF379642FFD6E9D8FFFFFFFF00FFFFFF00FFFFFF009090
        91FF909090FFB2B2B2FF737272FF5B5B5BFF5A5A5AFF5B5B5BFF5C5C5CFF6564
        65FF727071FF7E7C7DFF898587FF979194FF88A488FF4F9757FF32903FFF3091
        3FFF42944CFF79A37AFFEEF4EEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F9F9F9FF8A8887FF83807FFF8E8B8BFFA29E9EFFB5B1B2FFBFBA
        BBFFC5C0C1FFCBC6C7FFCEC9CAFFCDC7C9FFD8D0D5FFC8C1C5FFC0BEBBFFC6C5
        C1FFCBC6C6FFCDC4CAFFFCFCFCFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF908F8BFFA5A19FFFB7B3B2FFC4BFBFFFCDC8C9FFCEC9
        CAFFCEC9CAFFCEC9CAFFCEC9CAFFCCC7C8FFD1CECFFFC4C0C2FFC9C2C5FFD1CA
        CCFFCDC7C9FFC5C1C1FFFCFCFCFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF93908EFFA6A2A0FFB6B3B1FFC3BEBEFFCCC7C8FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC6C7FFD0CDCEFFC2BFC0FFC7C2C3FFCEC9
        CAFFCBC7C8FFC5C2C2FFFCFCFCFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF93908EFFA6A2A0FFB6B3B1FFC3BEBEFFCCC7C8FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC6C7FFD0CCCDFFC1BFBFFFC7C2C3FFCEC9
        CAFFCBC7C8FFC5C1C2FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF94918DFFA6A2A0FFB6B3B1FFC3BEBEFFCCC7C8FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC6C7FFCFCCCDFFC1BDBEFFC8C2C3FFCEC9
        CAFFCBC6C7FFC5C2C3FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF94918FFFA7A3A1FFB7B4B2FFC5BFBFFFCEC9CAFFD0CB
        CCFFD1CCCDFFD3CFD0FFD6D1D2FFD6D0D2FFD6D3D4FFC1BEBFFFC7C2C3FFCEC9
        CAFFCBC6C7FFC6C3C3FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF95928FFFA4A09EFFB2AEADFFBDB8B8FFC2BEBFFFBBB7
        B8FFAFABABFF9F9B9CFF8D8A8AFF757272FF797778FFB2AFB0FFCAC5C6FFCEC9
        CAFFCBC6C7FFC7C3C4FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CECECEFF4D4C4BFF5C5A5AFF696868FF747273FF777575FF6F6D
        6EFF5E5D5DFF494747FF363535FF282827FF494A49FFA3A1A2FFBDB8B9FFC8C3
        C4FFCAC5C6FFC8C4C4FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CBCBCAFF676665FF7E7D7BFF91908EFF9E9C9BFFA6A4A2FFA6A4
        A1FFA19F9CFF9E9C98FF9E9B97FF9C9995FFA19E9BFFC0BBBBFFB9B4B3FFB5B0
        B1FFBBB5B7FFC2BFC0FFFDFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FBFBFBFFCBC9C8FFB5B1B0FFB4B0AFFFB5B1B0FFB4B0AFFFB2AE
        ADFFB2AEADFFB2AEACFFB1ADABFFB0ACABFFAEABA9FFB3AFAEFFBEB9B7FFBCB7
        B2FFB3AFACFFB8B5B6FFFEFDFDFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00F4F4F4FFDCDADAFFC5C2C3FFBBB6B7FFC2BD
        BEFFC7C2C3FFC8C3C5FFC8C3C4FFC7C2C3FFC6C2C3FFC3BEBFFFBDB8B9FFB9B5
        B4FFB8B4B1FFC5C0C0FFF9F8F8FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F0EFEFFFD5D2
        D3FFC2BFC0FFC7C4C5FFCBC9C9FFCFCCCDFFD3D0D1FFD7D5D5FFDBD9DAFFDEDD
        DDFFE0DFDFFFE6E5E5FFFDFDFDFFFFFFFF00FFFFFF00}
      TabOrder = 0
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnAddProdusClick
    end
    object btnStergeProd: TcxButton
      Left = 666
      Top = 14
      Width = 85
      Height = 25
      Hint = 'Sterge tipul de stoc curent'
      Anchors = [akTop, akRight]
      Caption = '&Sterge'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360900000000000036000000280000001800000018000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FAF9F9FFEDECECFFDFDE
        DEFFD5D2D3FFCAC6C7FFD4D1D2FFE6E6E3FFDDDDE4FF8F8FD9FF6565CEFF6060
        CCFF7676D2FFBEBEE9FFFEFEFFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FEFE
        FEFFF8F8F7FFE9E8E8FFDBDADAFFD2CFCFFFCAC7C7FFC2BEBEFFC1BEBEFFC5C0
        C1FFC9C4C5FFCAC4C5FFD8D5CEFF535488FF1313AAFF1D1DE1FF0F0FF1FF1011
        F2FF2222E9FF2221CCFF6060CBFFF0F0FAFFFFFFFF00FFFFFF00FFFFFF00D0CF
        CEFF93918EFFA4A19FFFB4B0AFFFC3BEBEFFCBC6C7FFCDC8C9FFCDC8C9FFCEC9
        CAFFCEC9CAFFD5CFC8FF8886BEFF2929CAFF4E4FF5FF2F2FF7FF0D0DF9FF090A
        FEFF2B2CF7FF5454F5FF4848D7FF5858C8FFFCFCFEFFFFFFFF00FFFFFF00C5C4
        C2FF969390FFADAAA8FFBDB9B8FFC8C4C4FFCDC8C9FFCDC8C9FFCDC8C9FFCDC8
        C9FFD2CDCAFFB3AFC3FF2726B8FF6161F4FF9E9EE8FFD7D7DBFF8E8ECFFF4444
        E0FFC4C4DBFFC7C7D2FF8A8AE7FF3E3ED4FFA3A3E0FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCDC8
        C9FFD8D3CAFF706DB9FF3D3ED1FF8282FFFF6E6EF5FFECECF9FFF3F3EFFFD4D4
        DFFFFFFFFF00C0C0E9FF7373FCFF7574F2FF5353C8FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCEC9
        C9FFD6D1CAFF5856B8FF5051E0FF8080FFFF5A5BFEFF9B9BEFFFFFFFFDFFFFFF
        FF00E9E9ECFF7778EBFF7273FFFF7D7DFCFF3535C2FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C3FFCDC8C9FFCDC8C9FFCDC8C9FFCEC9
        C9FFD6D1CAFF5957B8FF4545E0FF7676FFFF6464FCFFA9A9E9FFFFFFFDFFFFFF
        FF00E6E6E6FF8080DFFF7574FFFF7474FDFF3434C3FFFFFFFF00FFFFFF00C5C5
        C3FF979490FFADAAA8FFBCB8B7FFC8C3C4FFCEC9CAFFCEC9CAFFCEC9CAFFCFCA
        CBFFDBD5CDFF7673BEFF2F2FD4FF6D6CFFFF8686EEFFF3F3F1FFF9F9FAFFE0E0
        EDFFFFFFFBFFC5C5D6FF7B7BF6FF6767F6FF4C4CC6FFFFFFFF00FFFFFF00C6C5
        C3FF969390FFADAAA8FFBBB7B6FFC6C2C2FFCBC6C8FFCBC6C7FFC7C2C3FFC1BD
        BEFFBEBAB7FF9390A0FF1C1CABFF5858F4FF9F9FF4FFD4D4F3FFD7D7F1FFC9C9
        F2FFD5D5F4FFC6C6EEFF8C8BF5FF3737D3FF9F9FDFFFFFFFFF00FFFFFF00C1C1
        C0FF8C8986FF9B9896FFA09E9DFFA29FA0FF9D999AFF8D8A8BFF767374FF5756
        56FF373636FF27261FFF3C3B67FF2424C1FF6969F7FFAEAFFFFFDBDBFFFFE1E1
        FFFFC0C0FFFF9292FFFF4747DEFF5959C8FFFDFDFEFFFFFFFF00FFFFFF006969
        68FF4F4E4EFF5A5959FF676666FF716F6FFF6B6A6AFF5B5A5AFF494949FF3C3B
        3BFF333334FF303030FF46473FFF2E2E5AFF1C1CA9FF3F3ED9FF6868E9FF7171
        ECFF5354E4FF2828C8FF6262CDFFF1F1FBFFFFFFFF00FFFFFF00FFFFFF009090
        91FF909090FFB2B2B2FF737272FF5B5B5BFF5A5A5AFF5B5B5BFF5D5C5DFF6766
        66FF737272FF807E7EFF898687FF98948DFFA5A3ADFF7674B4FF5351B8FF4E4C
        BAFF6461B8FFA8A5C7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F9F9F9FF8A8887FF83807FFF8F8C8CFFA4A1A1FFB7B3B4FFBFBB
        BCFFC6C1C2FFCCC6C7FFCEC9CAFFCDC8C9FFD5D3D0FFCBC7BEFFD0CBC4FFD3CE
        CAFFD3CDC6FFD6D4CFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF908F8BFFA5A19FFFB8B4B3FFC6C1C1FFCEC9CAFFCEC9
        CAFFCEC9CAFFCEC9CAFFCEC9CAFFCBC6C7FFD2CFD0FFC1BEBEFFC9C4C4FFCFCA
        CAFFC9C4C5FFD1CECFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF93908EFFA6A2A0FFB7B3B2FFC5C0C0FFCDC8C9FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC5C6FFD1CFCFFFC0BDBDFFC9C4C5FFCEC9
        CAFFC9C4C5FFD1CFCFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF93908EFFA6A2A0FFB7B3B2FFC5C0C0FFCDC8C9FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC5C6FFD1CECEFFBFBCBCFFC9C4C5FFCEC9
        CAFFC9C4C5FFD2CFCFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F7FF94918DFFA6A2A0FFB7B3B2FFC5C0C0FFCDC8C9FFCDC8
        C9FFCDC8C9FFCDC8C9FFCDC8C9FFCBC5C6FFD0CDCEFFBFBBBCFFC9C4C5FFCEC9
        CAFFC8C3C4FFD2CFD0FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF94918FFFA7A3A1FFB8B4B3FFC6C1C1FFCFCACBFFD0CB
        CCFFD1CCCDFFD3CFCFFFD6D1D2FFD5D0D1FFD6D3D4FFBFBBBCFFC9C4C5FFCEC9
        CAFFC8C3C4FFD3D0D1FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F8F8F8FF95928FFFA4A09EFFB3AFAEFFBEBABAFFC2BDBFFFBAB6
        B6FFADA9AAFF9C9999FF8A8788FF706D6EFF807F80FFB7B3B4FFCBC6C7FFCEC9
        CAFFC8C3C4FFD3D1D1FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CECECEFF4D4C4BFF5C5A5AFF6A6869FF757374FF777575FF6D6C
        6CFF5C5A5BFF464544FF343333FF262625FF565656FFAAA7A8FFBEBABBFFCAC5
        C6FFC8C3C4FFD4D1D1FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00CBCBCAFF676665FF7E7D7BFF91908EFFA09E9CFFA7A5A3FFA5A3
        A0FFA19E9BFF9E9C98FF9E9B97FF9B9895FFA4A29FFFC0BDBCFFB7B3B2FFB5B1
        B1FFBBB6B7FFD1CFD0FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FBFBFBFFCBC9C8FFB5B1B0FFB4B1AFFFB5B1AFFFB4B0AEFFB2AE
        ADFFB2AEADFFB2AEACFFB1ADABFFB0ACABFFAEABA9FFB4B0AFFFBFBAB7FFBBB6
        B1FFB0ACABFFCAC8C8FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00F4F4F4FFDAD9D9FFC3BFC0FFBCB7B8FFC3BE
        BFFFC8C3C4FFC8C3C5FFC8C3C4FFC7C2C3FFC6C2C3FFC2BDBEFFBCB8B9FFB8B4
        B3FFB9B5B3FFD2CECEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FEFEFEFFEDECECFFD1CF
        D0FFC2BEBFFFC8C5C6FFCCC9C9FFCFCCCDFFD3D1D1FFD7D5D6FFDCDADAFFDFDD
        DDFFE0DFDFFFEDECECFFFFFFFF00FFFFFF00FFFFFF00}
      TabOrder = 1
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = btnStergeProdClick
    end
  end
  object cxSplitter: TcxSplitter
    Left = 250
    Top = 41
    Width = 8
    Height = 322
    HotZoneClassName = 'TcxMediaPlayer9Style'
    Control = pnLeft
    ExplicitHeight = 372
  end
  object cxStyleRepository1: TcxStyleRepository
    Left = 5
    Top = 4
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBlack
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle2: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle3: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 7566195
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle4: TcxStyle
      AssignedValues = [svColor]
      Color = 12937777
    end
    object cxStyle5: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11295531
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle6: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 16247513
      TextColor = clBlack
    end
    object cxStyle7: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 12937777
      TextColor = clWhite
    end
    object cxStyle8: TcxStyle
      AssignedValues = [svColor]
      Color = 15119240
    end
    object cxStyle9: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 4707838
      TextColor = clBlack
    end
    object cxStyle10: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 15120025
      TextColor = clWhite
    end
    object cxStyle11: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle12: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle13: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clSilver
      TextColor = clBlack
    end
    object cxStyle14: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 15461355
      TextColor = clBlack
    end
    object cxStyle15: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle16: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clGray
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle17: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle18: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle19: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle20: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle21: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle22: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle23: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle24: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clSilver
      TextColor = clBlack
    end
    object cxStyle25: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 15461355
      TextColor = clBlack
    end
    object cxStyle26: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle27: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clGray
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle28: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle29: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle30: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle31: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle32: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle33: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle34: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle35: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle36: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clSilver
      TextColor = clBlack
    end
    object cxStyle37: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = 15461355
      TextColor = clBlack
    end
    object cxStyle38: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle39: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clGray
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle40: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle41: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle42: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle43: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle44: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlue
    end
    object cxStyle45: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 85
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle46: TcxStyle
      AssignedValues = [svColor]
      Color = clSilver
    end
    object cxStyle47: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle48: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle49: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle50: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clSilver
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle51: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 15461355
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle52: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle53: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6447714
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle54: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 6908265
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle55: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clSilver
    end
    object cxStyle56: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBlack
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle57: TcxStyle
      AssignedValues = [svColor]
      Color = 13750737
    end
    object cxStyle58: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 10911061
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = [fsBold]
      TextColor = clWhite
    end
    object cxStyle59: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 16119285
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle60: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13750737
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle61: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11579568
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWhite
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle62: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 12097140
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Tahoma'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle63: TcxStyle
    end
    object cxStyle64: TcxStyle
      AssignedValues = [svColor]
      Color = 15591908
    end
    object cxStyle65: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13154717
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle66: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13154717
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle67: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle68: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13154717
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle69: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 14933198
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle70: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13154717
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clBlack
    end
    object cxStyle71: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 11441772
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clWhite
    end
    object cxStyle72: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 13154717
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = [fsBold]
      TextColor = clBlack
    end
    object cxStyle73: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clWhite
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = 9928789
    end
    object cxStyle74: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = 9928789
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'Microsoft Sans Serif'
      Font.Style = []
      TextColor = clWhite
    end
    object GridTableViewStyleSheetUserFormat4: TcxGridTableViewStyleSheet
      Caption = 'UserFormat4'
      Styles.Content = cxStyle23
      Styles.ContentEven = cxStyle24
      Styles.ContentOdd = cxStyle25
      Styles.Footer = cxStyle26
      Styles.Group = cxStyle27
      Styles.GroupByBox = cxStyle28
      Styles.Header = cxStyle29
      Styles.Inactive = cxStyle30
      Styles.Indicator = cxStyle31
      Styles.Preview = cxStyle32
      Styles.Selection = cxStyle33
      BuiltIn = True
    end
    object cxVerticalGridStyleSheetStormVGA: TcxVerticalGridStyleSheet
      Caption = 'Storm (VGA)'
      Styles.Background = cxStyle57
      Styles.Content = cxStyle59
      Styles.Inactive = cxStyle61
      Styles.Selection = cxStyle62
      Styles.Category = cxStyle58
      Styles.Header = cxStyle60
      BuiltIn = True
    end
    object TreeListStyleSheetSlate: TcxTreeListStyleSheet
      Caption = 'Slate'
      Styles.Content = cxStyle67
      Styles.Inactive = cxStyle71
      Styles.Selection = cxStyle74
      Styles.BandBackground = cxStyle64
      Styles.BandHeader = cxStyle65
      Styles.ColumnHeader = cxStyle66
      Styles.ContentEven = cxStyle68
      Styles.ContentOdd = cxStyle69
      Styles.Footer = cxStyle70
      Styles.Indicator = cxStyle72
      Styles.Preview = cxStyle73
      BuiltIn = True
    end
  end
  object ConfMenu: TPopupMenu
    Images = ImgList
    Left = 416
    Top = 6
    object mnuConfigureazaTipMat: TMenuItem
      Caption = 'Configurare Tipuri Materiale'
      ImageIndex = 0
    end
    object mnuDeseleteazaTot: TMenuItem
      Caption = 'Configurare Tip Stock'
      ImageIndex = 1
    end
    object mnuInfluentaStock: TMenuItem
      Caption = 'Configurare Influenta Stock'
      ImageIndex = 2
    end
  end
  object ImgList: TImageList
    Left = 448
    Top = 6
    Bitmap = {
      494C010102000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000F7F7F700F7F7F700EFEFE700CECECE007373BD003939AD003939
      A50052529C00C6C6CE0000000000000000000000000000000000000000000000
      0000F7F7F700E7E7E700CEBDB500BDA59400B59C8C00ADA59C00BDBDBD00DEDE
      DE00F7F7F7000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000CEBDB500B5A5
      9C00A5948C00A5948400A59484008C7B84002121AD002121E7000808F7001818
      F7003131E7002929AD00B5B5CE0000000000000000000000000000000000EFEF
      EF00CEAD9400BD6B3100C65A1000C65A1000CE5A1000C65A1000B55A2100A57B
      5A00B5B5B500EFEFEF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6BDAD00E7CE
      C600EFDED600F7DED600EFD6D6004A42BD006B6BE7008484DE004A4ADE004A4A
      E7008484D6007373DE003131A500E7E7EF000000000000000000EFE7E700C684
      5200CE5A1000D66B1000DE731000DE7B1800DE7B1800DE731000DE6B1000CE63
      1000B5632900ADA59C00EFEFEF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000D6C6BD00F7DE
      DE00F7DED600F7DED600B5A5C6005252D6006B6BF700CECEEF00D6D6E700D6D6
      E700DEDEEF007373F7004A4AD600A5A5C60000000000F7F7F700CE844A00CE63
      1000D67B2100BD9C7B00BD9C8400B5845A00C67B3100CE7B3100D67B2100DE7B
      1800D6631000B5632100BDB5B500F7F7F7000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000D6C6BD00F7DE
      D600EFDED600EFD6CE009484BD005A5AE7006B6BFF008484EF0000000000F7F7
      F7009494DE007373FF005A5AE7008484C60000000000DEAD8C00CE631000E77B
      1000D6BD9C00F7F7F700E7DED600D6CEC600C6B5AD00BD8C4A00C68C3900CE84
      3100DE7B1800CE631000B5734A00DEDEDE000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000DECEC600EFDE
      D600EFD6CE00E7D6CE00948CBD004A4ADE006B6BFF00ADADE700F7F7F700F7F7
      F700B5B5D6007373F7004A4ADE009494CE00F7F7EF00C66B2100E77B1000DE84
      2100E7D6CE00D6D6CE00BD8C4A00C6944200BD945A00BDA57300C6944200CE94
      4200D68C3100E77B1800BD5A1000C6BDB5000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000DED6CE00EFD6
      CE00E7D6CE00E7CEC600C6B5BD003931C6007373EF00D6D6E700D6D6E700CECE
      E700D6D6E7008484E7003939C600D6D6E700EFD6BD00CE631000EF841800DE94
      3100DED6C600BDB5A500B5945A00CE9C4200CE9C4A00C69C4200C69C4A00CE9C
      4A00D69C3900E78C2100CE631000BD947B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000DED6CE00E7CE
      C600E7CEC600DECEC600DEC6BD00736BB5005252DE00A5A5FF00D6D6FF00CECE
      FF00A5A5FF005252DE007B7BCE0000000000E7B59400D66B1000E78C2100C6A5
      6B00E7E7DE00DEDEDE00BDA57300D6AD4A00D6A54A00C69C5200BDB59C00CEA5
      4200D6A54200E7942900D6731000C68C63000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000DED6D600DECE
      C600DEC6BD00DEC6BD00D6BDB500CEBDB5007363AD003131C6005A5AD6005A5A
      D6003131C6007B73BD00F7F7F70000000000DEAD8C00D66B1000E7942100D6A5
      4200D6C69C00E7E7E700D6C69400DEBD6300DEB54A00D6BD7B00E7E7E700BDB5
      9C00CEAD4A00E79C3100DE731000C68C63000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000E7DEDE00DEC6
      BD00D6C6BD00D6BDB500D6BDB500CEB5AD00CEB5AD00BDA5AD009484A5009484
      A500A5949400D6CEBD000000000000000000E7C6AD00CE631000EF9C2900DEAD
      5200DEC68400D6CEC600EFEFD600EFDEAD00DEC68400CEC6A500EFEFEF00C6BD
      9C00CEB56300E7A53900D66B1800CEA584000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000E7E7DE00D6BD
      B500D6BDB500CEB5B500CEB5AD00CEB5AD00C6B5AD00C6ADA500C6ADA500BDAD
      A500B59C9400CEC6BD000000000000000000F7E7DE00C65A1000EF942900E7B5
      5A00E7D69C00E7DECE00EFEFE700F7EFDE00E7D6AD00CEB56B00E7E7DE00BDAD
      9400DEB56300E79C4200C65A1000E7D6C6000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000E7E7E700CEB5
      AD00CEB5AD00CEB5AD00C6B5AD00C6ADA500C6ADA500BDADA500BDADA500BDA5
      9C00AD9C9400CEC6B5000000000000000000FFFFF700D68C5A00D6732100E7A5
      5200E7CE9400E7DEBD00CECEC600D6CEBD00C6BD9400BDB59400EFEFEF00BDB5
      9C00DEB56300DE842900CE7B4200F7F7F7000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000EFEFE700A584
      730094635200946B5A009C736300A5736300A57B6B00A5847300A5847300AD8C
      7B00A58C8400C6B5AD00000000000000000000000000F7E7DE00C6632100DE84
      3100E7B57300E7CEA500DECEAD00D6CEBD00E7E7E700EFEFEF00F7F7F700CEAD
      8400DE944200C6631800EFD6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000F7F7EF00BDA5
      9400A5735A00A56B5200A56B5200A56B5200A56B5200A56B5200A56B5200A573
      5A00A58C8400C6BDB50000000000000000000000000000000000EFCEBD00C663
      1800D67B3100E7A56B00E7B58C00DEBD9C00D6BD9400D6BDA500DEAD7B00D684
      3900C6631800E7BDA50000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000F7F7F700E7D6
      CE00DECEBD00D6B5A500CE9C8400CE947B00C6947300C68C7300BD947B00C6AD
      9400D6BDAD00CEC6B5000000000000000000000000000000000000000000EFDE
      D600D6845200C6631800D6732900D6843900D6844200D67B3100C6631800CE7B
      4200EFD6C6000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000EFDEDE00D6B59C00DEBDAD00F7EFEF00EFE7DE00E7DE
      DE00E7DED600E7DED60000000000000000000000000000000000000000000000
      0000FFF7F700EFDECE00DEAD8C00D68C5A00D68C5200DEAD8400EFD6C600F7F7
      F700000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00F803F00700000000C001E00300000000
      C000C00100000000C000800000000000C020800000000000C000000000000000
      C000000000000000C001000000000000C001000000000000C003000000000000
      C003000000000000C003000000000000C003800100000000C003C00300000000
      C003E00700000000FC03F00F0000000000000000000000000000000000000000
      000000000000}
  end
  object DTTipStoc: TDataSource
    DataSet = QryTipStoc
    Left = 744
    Top = 8
  end
  object QryTipStoc: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_TIP_STOC')
    Params = <>
    Left = 776
    Top = 8
  end
  object DTGrupaStoc: TDataSource
    DataSet = QryGrupaStoc
    Left = 24
    Top = 144
  end
  object QryGrupaStoc: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_GRUPA_STOC')
    Params = <>
    Left = 56
    Top = 144
  end
  object DTNivelStoc: TDataSource
    DataSet = QryNivelStoc
    Left = 8
    Top = 488
  end
  object QryNivelStoc: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_NIVEL_STOC')
    Params = <>
    Left = 40
    Top = 488
  end
  object cxGridPopupMenu1: TcxGridPopupMenu
    PopupMenus = <>
    Left = 360
  end
end
