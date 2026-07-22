object frmFunctii: TfrmFunctii
  Left = 194
  Top = 227
  Caption = 'Nomenclator Departamente'
  ClientHeight = 602
  ClientWidth = 936
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
    Left = 732
    Top = 73
    Height = 510
    Align = alRight
    ExplicitLeft = 429
    ExplicitTop = 79
    ExplicitHeight = 342
  end
  object PageDeps: TcxPageControl
    Left = 0
    Top = 73
    Width = 732
    Height = 510
    Align = alClient
    TabOrder = 0
    Properties.ActivePage = tabOrganigrama
    Properties.CustomButtons.Buttons = <>
    ClientRectBottom = 510
    ClientRectRight = 732
    ClientRectTop = 24
    object tabTree: TcxTabSheet
      Caption = 'Arbore Functii'
      object TreeDepartamente: TcxDBTreeListEx
        Left = 0
        Top = 0
        Width = 732
        Height = 486
        Align = alClient
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = frmData.DTFunctiuni
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID_FUNCTIUNI'
        DragMode = dmAutomatic
        Images = ImaginiFunctii
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragFocusing = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ShowHourGlass = False
        OptionsSelection.HideFocusRect = False
        OptionsSelection.InvertSelect = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.Indicator = True
        RootValue = Null
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        OnGetNodeImageIndex = TreeDepartamenteGetNodeImageIndex
        ExplicitLeft = -3
        ExplicitTop = -6
        object TreeDepartamenteDENUMIRE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignHorz = taCenter
          Caption.AlignVert = vaTop
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'DENUMIRE'
          Options.Editing = False
          Width = 126
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDepartamenteDATA_INTRARE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Caption.AlignHorz = taCenter
          Caption.AlignVert = vaTop
          Caption.Text = 'Data Intr.'
          DataBinding.FieldName = 'DATA_INTRARE'
          Options.Editing = False
          Width = 76
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDepartamenteDATA_IESIRE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Caption.AlignHorz = taCenter
          Caption.AlignVert = vaTop
          Caption.Text = 'Data Ies.'
          DataBinding.FieldName = 'DATA_IESIRE'
          Options.Editing = False
          Width = 71
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDepartamenteTELEFON: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = '0'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignHorz = taCenter
          Caption.AlignVert = vaTop
          Caption.Text = 'Cod Functie'
          DataBinding.FieldName = 'COD_FUNCTIE'
          Options.Editing = False
          Width = 142
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDepartamenteSTARE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          DataBinding.FieldName = 'STARE'
          Options.Editing = False
          Width = 691
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object tabOrganigrama: TcxTabSheet
      Caption = 'Organigrama'
      ImageIndex = 1
      object Organigrama: TdxDbOrgChart
        Left = 0
        Top = 0
        Width = 732
        Height = 486
        DataSource = frmData.DTFunctiuni
        KeyFieldName = 'ID_FUNCTIUNI'
        ParentFieldName = 'ID_PARINTE'
        TextFieldName = 'DENUMIRE'
        OrderFieldName = 'POS_ID'
        OnLoadNode = OrganigramaLoadNode
        DefaultNodeWidth = 80
        DefaultNodeHeight = 60
        Options = [ocSelect, ocFocus, ocButtons, ocDblClick, ocEdit, ocCanDrag, ocShowDrag, ocInsDel]
        EditMode = [emCenter, emVCenter, emWrap]
        DefaultImageAlign = iaLT
        Align = alClient
        Color = clDefault
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = []
        ParentFont = False
      end
    end
  end
  object StatusBar: TStatusBar
    Left = 0
    Top = 583
    Width = 936
    Height = 19
    Panels = <>
  end
  object pnTools: TPanel
    Left = 0
    Top = 41
    Width = 936
    Height = 32
    Align = alTop
    TabOrder = 2
    object BtnAddDep: TcxButton
      Left = 7
      Top = 4
      Width = 110
      Height = 22
      Caption = 'Functie Noua'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
        610000001B744558745469746C65004164643B506C75733B426172733B526962
        626F6E3B9506332F0000036349444154785E35927D6C535518C69F73EE6DEB64
        63A3AEFB60A3A36E33B8C56581E0D8707E21CC1A43A2A22304FE3001512A86C4
        E900132451FF503367420043B244364C483031465C248B4441C0980C45B4D065
        CDBA4ECAE82AAC5DBBDE8FF3E1BD27F1397973DE9C3CBFF7233964226FC2D543
        A53E0280443E3FD752525AB14323FA06685A3381E492F329C6ADF39954E2F8C9
        C3DBA6018858DE940A9C2C5870C1D51BB6FAF61DBB327860F81A1BFE25297FB8
        3127C7EFE4E5D5745E9EBB9991239766E481937FE4DE1818DB0DC0EB322EABBA
        B63FD5EB7D6CCBBE6F1B83FE9E67BA82E084C0E4123697CAE0D109BC94805B0C
        E7AFCC606A66EEECF75FBCBB753AFAEB2201A0BD3E7861B02914D8DBF34408A9
        AC0D2181D3672E23319D81AB950D016CEBED824E809A722FC62E4CE17A343130
        D4DF73507FB9FFAB551E9F6FCF93EB82B879BB088D52504A14FCC9CE4E95F79D
        B80CD396284A8179C7D3DD1144F29FEC5BE1D73E1BA6BEB2C09BEDCD955A7CCE
        44D1744C1687C9045C05EBFC686F0DAADCB08413D2098E89B4E1BC5779965687
        5ED585D03ACBFDA548E7197EFA711C776EDFC5FF12200A7075F4E85975D7D4FA
        F1F4A635A82C5F02A2956CD46D2EEB1D160B455BC19FEE5E0F4A885A45828071
        81137D1B61DB0C1E5D43E4C8CF5858E4D0A1810BBA5CB76DEEBDB768C1E604AE
        EA6B1F40D9121F0A265385BC0E5457530109404A8010E27805EEE60598CDA15B
        8699C8E7CD4784EEC3F2BA00767C340A4AA9327E79300CE1505BDEFF0E9AA681
        5082150DD5604CA26858282E1693D428E42F6666B3909068EF68C5E6171FC7E6
        17BA611A260C93A9029C713CF7FC3A3C1BEE404B5B2398E0989FCBA190FD774C
        CFA46243B11B4B77ADADF67BB236478E10500AA5D2121D5C48354D3A674108A1
        56114C201E4BB1D9F86FA70880FB1EDD3E34B0A229B4E7E1350FC2E22E2011BF
        16C3FCBD050557562DC3CA964608B8B4C4E49F4924A27F1F193F1DD9AF03B0FE
        1AFDE03D113EDC6431B1A96575089212B4AD6D555F581280D902398343308EC9
        EB49DC9A981A75E043000CA46D09005A49457059DB4BC78E77EDFCDAEAFDF892
        DC3B1295EF7C13977D4E444E45E52BCE5BE7AE338555E10FDF0650EE32B30E4B
        D24C0212A8F210EAAED3D01969BB3FD0BCDDE32BEB06D56AD5D09CCDDA66EE62
        EED6EF43A9AB2331008603ABCEFF019D3AAD15CCD8D2E00000000049454E44AE
        426082}
      TabOrder = 0
      OnClick = BtnAddDepClick
    end
    object BtnAddSubDep: TcxButton
      Left = 121
      Top = 4
      Width = 110
      Height = 22
      Hint = 'Creaza functie subordonata pentru pozitia curenta'
      Caption = 'Subordonata Noua'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
        610000002C744558745469746C65004164643B4974656D3B4164644974656D3B
        426172733B526962626F6E3B4974656D3B506C75734E32EF8100000286494441
        54785E5D915D48545B14C77FFB9C199D11CDA264A2C828B2A44891B844491924
        A441915020611045742F04D14B11D1439015193E28652541057D125CEFBDF8D0
        43D017D8BDE0EDC1BC57ED8331106AC6907072C673CEDEAB98738686D9ECBDD8
        9BCDFFBF7E6B2D05D8B7FF1C7962DB768320002020E2DF252F8880E7382F0EEE
        A9691411072004584AA986BDDB5700148A0005FC34BBD537BA09B00172064A6B
        01C0714D8E01113F0882BF15C5610BCF330056BE01DA8000264F1DE803A2E01F
        411B0D4001819BAB3B10E5F74005068288CA11A8020300C1989F225028019409
        5E0631B0A8A204C06ABBF88AD9B4830528CFD501B2600013D4EE89C7B3A1043D
        8FE3B4FF1EA7AB3F4E32AD299BBF387AE7C4068504CD7082261A14C677427B2E
        97FF7ACF6832C3968D0BF9AD75395BEA63A4EC302DC7EFFFB1B8BA3EFAE84C83
        CA1218CF030163C4CF2E86FE810954B14D63C312929922B6ED7F80A722346FAD
        64CDCA8AF5EB769E3C05D85903D71F9F2F3682D6FA077A92BADA1803EF1C1229
        8D339B617246F37ADCA5B6268656E17D40289435D006117F9C205862484CCEF0
        E1AB45C7B9BB686310ED71F8C8358CD6F474FD8AEB780B013B9882017C7CB225
        0891B0E1E3C4341DED6D84141C3A7A8587D78F90CA0883EFA648CFA43FE7C628
        3982D2888D88C218F8A56A0EFF0CC50947A3D8B6C24967F8EFB3CB976F9A91D7
        1F989EFAD20778214067BEA59E9FEE7CB9591044A0BC2CCCD2452564129FF8F7
        E9104B5757D2DD7D8CBFDF24181F1E67622C3EFC71F0DE59C005B08028500ECC
        05E60567412852BEACBAE94267CDEE1B6375AD77646D4BEFDB554DE72F1595C6
        2A8050F58E5E948850B856EFBA094AF8BFEF80058481A22091011CC0AD6ABE6A
        44E03B805C64CDB4C3E1300000000049454E44AE426082}
      TabOrder = 1
      OnClick = BtnAddSubDepClick
    end
    object BtnDelDepartament: TcxButton
      Left = 235
      Top = 4
      Width = 110
      Height = 22
      Caption = 'Sterge Functie'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
        6100000023744558745469746C650043616E63656C3B53746F703B457869743B
        426172733B526962626F6E3B4C9696B20000038849444154785E1D906B4C5367
        18C7FF3DBD40CB1A2E32B55C9D598B4CA675D8D13836652E9B0359B67D589665
        3259E644A52571644474CB4CB6ECC23770C4449DD38D2885005E4683AB69C616
        8DA12384264EC8AAAC0C9149A1175ACEE9E939CFDE9EE7E477F2CBFFB924E720
        E6E943CC3B8895D12B00A0FEE3D08167A75A5BBAEEB71D9D081E6B4DA549FBDD
        A3CEEFDD1F3658016818AA98A71FD1915E202DE980A19D741E3EF6E0F8A7FC7F
        673B6979E002C5BC43B4C2581EB8480BE7BA68E6441BEF3B72F03300990C8E1D
        5016554E7B55D6C1ED9543C6C2B5BB739FDF025988838424E4240F10A0D2EAA0
        D26540AD37203CFE17C2C187A3EDBFDE7CF3DAD4748403A06EA8A8E830AC5FB3
        3B7BAB1901B717AE23DFE1CEC5EBEC90A0E0EB71A3CFD981C0B017C6F252180B
        D6BD74BCFA856E003A0CBDFD966DF250532AD4FF038DB734D18557DF21CFB08F
        2E37B5D370ED5E72D7D52BEEF9654CE9F91C1FD392EB0C4D3A0E4BE7F6ECD909
        CFDEFAD381AF4ED0A3D35FD399E272BA3F3D478F971234FD2044BDCE930AF798
        CF2FAED0DF5373CACCFCA92F2970B29DDCAFD7F56B48945E918201C41738945A
        2D581C7461ADA3192AB50AD64F9A010272730CC8D4AA313BE44289D58CF85D3F
        2411504BB28D93845489145E041F9CC1863C09A11BD7E1EFEA86240339463DB2
        B3F59025C0DFD98DD0C83594E6886C360831F408523265D208BC0021B20A35A7
        82B8BC0429C2239A10D812417988007088B14C8A8421EA75A094044A8A48F200
        17E78587629220B370E69F2884EA3750F07E23245946868E43A64EA3B8695F23
        F8EA7A046763EC780AC9640AF155FEB1269AE0BD91AC8CFDF910108E26F15A5B
        33788D1E860CF6CDE7CF225D45FB3F02A0C7CE36076E5CBD84825C3562A20E4B
        097E0CAD051B5FFCA97C9BE4ABAEA05B2FDBE9E6BE0F880F8568FCDB0E1AA9AA
        646C579C654AEF564D15FDB96333FDBCC94A8E751B6A0140DF5168B9E42A7B86
        266AB6D2ED1A1BF559CAC853B58DFCB576F2D7D9D3AE64B777D96862D716EA2F
        2BA76F4CE62B008C1A00C2F9C57F9D8DA2C99212C5E72C85323699F320A77FD2
        72040021DF9885F56BF2204457706F9EC74C4CF2F744169A012430DBF21E00A8
        2B754F98BEC82EEEED7AF2291A306FA451EBD3346633938FF13BF341969D62BD
        CF738AAF6ED6EA4B006882CE77A14ABFD255D2799903606830E4EF28E274070C
        1C67D74255041044C25C9CE43B4149F8B16735F41B8038DB9300E07F6924ECFB
        01D589CC0000000049454E44AE426082}
      TabOrder = 2
      OnClick = BtnDelDepartamentClick
    end
    object BtnUtilizatori: TcxButton
      Left = 349
      Top = 4
      Width = 110
      Height = 22
      Caption = 'Utilizator Asociati'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
        610000001974455874536F6674776172650041646F626520496D616765526561
        647971C9653C00000011744558745469746C65004465706172746D656E743B97
        2A9DC10000035D49444154785E7D927D4CD4751CC7DFF7BB1F46543270F36A88
        4EF14C4BA20D708CBA2089120A56D464B23593E2411256EA690F73D0863D58D9
        1FECD810179378D87235B90A51DC8910B31BB79A047887811258E28D79D71DFC
        1EEFDEFDE6FA83ADD56B7BFDF5DDE7F37DBFBFFB9A48E297AE02A8B29E2B4BFA
        2E45D29E93241D4B8BEAB9A5B0DEBDB8A80ED4B579F15F888650642DF7FED519
        2792D6665B13D76D03152F7EFFA9BDD2E71E79DA7F4BAB0430F0BF0B223AF7C3
        44EBBCEF3CC6FA9BB0180CC1CC10A49064F507E4C32693E91A807B0D75C3F03F
        2A34106020C608F90949F348D926216FB705456F5890B97D15E2D7C4222A8ACF
        1C7D6DCBE9E6AA473D9F95597F7E3B3FA9A3F4F1552F0388351980243C5F3D9B
        D37E2853BBDC51C59E461B3BECE96CA9C9E0FEA2F591B32DD5A129F7370CF987
        19BCFE113D27B3E9D8B37EEE95CDF12F013083E45DDF79DEEABA32D8CB59573D
        AF9DA963AB3D8F27ECE98B57CE1CA4BB7B2F9D1FDAD87D388DA70F3DCC5315C9
        AC4A4FEC07200AAF1E3C8EB2B73E818A1567DD7D5F63F64F1D53BE194CFA6E62
        CBC6F8D8C4643F3665C9D851918C92BDEB905DB01A298FAD44DC3DA20D4054A8
        DFB713D168A4CCBFA1F488FB06E1ECB944D7C86D4E6A9BD1747E8576A16B9C13
        8322BE73CCA0EB8B599CFB56C20FBD615D268B0044B1ABEEE3EAC6A64EFEEA9D
        E26E7B2B2F7B46E91A76D3B6F303369F3CC5EA82342EAB665429E2BEDCB58300
        4C86101256DE5753FAC253888931231C9660369B60160448920C511411F7D026
        2CAF36313E038D426F7171310B0B0B211803A924D173611409F1B138DE76119F
        B6F6232E46435FFF8FE0030FE28F4BC3F8BEE72207466E71E16A00414BC67B91
        48A4ACBCBC1C78DDFE39BDBFDD60C5BBAD6C6CF3F0FDE64156D677734FF5013A
        1C0E7ABD5E1E4DDD488FC7C3A1A121DA37ACA1D3E9646D6D2DF3F3F3ABC58540
        D075A0B1657B302C62F4EA75A89A06C877509865811111C64D50540D822018F5
        CC508C73E3FFC066B3616E6EAE0624970B83989C9C9C171B1A1A383D3D4DE79B
        156CCFCDE2974FA4D391B995C752AD3CB6238F9D9D9D2C2929A1887F13D5346D
        5E966544A3510426C6B0D59A82F05F21DC0904B0100C616C7C0C165D87AAAA30
        91C4728C783088371EA80F40D693BE51289A82254941585110920D1515938FA4
        C1C0F53769EADEE8FC10B1660000000049454E44AE426082}
      TabOrder = 3
      OnClick = BtnUtilizatoriClick
    end
  end
  object pnTop: TDegradePanel
    Left = 0
    Top = 0
    Width = 936
    Height = 41
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Intretinere Organigrama Functii'
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
    Indent = 10
    StartColor = clBlack
    DegradeType = dtHorizontal
  end
  object InspFunctii: TcxDBVerticalGrid
    Left = 735
    Top = 73
    Width = 201
    Height = 510
    Align = alRight
    LookAndFeel.Kind = lfUltraFlat
    OptionsView.CellTextMaxLineCount = 3
    OptionsView.AutoScaleBands = False
    OptionsView.GridLineColor = clBtnShadow
    OptionsView.RowHeaderMinWidth = 30
    OptionsView.RowHeaderWidth = 98
    OptionsView.RowHeight = 20
    OptionsView.ValueWidth = 102
    OptionsBehavior.GoToNextCellOnEnter = True
    OptionsBehavior.RowSizing = True
    Navigator.Buttons.CustomButtons = <>
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 4
    DataController.DataSource = frmData.DTFunctiuni
    Version = 1
    object InspFunctiiID_FUNCTIUNI: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Id Functie'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = True
      Properties.DataBinding.FieldName = 'ID_FUNCTIUNI'
      Properties.Options.Editing = False
      ID = 0
      ParentID = -1
      Index = 0
      Version = 1
    end
    object InspFunctiiID_PARINTE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Id Parinte'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'ID_PARINTE'
      ID = 1
      ParentID = -1
      Index = 1
      Version = 1
    end
    object InspFunctiiID_UTILIZATORI: TcxDBEditorRow
      Expanded = False
      Height = 17
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
    object InspFunctiiID_INITIAL: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Id Initial'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'ID_INITIAL'
      ID = 3
      ParentID = -1
      Index = 3
      Version = 1
    end
    object InspFunctiiCOD_FUNCTIE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Cod Functie'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'COD_FUNCTIE'
      ID = 4
      ParentID = -1
      Index = 4
      Version = 1
    end
    object InspFunctiiDENUMIRE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Denumire'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'DENUMIRE'
      ID = 5
      ParentID = -1
      Index = 5
      Version = 1
    end
    object InspFunctiiATRIBUTII: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Atributii'
      Properties.EditPropertiesClassName = 'TcxMemoProperties'
      Properties.EditProperties.Alignment = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'ATRIBUTII'
      ID = 6
      ParentID = -1
      Index = 6
      Version = 1
    end
    object InspFunctiiDATA_INTRARE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Data Intrare'
      Properties.EditPropertiesClassName = 'TcxDateEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.DateButtons = [btnClear, btnToday]
      Properties.EditProperties.DateOnError = deToday
      Properties.EditProperties.InputKind = ikRegExpr
      Properties.DataBinding.FieldName = 'DATA_INTRARE'
      ID = 7
      ParentID = -1
      Index = 7
      Version = 1
    end
    object InspFunctiiDATA_IESIRE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Data Incetare'
      Properties.EditPropertiesClassName = 'TcxDateEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.DateButtons = [btnClear, btnToday]
      Properties.EditProperties.DateOnError = deToday
      Properties.EditProperties.InputKind = ikRegExpr
      Properties.DataBinding.FieldName = 'DATA_IESIRE'
      ID = 8
      ParentID = -1
      Index = 8
      Version = 1
    end
    object InspFunctiiSHAPE_TYPE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Tip Forma'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_TYPE'
      ID = 9
      ParentID = -1
      Index = 9
      Version = 1
    end
    object InspFunctiiSHAPE_LEFT_TOP: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Latime'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_LEFT_TOP'
      ID = 10
      ParentID = -1
      Index = 10
      Version = 1
    end
    object InspFunctiiSHAPE_RIGHT_BOTT: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Inaltime'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_RIGHT_BOTT'
      ID = 11
      ParentID = -1
      Index = 11
      Version = 1
    end
    object InspFunctiiSHAPE_COLOR: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Culoare'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_COLOR'
      ID = 12
      ParentID = -1
      Index = 12
      Version = 1
    end
    object InspFunctiiSHAPE_FONT_COL: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Culoare Font'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_FONT_COL'
      ID = 13
      ParentID = -1
      Index = 13
      Version = 1
    end
    object InspFunctiiSHAPE_FONT_NAME: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Nume Font'
      Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'SHAPE_FONT_NAME'
      ID = 14
      ParentID = -1
      Index = 14
      Version = 1
    end
    object InspFunctiiCategoryRow1: TcxCategoryRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Atribute Functie'
      ID = 15
      ParentID = -1
      Index = 15
      Version = 1
    end
    object InspFunctiiCategoryRow2: TcxCategoryRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Descriere Functie'
      ID = 16
      ParentID = -1
      Index = 16
      Version = 1
    end
    object InspFunctiiCategoryRow3: TcxCategoryRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Existenta Functie'
      ID = 17
      ParentID = -1
      Index = 17
      Version = 1
    end
    object InspFunctiiSTARE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Stare'
      Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
      Properties.EditProperties.Alignment = taLeftJustify
      Properties.EditProperties.NullStyle = nssUnchecked
      Properties.EditProperties.ReadOnly = True
      Properties.EditProperties.ValueChecked = '1'
      Properties.EditProperties.ValueGrayed = ''
      Properties.EditProperties.ValueUnchecked = '0'
      Properties.DataBinding.FieldName = 'STARE'
      ID = 18
      ParentID = -1
      Index = 18
      Version = 1
    end
    object InspFunctiiCategoryRow4: TcxCategoryRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Descriere Vizuala'
      ID = 19
      ParentID = -1
      Index = 19
      Version = 1
    end
    object InspFunctiiPOS_ID: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Pozitie'
      Properties.EditPropertiesClassName = 'TcxSpinEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.AssignedValues.MaxValue = True
      Properties.EditProperties.AssignedValues.MinValue = True
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'POS_ID'
      ID = 20
      ParentID = -1
      Index = 20
      Version = 1
    end
    object InspFunctiiDESCRIERE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Descriere'
      Properties.EditPropertiesClassName = 'TcxMemoProperties'
      Properties.EditProperties.Alignment = taLeftJustify
      Properties.EditProperties.MaxLength = 0
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'DESCRIERE'
      ID = 21
      ParentID = -1
      Index = 21
      Version = 1
    end
    object InspFunctiiDBEditorRow1: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Utilizatori Asociati'
      Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      ID = 22
      ParentID = -1
      Index = 22
      Version = 1
    end
    object InspFunctiiID_DEPARTAMENTE: TcxDBEditorRow
      Expanded = False
      Height = 17
      Properties.Caption = 'Departament'
      Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
      Properties.EditProperties.Alignment.Horz = taLeftJustify
      Properties.EditProperties.DropDownRows = 7
      Properties.EditProperties.Items = <>
      Properties.EditProperties.ReadOnly = False
      Properties.DataBinding.FieldName = 'ID_DEPARTAMENTE'
      ID = 23
      ParentID = -1
      Index = 23
      Version = 1
    end
  end
  object ppTipDepartament: TPopupMenu
    Left = 212
    Top = 217
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
  object ImaginiFunctii: TcxImageList
    SourceDPI = 96
    FormatVersion = 1
    DesignInfo = 14155896
    ImageInfo = <
      item
        ImageClass = 'TdxPNGImage'
        Image.Data = {
          89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
          6100000014744558745469746C650048797065726C696E6B3B5765623B06FEA0
          CE0000036F49444154785E5DD26B4C5B651C06F0BA8966662ED9C4B828642A59
          28B30C033B2D816119D0C2904BE95837DA420FAC3046D34E2CD1B6A3B4AB16C6
          2A05A183192E8B73CDA099ECC25A06AD2DB7029B61BA281B2223EAD4E81713F4
          D336CAE37B0CABC637793E9D3CBFFFFF9CF3B252D44116394F914424290665DC
          72B793A71C594AAEF43EA668CFFD44A9CBC529EC5292E79B493652F2EB2C00E1
          FC534E38742E8AAA703B72F4FE55CBE014EC431E5C0C7C81A3DDA7916FB7E3ED
          FAB1B554FAD2C00E6E4D2C83FC1F88D843CA791F5C8776B813FAC19368BAE240
          BBBB1FA53D9FE105CB5EC49864109B669151717598005B4936840166ED74AD77
          35B34783E44F332171974133A2C33BAE01348C2D4376D683486306B87A3BC4EF
          7FB54689CE9A09B0290C702BDCCE746B3BB69F8A4354171B6F346742E5BA0CEB
          CC2FB04CAE40E55C40E91933E8561344DA1BC8ADF22E10605B18E01D1959DA55
          7F04D1DABD78B16137E80B4E34DEFC1D1F4EFF05C3E80A949FCC43D0548DC659
          05689B1162F5CC4302BCFC2FA01C7D1C5B5B01D147BD90F50EC2E07B8086C09F
          D0795670BCFF0F1CB6DD46AC390BB26BB990B71D8544330B02BC1A062885FBBE
          D07411D1C78A21EF1887F4633F646D41281C77A168FF0985C6DBD8AA4A07BB35
          0159660944CAD11001C27F83955832E0CA3F7107FCDA3E24C9D5E096282090D7
          21F5B006197417B2AA038897A8917880469A588DD402ED2A9B9B6D27402483B0
          38059D4A816A662DBBCA09ABE302FC5373F8F9D7DFE09BF812667B1F78F97530
          DA7AE0F105B1B4FC03864626A0B37686D8BC9C26026C21616DA68ACF5F12969E
          823F38074B473FA4B52D90A89B515CD30851E549E4951B9153A64756491D8EE9
          5B70653880847D87E609F00A036CDCCE16250AA4EF8598C9124D337ACFF7A3A8
          CA02E6CC2F2CE2CE37DF62BFC280B633DD4829A8C6DD85EF41ED2F7F44801816
          25BBC6205BDE2AAAB9E51DBF05C3E973EB1375109089FB0E1E27EFAE628AE0E5
          5542A9B5E2F3211F386907BE23C08E27F7F9594A28D5995AFA566F04A6B1FCE3
          03B87D933034758538694537755647E8B2C78FF97B8BA4ECC5BBE6D6D0EB09FC
          76E65A3F0136305BBC995E5CCFCBA5E7520AAB1F2609E55FC7F1721B091EB773
          8FD016CF3F782F29877EC449132FBEB69BDF413AD1244F33C07F91E74822D73F
          CE4B24CF933CC3E0EB859DCCDACC64A6CCF4FE06A358007EE055FFED00000000
          49454E44AE426082}
      end
      item
        ImageClass = 'TdxPNGImage'
        Image.Data = {
          89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
          6100000015744558745469746C65005265766965776572733B5465616D3BBD83
          5A2F0000035849444154785E758E7D485D651CC7BFE739E778EED1DDAB2EAFCC
          EBD4BDCFD9AE6E5A46F6B28C8D08742BC20A21EACFA00869ACF65749442C8825
          112D28FF88D85634022385569131C5F5A2735B43575EBDD75D77AFD7EBF5BE1C
          CFDBF33CE7A92E4145ED073FF8F2FDC087AF22844057571774AE836A54F23C0F
          7F7684100C0D0D895BB15C2E27A6A7A7A10000E79C1CAE8D3CEDF7C9C708C156
          D365B135C3FBA0BBBB7B80732EFE8F8D78F503007851A0A99AEABAEC5D2558AD
          D735842433BBBA7B79F2FA5BDB903A1B536B566FC5EE397EFC6651203C5192B7
          98DADC72BFB4149B855CBA03A1FD75FCB1A75EEAD17CBA3BF9C9DBDA9670B314
          FDF93C12D13432059B2DC95BD847274E407965F04110B55C6815A14B81CAF2F6
          70DB1398B9380E2D73C55D1CEC7DCD36AD32836E60E98A0DF281233DB8347C06
          D1A5C204937D14008A0B645966522078766561AE7D73E836447FF80ADBDB3AF4
          6C26AB57EF0A418A46E4EF4F7F88F0DED7B19248C126A5E7003845C13B47A7A4
          AAAA2ACF34367DED928CB52EFC7A72A50079FE063C104C8C8DA0C25F82759B63
          7C620697D7D6AC8B39329A4CCC700092224992EF91E743BD7EBF76F4F63B3AF5
          A07F377EBD3A06A1F8F1D0B32FA063FE479C3BF5313686F762D3A17D686E657A
          E6C2B79FA697D581E141E98CF2DC9B2DC30D9B1B3AB737EC42C69A879B7750FB
          F821E0BB6908BB00C22C108963DBE10EC4739348AF47106E6E6C9CB93EF7FEC1
          67CC5E85BAF440656505666F4CC1760C08B18860492BB2561EB363A33012BF21
          257194C3C5427C0AF97C019645A16A049439F72AC944EED49723DFDCA740F5D5
          D5ECA9DD18A8294BB959F766B020F32B1764C7B611AF27AC2C9E66AABAC7E7AE
          46EDF9856B71C32CD8D4F6C6952FDE5B3A06401542C8771F29346CDD5FF672B9
          DFF7E80307DB645926481931046C4BF969FCBC5859363F5BBC669F8C4DB10821
          84027095BEBE3E0B80D5DFDFAFECBCB3FE746BB8A9A9AE36082A1C2CE7227024
          03D03CB47756ABC918EB318CB97D5747F32D773D19B01E6E7C110AFE3E625A6E
          93432DFC129D86E916A095029EE7219F73415D0EC364304DBA13806C1B1400FE
          25F0D692EBAF8E0C4FB633C6EB29E53AE37C07733CB8B68830C66D6AF338A5E2
          3200E63AFC3F02FEF9C9F81B00E4BF5E0240FEC105000F0007408B09C0EFC0AD
          C83118FFB0A70000000049454E44AE426082}
      end
      item
        ImageClass = 'TdxPNGImage'
        Image.Data = {
          89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF
          6100000023744558745469746C65004564697452616E67655065726D69737369
          6F6E3B53656375726974793B7B1AED18000002BC49444154785E7D936D481559
          1CC69FB9CEF5ADFDB21242452DF6C252081A15921A15525C5358F083517E504A
          8C34CA97726BE152A1469A0A05995C83BD2C1141118A5251525CBD698295697D
          C92C0577352B6FABDE3B7367E69C7F67864154C2071E9EF9F2FFFD9F7386233D
          A8FA0323A353070DEEF8676C7CDAF9F9DB74FFBB8950DAE0C4AC0E2122C2B2BA
          7BCE85EADCA4AFAF3C0574253F950A53D76979DB56D502700ACB66DA8E148EB2
          BF1D26D8B41C54340466D468FFC047B872B7436EE5CE976311957F79DB2BE190
          41C4C139600D7042381CF65F2B49CB00A0C1965499BDA5A3EBEF220ABEAF274F
          D91E6A3A5F46AAAAD2CF54DADC470062E71B40488E8CF10C0F7DCA8A961504E7
          082959FB31A73268CC0081A0330211AC016ED6011C3B734EA3F77E3D1CEB73E2
          631E6F99CEE89C9D0879DBDFA25BFF8E6781E7088682D0058013ACEA9CB83081
          310B2019222DD2CAE45FAA0FA4A79DDC5BB42F76F5B164C4B9E2F1F4BF56B4F8
          1AA12A616BE8EBF719D435DDC1C1E22A0C0FF660C38EEC35DC06C80EA794F7E8
          750F38B3EA091398C8A7331D38B2AB1C4E168DEBDE362424AC85E7E821DC6BEB
          C4D4BF63371963E9569585BF4638E658C393C99AFCADF85F61703AA3C0988182
          B2CBB870B618672F5E85DB4C7783F6B6D31307400511CDDB049C68EA25539301
          853E8C7FA1811B8769A82689062E2452BF7B33F5FDF93BF5546CA487C77F7B01
          60858CC572D867B38EA28575447C7B87C492DB20A600862A1C02E941F8EA4EA5
          00885C0A00E364A508EBC60D9D0186027A7309B75ABA91579002E20698C12024
          2D05B07068CE57D8D8B59B138732378B0ADD30B70B1B18F9A25AC3E04C80394C
          2D0568DE73AE4CB39A7DA9BF969ED934023D04620CEEF25440004800B8000BD1
          2200119958C5362449D202B3BADF575B94CE7406A673EB585CC06614E607A0C9
          585E6A66F3A8CB6E242DDC653F26F507A124B4B097E1BD620000000049454E44
          AE426082}
      end>
  end
  object DataSource1: TDataSource
    Left = 360
    Top = 136
  end
end
