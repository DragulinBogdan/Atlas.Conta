object frmALOPInfo: TfrmALOPInfo
  Left = 324
  Top = 319
  ClientHeight = 302
  ClientWidth = 715
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object cxPageInfo: TcxPageControl
    Left = 0
    Top = 0
    Width = 715
    Height = 302
    Align = alClient
    TabOrder = 0
    Properties.ActivePage = tabAngajament
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabSlants.Kind = skCutCorner
    Properties.TabSlants.Positions = [spLeft, spRight]
    ClientRectBottom = 302
    ClientRectRight = 715
    ClientRectTop = 20
    object tabBuget: TcxTabSheet
      Caption = 'Buget'
      ImageIndex = 0
    end
    object tabAngajament: TcxTabSheet
      Caption = 'Angajament'
      ImageIndex = 1
      DesignSize = (
        715
        282)
      object LbPredator: TLabel
        Left = 9
        Top = 60
        Width = 143
        Height = 13
        Caption = 'Departament Angajament'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object lbDocument: TLabel
        Left = 10
        Top = 12
        Width = 88
        Height = 13
        Caption = 'Nr. Angajament'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object LbDataNota: TLabel
        Left = 121
        Top = 12
        Width = 63
        Height = 13
        Caption = 'Din Data : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object LbScopul: TLabel
        Left = 239
        Top = 12
        Width = 89
        Height = 13
        Caption = 'Tip Angajament'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object LbPrimitor: TLabel
        Left = 241
        Top = 60
        Width = 46
        Height = 13
        Caption = 'Furnizor'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label5: TLabel
        Left = 415
        Top = 12
        Width = 56
        Height = 13
        Caption = 'Explicatie'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edPredator: TcxPopupEdit
        Left = 12
        Top = 75
        Hint = 
          'Departamentul/Biroul care reprezinta partea tehnica a angajament' +
          'ului'
        AutoSize = False
        ParentFont = False
        Properties.PopupControl = cxTreeRepartitori
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseUp = edPredatorPropertiesCloseUp
        Properties.OnInitPopup = edPredatorPropertiesInitPopup
        Properties.OnPopup = edPredatorPropertiesPopup
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        StyleDisabled.TextColor = clBlack
        TabOrder = 0
        OnKeyDown = edPredatorKeyDown
        Height = 21
        Width = 217
      end
      object edPrimitor: TcxPopupEdit
        Left = 241
        Top = 75
        Hint = 'Furnizorul/entitatea externa cu care se incheie angajamentul'
        AutoSize = False
        ParentFont = False
        Properties.PopupControl = cxTreeRepartitori
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseUp = edPredatorPropertiesCloseUp
        Properties.OnInitPopup = edPredatorPropertiesInitPopup
        Properties.OnPopup = edPredatorPropertiesPopup
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        StyleDisabled.TextColor = clBlack
        TabOrder = 4
        OnKeyDown = edPredatorKeyDown
        Height = 21
        Width = 247
      end
      object btnSaveAng: TcxButton
        Left = 607
        Top = 225
        Width = 83
        Height = 27
        Anchors = [akRight, akBottom]
        Caption = 'Salvare'
        Enabled = False
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
        TabOrder = 5
        OnClick = btnSaveAngClick
      end
      object edNumarDoc: TcxDBButtonEdit
        Left = 13
        Top = 28
        Hint = 'Numarul de angajament'
        DataBinding.DataField = 'NUMAR'
        DataBinding.DataSource = DTAng
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.OnButtonClick = edNumarDocPropertiesButtonClick
        Style.Color = clCream
        TabOrder = 1
        Width = 105
      end
      object edDataDoc: TcxDBDateEdit
        Left = 128
        Top = 28
        Hint = 'Data Angajamentului'
        DataBinding.DataField = 'DATA_EMITERE'
        DataBinding.DataSource = DTAng
        Properties.DateOnError = deNull
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        Style.Color = clCream
        TabOrder = 2
        Width = 104
      end
      object edTipAngajament: TcxDBImageComboBox
        Left = 247
        Top = 28
        Hint = 'Tipul de angajament'
        DataBinding.DataField = 'TIP_ANGAJAMENT'
        DataBinding.DataSource = DTAng
        Properties.Alignment.Horz = taLeftJustify
        Properties.Items = <>
        Style.Color = clCream
        TabOrder = 3
        Width = 153
      end
      object cxTreeRepartitori: TcxDBTreeList
        Left = 24
        Top = 136
        Width = 489
        Height = 105
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = frmData.DTRepartitori
        DataController.ParentField = 'ID_REPARTITORI'
        DataController.KeyField = 'ID_REPARTITORI'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragExpand = False
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
        OptionsView.CellTextMaxLineCount = -1
        OptionsView.ShowEditButtons = ecsbFocused
        OptionsView.ColumnAutoWidth = True
        ParentColor = False
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 6
        Visible = False
        OnDblClick = cxTreeRepartitoriDblClick
        OnKeyDown = cxTreeRepartitoriKeyDown
        object cxTreeRepartitoriNUME: TcxDBTreeListColumn
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'NUME'
          Width = 263
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeRepartitoriADRESA: TcxDBTreeListColumn
          Caption.Text = 'Adresa'
          DataBinding.FieldName = 'ADRESA'
          Width = 61
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeRepartitoriCONT: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeRepartitoriCODFISC: TcxDBTreeListColumn
          Caption.Text = 'Cod Fiscal'
          DataBinding.FieldName = 'CODFISC'
          Width = 59
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeRepartitoriGESTINT: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Tip Gestiune'
          DataBinding.FieldName = 'GESTINT'
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object edtScop: TcxDBTextEdit
        Left = 414
        Top = 30
        Hint = 'Descriere Angajament'
        Anchors = [akLeft, akTop, akRight]
        AutoSize = False
        DataBinding.DataField = 'SCOPUL'
        DataBinding.DataSource = DTAng
        Style.Color = clCream
        TabOrder = 7
        Height = 21
        Width = 291
      end
    end
    object tabOrdonantare: TcxTabSheet
      Caption = 'Ordonantare'
      ImageIndex = 2
      DesignSize = (
        715
        282)
      object lbNrOrdonantare: TLabel
        Left = 6
        Top = 12
        Width = 89
        Height = 13
        Caption = 'Nr Ordonatare :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object lbDataEmitere: TLabel
        Left = 238
        Top = 12
        Width = 82
        Height = 13
        Caption = 'Data Emitere :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object lbNrOrdine: TLabel
        Left = 453
        Top = 12
        Width = 113
        Height = 13
        Caption = 'Nr Ordine din data :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label1: TLabel
        Left = 5
        Top = 46
        Width = 107
        Height = 13
        Caption = 'Detalii Angajament'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label2: TLabel
        Left = 238
        Top = 45
        Width = 92
        Height = 13
        Caption = 'Nr Angajament :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label3: TLabel
        Left = 459
        Top = 45
        Width = 106
        Height = 13
        Caption = 'Data Angajament :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label4: TLabel
        Left = 5
        Top = 78
        Width = 98
        Height = 13
        Caption = 'Natura Cheltuielii'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edtNrOrdonantare: TcxDBButtonEdit
        Left = 96
        Top = 8
        DataBinding.DataField = 'NUMAR'
        Properties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.MaxLength = 0
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 0
        Width = 121
      end
      object edtDataEmitere: TcxDBDateEdit
        Left = 321
        Top = 8
        DataBinding.DataField = 'DATA_EMITERE'
        Properties.ImmediatePost = True
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 1
        Width = 121
      end
      object edtNrOrdine: TcxDBSpinEdit
        Left = 568
        Top = 8
        DataBinding.DataField = 'NR_ORDINE'
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 2
        Width = 81
      end
      object btnAngajamentOrd: TcxButton
        Left = 115
        Top = 38
        Width = 121
        Height = 27
        Caption = 'Sel Angajament'
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360900000000000036000000280000001800000018000000010020000000
          000000000000C40E0000C40E0000000000000000000000000000000000000000
          0000000000000000000000000000000000000000000000000000000000000000
          000000000000000000000101010212121229322E2A754A413AA5474039AA2F2A
          278A100F0E470202020F00000000000000000000000000000000000000000000
          0000AB772CE6B87638F9B8783AF9B8783BF9B8783BF9B8783BF9B8783BF9B878
          3BF9B8783BF9B5773AF9916E4BF9AA9279FECAA77FFFD3AE81FFD3B086FFCAAC
          89FFAB957FFF473F36C30101012E000000010000000000000000000000000000
          0000BB8739ECDC9E60FFDCA264FFDCA366FFDCA366FFDCA366FFDCA366FFDCA3
          66FFDAA166FFA18468FFB99A76FFD4AA79FFDAB38BFFE0BC9DFFE0BD9FFFDCB5
          8FFFD5AE80FFB89771FF60544ADF010101340000000100000000000000000000
          0000BC8B3DECE1A86EFFE1AD74FFE1AF76FFE1AF76FFE1AF76FFE1AF76FFE1AF
          76FFB69470FFB19A80FFD2A877FFD7B182FFDE937AFFD70F04FFD71004FFDD68
          56FFDBB78AFFD1A570FFB3926BFF3C352FC10000001500000000000000000000
          0000BF8E43ECE7B37DFFE7B984FFE7BB86FFE7BB86FFE7BB86FFE7BB86FFE2B8
          86FFAD9882FFE1C5A5FFE1BE96FFE2C199FFE9D5BCFFD71004FFD71205FFE5A8
          95FFDEBC92FFD7AA76FFD1A46EFF97826CFE0605045800000000000000000000
          0000C09146ECECC08CFFECC594FFECC797FFECC797FFECC797FFECC797FFD1B5
          91FFC5BAACFFF2E3D1FFF1DDC7FFEBD1B2FFEDDAC4FFD71004FFD71205FFE8AB
          9AFFE4C49DFFDEB484FFDBAF7CFFBA9A74FF211D1A9C00000000000000000000
          0000C1954BECF1CA9BFFF1D0A4FFF1D2A7FFF1D2A7FFF1D2A7FFF1D2A8FFCAB5
          9AFFD6D1CCFFFAF5EEFFFBF3EAFFF6E6D4FFF4E4D5FFD71004FFD71205FFEAAE
          9FFFE8CAA7FFE4BE92FFE5BF92FFC9AC83FF312C27B600000000000000000000
          0000C39950ECF7D7ABFFF7DDB4FFF7DFB8FFF7DFB8FFF7DFB8FFF7DFB8FFCFBD
          A4FFD7D2CEFFFBF9F6FFFDF8F3FFFBF5EDFFF8E4DCFFD71004FFD71205FFEEB3
          A6FFEFD5B8FFECCCA7FFECCCA4FFC7B490FF302B26B500000000000000000000
          0000C49B54ECFBDFB8FFFBE6C3FFFBE8C7FFFBE8C7FFFBE8C7FFFBE8C7FFE0D1
          B6FFC5BDB8FFF9F7F5FFFEFBF9FFFCF9F6FFF3BDB9FFDD362CFFD71105FFEFB1
          A8FFF6E4CFFFF7E1C0FFEADCB8FFB3AA91FF1A17149100000000000000000000
          0000C59D58ECFEE6C3FFFEEDCEFFFEEFD2FFFEEFD2FFFEEFD2FFFEEFD2FFF9EA
          CFFFB0A59AFFEBEAE8FFFDFBF9FFFEFDFBFFFEFAF9FFEA8782FFE7736DFFF9DE
          DAFFFDFAF4FFF9F6E6FFDBD9C8FF897D71FA0302024900000000000000000000
          0000C59F5AECFFEBCAFFFFF2D6FFFFF4DAFFFFF4DAFFFFF4DAFFFFF4DAFFFEF4
          D9FFD2C6B2FFB6AEA7FFF2F0EFFFFDFBF9FFFEF8F6FFDA2920FFD71105FFF1B3
          B0FFFDFCFAFFEBE9E8FFADA59FFF282420AA0000000D00000000000000000000
          0000C5A05CECFFEECFFFFFF5DBFFFFF7DFFFFFF7DFFFFFF7DFFFFFF7DFFFFFF7
          DFFFFDF5DDFFC1B7A6FFB0A7A0FFE6E4E3FFF7F6F5FFFBF5F4FFF7DDDBFFF9F8
          F7FFE8E7E6FFADA59EFF413A34C3000000200000000000000000000000000000
          0000C5A15DECFFEFD3FFFFF6DEFFFFF9E3FFFFF9E3FFFFF9E3FFFFF9E3FFFFF9
          E3FFFFF9E3FFFCF7E1FFCFC7B5FFA4998EFFB4ACA5FFC9C4BFFFCAC5C1FFB9B1
          ABFF9A8F84FF322B22A80000001C000000000000000000000000000000000000
          0000C5A15EECFFF1D8FFFFF8E3FFFFFBE8FFFFFBE8FFFFFBE8FFFFFBE8FFFFFB
          E8FFFFFBE8FFFFFBE8FFFFFBE8FFF9F5E2FFDDD8C8FFC1BAACFFBAB3A5FFC7C0
          B1FFE4D8C1FF271B0A5C00000000000000000000000000000000000000000000
          0000C5A25FECFFF2DBFFFFF9E7FFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFC
          ECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFCECFFFFFA
          E9FFFDF1D9FF39260D5B00000000000000000000000000000000000000000000
          0000C5A260ECFFF3DEFFFFFAEAFFFFFDEFFFFFFDEFFFFFFDEFFFFFFDEFFFFFFD
          EFFFFFFDEFFFFFFDEFFFFFFDEFFFFFFDEFFFFEFCEFFFFEFCEFFFFFFDEFFFFFFB
          EBFFFCF0DBFF3325115C00000000000000000000000000000000000000000000
          0000C5A361ECFFF4E1FFFFFBEDFFFFFEF2FFFFFEF2FFFFFEF2FFFFFEF2FFFFFE
          F2FFFFFEF2FFFFFEF2FFFFFEF2FFFCFBEEFFF9F7EAFFF8F6E9FFFAF8EBFFFBF7
          E8FFF8ECD8FF3426125F01010003000000010000000000000000000000000000
          0000C5A361ECFFF4E3FFFFFBF0FFFFFEF5FFFFFEF5FFFFFEF5FFFFFEF5FFFFFE
          F5FFFFFEF5FFFFFEF5FFFDFCF2FFF5F2E6FFEAE4D4FFE4DDCBFFE6E0CFFFEAE2
          D0FFEBDDC7FF3627126403020107000000010000000000000000000000000000
          0000C5A362ECFFF4E5FFFFFBF2FFFFFEF7FFFFFEF7FFFFFEF7FFFFFEF7FFFFFE
          F7FFFFFEF7FFFFFEF7FFFBFAF1FFEBE6D9FFD3C6ADFFC5B79EFFC8B9A0FFD3C4
          ADFFDFCAB0FF2E210F5703020107000000010000000000000000000000000000
          0000C5A363ECFFF4E7FFFFFBF4FFFFFEF9FFFFFEF9FFFFFEF9FFFFFEF9FFFFFE
          F9FFFFFEF9FFFFFEF9FFF9F7F0FFE3DCCEFFD5B380FFD3AB70FFBCA071FFC9AB
          7BFF684F27A20806031202010104000000000000000000000000000000000000
          0000C5A263ECFFF3E7FFFFFAF3FFFFFCF8FFFFFCF9FFFFFCF9FFFFFCF9FFFFFC
          F9FFFFFCF9FFFFFCF9FFF9F5F0FFE3DCCFFFE5C48DFFFEE5C1FFFEE3BFFF8A6D
          3EBC0E0B051F0302010700000001000000000000000000000000000000000000
          0000C5A162ECFFF0E2FFFFF5ECFFFFF8F1FFFFF8F1FFFFF8F1FFFFF8F1FFFFF8
          F1FFFFF8F1FFFFF8F1FFFBF3EBFFECE1D4FFE9C995FFFFEBD0FFA0885DCA140E
          062A0503010A0000000100000000000000000000000000000000000000000000
          0000B7924EE2E3C9A5F4E3CCAAF4E3CEACF4E3CEACF4E3CEACF4E3CEACF4E3CE
          ACF4E3CEACF4E3CEACF4E0CBAAF4DBC4A3F4CCA972F4C5A678E3140F06290504
          020B010100020000000000000000000000000000000000000000000000000000
          0000000000000000000000000000000000000000000000000000000000000000
          0000000000000000000000000000010100020202010504030108030201060101
          00020000000000000000000000000000000000000000}
        TabOrder = 3
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        WordWrap = True
      end
      object edtNrAngajament: TcxDBTextEdit
        Left = 332
        Top = 40
        DataBinding.DataField = 'NR_ANGAJAMENT'
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 4
        Width = 121
      end
      object edtDataAngajament: TcxDBDateEdit
        Left = 572
        Top = 40
        DataBinding.DataField = 'DATA_ANGAJAMENT'
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 5
        Width = 121
      end
      object edtNaturaCheltuielii: TcxDBMemo
        Left = 112
        Top = 74
        Anchors = [akLeft, akTop, akRight]
        DataBinding.DataField = 'NATURA_CHELTUIELII'
        Style.Color = clCream
        StyleDisabled.Color = clWindow
        TabOrder = 6
        Height = 35
        Width = 593
      end
    end
  end
  object DTAng: TDataSource
    DataSet = qryAng
    OnDataChange = DTAngDataChange
    Left = 344
    Top = 120
  end
  object qryAng: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryAngAfterOpen
    OnNewRecord = qryAngNewRecord
    SQL.Strings = (
      'select * from alop_angajamente where id_alop_angajamente = '
      
        '  (select id_alop_angajamente from alop_angajamente_Defalcare wh' +
        'ere id_alop_angajamente_defalcare =:idAng)')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idAng'
        ParamType = ptUnknown
      end>
    Left = 376
    Top = 120
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idAng'
        ParamType = ptUnknown
      end>
  end
  object DTAngD: TDataSource
    DataSet = qryAngD
    OnDataChange = DTAngDataChange
    Left = 344
    Top = 144
  end
  object qryAngD: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = qryAngDNewRecord
    SQL.Strings = (
      
        'select * from alop_angajamente_Defalcare where id_alop_angajamen' +
        'te_defalcare =:idAng')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idAng'
        ParamType = ptUnknown
      end>
    Left = 376
    Top = 144
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idAng'
        ParamType = ptUnknown
      end>
  end
end
