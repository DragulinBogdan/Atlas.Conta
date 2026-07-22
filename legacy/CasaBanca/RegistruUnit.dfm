object FrmRegistru: TFrmRegistru
  Left = 396
  Top = 253
  ActiveControl = GridRegistru
  Caption = 'Registru Casa / Banca'
  ClientHeight = 643
  ClientWidth = 1046
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  ShowHint = True
  WindowState = wsMaximized
  OnActivate = FormActivate
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object Splitter2: TSplitter
    Left = 0
    Top = 600
    Width = 1046
    Height = 1
    Cursor = crVSplit
    Align = alBottom
    ExplicitTop = 527
    ExplicitWidth = 938
  end
  object pnRest: TPanel
    Left = 0
    Top = 0
    Width = 1046
    Height = 509
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 0
    object GridRegistru: TdxDBTreeList
      Left = 0
      Top = 65
      Width = 1046
      Height = 444
      SearchType = stStart
      Bands = <
        item
          Caption = 'Stare'
          DisableCustomizing = True
          DisableDragging = True
          Fixed = bfLeft
          Width = 68
        end
        item
          Caption = 'Valori'
          Width = 323
        end
        item
          Caption = 'Cont'
          MinWidth = 10
          Visible = False
          Width = 78
        end
        item
          Caption = 'Proiect'
          DisableCustomizing = True
          DisableDragging = True
          Visible = False
          Width = 154
        end
        item
          Caption = 'HiddenProj'
          Visible = False
          Width = 41
        end
        item
          Caption = 'Facturi'
          Visible = False
          Width = 228
        end>
      DefaultLayout = False
      HeaderPanelRowCount = 1
      KeyField = 'ID_LISTA'
      ParentField = 'ID_PARINTE'
      Align = alClient
      PopupMenu = GridRegistruPopup
      TabOrder = 0
      OnKeyDown = GridRegistruKeyDown
      OnKeyPress = GridRegistruKeyPress
      OnMouseMove = GridRegistruMouseMove
      BandColor = clWindow
      DataSource = DTRegistru
      FixedBandLineWidth = 1
      HeaderColor = clWindow
      HighlightColor = clRed
      HighlightTextColor = clNavy
      LookAndFeel = lfUltraFlat
      OptionsBehavior = [etoAnsiSort, etoAutoCopySelectedToClipboard, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoEnterThrough, etoImmediateEditor, etoMultiSelect, etoTabs, etoTabThrough, etoVertThrough]
      OptionsDB = [etoCancelOnExit, etoCanInsert, etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
      OptionsView = [etoAutoWidth, etoHotTrack, etoIndicator, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
      PaintStyle = psOutlook
      PreviewFieldName = 'MEXPLIC'
      ShowBands = True
      ShowGrid = True
      SimpleCustomizeBox = True
      TreeLineColor = clGrayText
      Filter.Active = True
      OnChangeColumn = GridRegistruChangeColumn
      OnChangeNode = GridRegistruChangeNode
      OnCollapsing = GridRegistruCollapsing
      OnColumnSorting = GridRegistruColumnSorting
      OnCustomDrawCell = GridRegistruCustomDrawCell
      OnDeletion = GridRegistruDeletion
      OnGetLevelColor = GridRegistruGetLevelColor
      OnSelectedCountChange = GridRegistruSelectedCountChange
      object GridRegistruPOZ: TdxDBTreeListMaskColumn
        Caption = 'Poz'
        Width = 52
        BandIndex = 0
        RowIndex = 0
        FieldName = 'POZ'
        SummaryFooterType = cstCount
      end
      object GridRegistruDATA: TdxDBTreeListDateColumn
        Caption = 'Data Tranzactie'
        Width = 43
        BandIndex = 1
        RowIndex = 0
        FieldName = 'DATA'
        DateValidation = True
        UseEditMask = True
        OnDateValidateInput = GridRegistruDATADateValidateInput
      end
      object GridRegistruCURS_SCHIMB: TdxDBTreeListButtonColumn
        Caption = 'Curs Schimb'
        Width = 42
        BandIndex = 1
        RowIndex = 0
        FieldName = 'CURS_SCHIMB'
        Buttons = <
          item
            Default = True
          end
          item
            Default = False
            Glyph.Data = {
              36030000424D3603000000000000360000002800000010000000100000000100
              18000000000000030000120B0000120B00000000000000000000FFFFFFFFFFFF
              FFFFFFFBFBFBB3B6B8C2C4C5F6F6F69DA3A9B9BCBEFFFFFFFFFFFFFFFFFFFFFF
              FFFFFFFFEDECEDDBDADAFFFFFFFFFFFFFFFFFFC7C9CA4A92C55088B199A0A550
              9DD3538AB2D7D8D8778A988D98A1FFFFFFF3F3F392737C8C5060FFFFFFFFFFFF
              FFFFFF9FA8AD9CDAFC76B6DE5D839599D7F973B1D875879162B2E6559CCDB7B9
              BB8D6C74A24257967F85FFFFFFDFDFDF7D887B6B8C8BACDCF788BFDC6A99A3B3
              E3FA80B5D1647980A3B1BF7999B17C6A76A3445A7F6974FBFBFBFFFFFFACAFAF
              4E746E578180A7D7F18BBCCE6796979DCEEDA9B8C5CFC5B8E7D6C5D7C9BCAD92
              8F888CA5899EAAFFFFFFA2A9AE5F92B498CCED6E9EB2789D8E81A89076A58C9B
              B4B8DBD6CEDCD8C4E7E4DEE2DFD3D8D0C0A3B6C675A5B2C5C8C78EA7B5CCFFFF
              C8F3FF89B7BF729B76769E7A719B798B9277DCCCAFDDDBC6DFE9E2DDE5DBE2E3
              D6A4A89D8DD9D697ADAAD5D7D8B3BBC09BA6A87D988270937673997C729B7D7C
              8C73D1BFA7DEDCC8DBE0D3DDE7DDE0E4DD94A1908DDBD699ADAAFFFFFFA5A9A6
              6A826F6F957879A1827BA4837AA382759E7D989A83D3C8B3DCD6C3D9D8CDAFB3
              A76E8D7A86CECD97A9A7FFFFFF86988D73997B749A7C78A080789F80779D7F77
              9E80739A7B7F9277929A828A9B8275977977A69188CECF9FA8A4FFFFFF9BA59E
              6F9376709578789F8080AC8886B58D87B58E82B08A7AA88474A07F749E7E749B
              7B78A28A94E3E4B2B8B6FFFFFFA4ABA7769A7E83A9888AB68F8BBD9092C59894
              C79999C99B9BC8A090C09788B48E80A684779D8196E8E8A3AEACFFFFFFE5E7E6
              B4C1B9A6B9AAAEC7B1A7CAAA8DBE91AAD2AFBFE4D5C0EAE7B3D6BC98C599A7D1
              B79AD5CA98E6E499A8A3FFFFFFFFFFFFFFFFFFFFFFFFE9EBEABDC6C096AE9B97
              B89CA9D0BAA9D3BB9ACEB491D8CC90D8D38DBCB2B0BEB6EEEEEEFFFFFFFFFFFF
              FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEDEFEEBFC9C39DBDBA96CECD9FC0B9C5CE
              C9F5F6F6FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
              FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF}
          end>
        OnButtonClick = GridRegistruCURS_SCHIMBButtonClick
      end
      object GridRegistruINCASARI: TdxDBTreeListCurrencyColumn
        Caption = 'Incasari'
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'INCASARI'
        Nullable = False
      end
      object GridRegistruECL: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Echilibrata'
        DisableEditor = True
        MinWidth = 16
        TabStop = False
        Width = 48
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ECL'
        Descriptions.Strings = (
          'Dezechilibrata'
          'Echilibrata'
          'Validata')
        Images = ImaginiEcl
        ImageIndexes.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4'
          '5')
        Values.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4'
          '5')
      end
      object GridRegistruPLATI: TdxDBTreeListCurrencyColumn
        Caption = 'Plati'
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'PLATI'
        Nullable = False
      end
      object GridRegistruSOLD: TdxDBTreeListMaskColumn
        Tag = 1
        Caption = 'Sold'
        DisableEditor = True
        TabStop = False
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'SOLD'
      end
      object GridRegistruCONT_CSP: TdxDBTreeListPopupColumn
        Caption = 'Cont Coresp.'
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'CONT_CSP'
        PopupAutoSize = False
        PopupControl = frmCasaContainer.TreePlan
        PopupFormBorderStyle = pbsSysPanel
        PopupMinWidth = 800
        PopupWidth = 800
        OnCloseUp = GridRegistruCONT_CSPCloseUp
        OnPopup = GridRegistruCONT_CSPPopup
      end
      object GridRegistruCODGEST: TdxDBTreeListPopupColumn
        Caption = 'Partener'
        Width = 33
        BandIndex = 1
        RowIndex = 0
        OnValidate = GridRegistruCODGESTValidate
        FieldName = 'CODGEST'
        OnGetText = GridRegistruCODGESTGetText
        PopupControl = frmCasaContainer.TreeRepartitori
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruCODGESTCloseUp
        OnPopup = GridRegistruCODGESTPopup
      end
      object GridRegistruCOD_CB: TdxDBTreeListMaskColumn
        Visible = False
        Width = 152
        BandIndex = 1
        RowIndex = 0
        FieldName = 'COD_CB'
      end
      object GridRegistruEXPLICATIE: TdxDBTreeListMaskColumn
        Caption = 'Explicatie'
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'EXPLICATIE'
      end
      object GridRegistruTIPDOC: TdxDBTreeListPopupColumn
        Caption = 'TipDoc'
        MinWidth = 5
        Width = 8
        BandIndex = 1
        RowIndex = 0
        OnValidate = GridRegistruTIPDOCValidate
        FieldName = 'TIPDOC'
        PopupControl = frmCasaContainer.TreeTipDoc
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruTIPDOCCloseUp
        OnPopup = GridRegistruTIPDOCPopup
      end
      object GridRegistruNRDOC: TdxDBTreeListMaskColumn
        Caption = 'Nr Doc'
        Sorted = csUp
        Width = 32
        BandIndex = 1
        RowIndex = 0
        FieldName = 'NRDOC'
      end
      object GridRegistruACHITAT: TdxDBTreeListCurrencyColumn
        Tag = 1
        Caption = 'Achitat'
        DisableEditor = True
        Width = 91
        BandIndex = 2
        RowIndex = 0
        FieldName = 'ACHITAT'
        Nullable = False
      end
      object GridRegistruID_PARINTE: TdxDBTreeListMaskColumn
        Visible = False
        Width = 695
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_PARINTE'
      end
      object GridRegistruSORTFIELD: TdxDBTreeListColumn
        Caption = 'SORT'
        DisableEditor = True
        TabStop = False
        Visible = False
        Width = 491
        BandIndex = 0
        RowIndex = 0
        FieldName = 'SORTFIELD'
      end
      object GridRegistruID_LISTA: TdxDBTreeListMaskColumn
        TabStop = False
        Visible = False
        Width = 637
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_LISTA'
      end
      object GridRegistruRecId: TdxDBTreeListColumn
        TabStop = False
        Visible = False
        Width = 356
        BandIndex = 0
        RowIndex = 0
        FieldName = 'RecId'
      end
      object GridRegistruON_SERVER: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Server'
        DisableEditor = True
        MinWidth = 16
        TabStop = False
        Visible = False
        Width = 556
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ON_SERVER'
        Descriptions.Strings = (
          'Local'
          'Server')
        ImageIndexes.Strings = (
          '0'
          '1')
        ShowDescription = True
        Values.Strings = (
          '0'
          '1')
      end
      object GridRegistruVALIDATA: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Validare'
        DisableEditor = True
        MinWidth = 10
        TabStop = False
        Visible = False
        Width = 102
        BandIndex = 0
        RowIndex = 0
        FieldName = 'VALIDATA'
        Descriptions.Strings = (
          'Validare Stearsa'
          'Validata'
          'Nevalidata'
          'Validata de alt operator'
          'Validata de Administrator')
        Images = ValidariList
        ImageIndexes.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4'
          '5'
          '6')
        Values.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4')
      end
      object GridRegistruCOD: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        TabStop = False
        Visible = False
        Width = 126
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD'
      end
      object GridRegistruCOD_CBT: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        TabStop = False
        Visible = False
        Width = 126
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_CBT'
      end
      object GridRegistruCOD_CasaTransfer: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taRightJustify
        Caption = 'Destinatie'
        DisableEditor = True
        MinWidth = 16
        TabStop = False
        Visible = False
        Width = 27
        BandIndex = 1
        RowIndex = 0
        FieldName = 'COD_CBT'
        ShowDescription = True
      end
      object GridRegistruTRANSFER: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Transfer'
        DisableEditor = True
        MinWidth = 16
        TabStop = False
        Visible = False
        Width = 79
        BandIndex = 0
        RowIndex = 0
        FieldName = 'TRANSFER'
        Descriptions.Strings = (
          'Normal'
          'Primit din alta Casa - Acceptat'
          'Primit din alta Casa - Neacceptat'
          'Iesire catre alta Casa - Intializare operatie - Local'
          'Iesire catre alta Casa - Confirmare Operatie pe Server'
          'Intrare din Banca - Acceptat'
          'Intrare din Banca - Neacceptat'
          'Iesire catre Banca - Intializare operatie - Local'
          'Iesire catre Banca - Confirmare Operatie pe Server'
          'Reject - Initalizare Operatie - Local'
          'Reject - Confirmare Operatie pe Server'
          'Anunt de confirmare Reject Acceptat pe Server'
          'Intrare alta Casa - Intializare operatie - Local'
          'Intrare alta Banca - Intializare operatie - Local')
        ImageIndexes.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4'
          '5'
          '6'
          '7'
          '8'
          '9'
          '10'
          '11'
          '12'
          '13')
        ShowDescription = True
        Values.Strings = (
          '0'
          '1'
          '2'
          '3'
          '4'
          '5'
          '6'
          '7'
          '8'
          '9'
          '10'
          '11'
          '12'
          '13')
      end
      object GridRegistruPROJ: TdxDBTreeListPopupColumn
        Caption = 'Cap.Subcap.'
        MaxLength = 11
        Width = 60
        BandIndex = 4
        RowIndex = 0
        OnValidate = GridRegistruPROJValidate
        FieldName = 'COD_FUNCTIONAL'
        OnGetText = GridRegistruPROJGetText
        PopupControl = frmCasaContainer.TreeFunctional
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruPROJCloseUp
        OnPopup = GridRegistruPROJPopup
      end
      object GridRegistruTIP_CHELTVEN: TdxDBTreeListPopupColumn
        Alignment = taLeftJustify
        Caption = 'Art.titl.alin.'
        MaxLength = 11
        Width = 71
        BandIndex = 4
        RowIndex = 0
        OnValidate = GridRegistruTIP_CHELTVENValidate
        FieldName = 'COD_ECONOMIC'
        OnGetText = GridRegistruTIP_CHELTVENGetText
        PopupControl = frmCasaContainer.TreeEconomic
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruPROJCloseUp
        OnPopup = GridRegistruPROJPopup
      end
      object GridRegistruORGANIGRAMA: TdxDBTreeListPopupColumn
        Alignment = taLeftJustify
        Caption = 'Organigrama'
        Width = 39
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_ORGANIGRAMA'
        OnGetText = GridRegistruORGANIGRAMAGetText
        PopupControl = frmCasaContainer.TreeOrganigrama
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruPROJCloseUp
        OnPopup = GridRegistruPROJPopup
      end
      object GridRegistruRESURSA: TdxDBTreeListPopupColumn
        Alignment = taLeftJustify
        Caption = 'Resurse'
        Width = 35
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_RESURSA'
        OnGetText = GridRegistruRESURSAGetText
        PopupControl = frmCasaContainer.TreeCheltituitori
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = GridRegistruPROJCloseUp
        OnPopup = GridRegistruPROJPopup
      end
      object GridRegistruID_PROIECT: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        Visible = False
        Width = 20
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_PROIECT'
      end
      object GridRegistruID_TIPURI_CHELTVEN: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        Visible = False
        Width = 20
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_TIPURI_CHELTVEN'
      end
      object GridRegistruID_ORGANIGRAMA: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        Visible = False
        Width = 20
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_ORGANIGRAMA'
      end
      object GridRegistruID_RESURSA: TdxDBTreeListMaskColumn
        Tag = 1
        DisableEditor = True
        Visible = False
        Width = 20
        BandIndex = 4
        RowIndex = 0
        FieldName = 'ID_RESURSA'
      end
      object GridRegistruNR_DECONT: TdxDBTreeListMaskColumn
        Tag = 1
        Caption = 'Nr Decont'
        DisableEditor = True
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NR_DECONT'
      end
      object GridRegistruDATA_DECONT: TdxDBTreeListDateColumn
        Tag = 1
        Caption = 'Data Decont'
        DisableEditor = True
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_DECONT'
      end
      object GridRegistruC_O: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Cod Operator'
        DisableEditor = True
        MinWidth = 16
        Visible = False
        Width = 100
        BandIndex = 0
        RowIndex = 0
        FieldName = 'C_O'
        ShowDescription = True
      end
      object GridRegistruV_O: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Validator'
        DisableEditor = True
        MinWidth = 16
        Visible = False
        Width = 100
        BandIndex = 0
        RowIndex = 0
        FieldName = 'V_O'
        ShowDescription = True
      end
      object GridRegistruV_O_1: TdxDBTreeListImageColumn
        Tag = 1
        Alignment = taLeftJustify
        Caption = 'Verificator'
        DisableEditor = True
        MinWidth = 16
        Visible = False
        Width = 100
        BandIndex = 0
        RowIndex = 0
        FieldName = 'V_O_1'
        ShowDescription = True
      end
      object GridRegistruCOD_TRANSFER: TdxDBTreeListMaskColumn
        Tag = 1
        Caption = 'Cod Transfer'
        DisableEditor = True
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_TRANSFER'
      end
      object GridRegistruNR_EXTRAS: TdxDBTreeListMaskColumn
        Caption = 'Nr Extras'
        Visible = False
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'NR_EXTRAS'
      end
      object GridRegistruDATA_EXTRAS: TdxDBTreeListDateColumn
        Caption = 'Data Extras'
        Visible = False
        Width = 33
        BandIndex = 1
        RowIndex = 0
        FieldName = 'DATA_EXTRAS'
      end
      object GridRegistruORD: TdxDBTreeListPopupColumn
        Caption = 'Ord'
        Width = 32
        BandIndex = 5
        RowIndex = 0
        PopupControl = frmCasaContainer.TreeListOrd
        OnCloseUp = GridRegistruORDCloseUp
      end
      object GridRegistruORDONANTARE: TdxDBTreeListPopupColumn
        Caption = 'Sel fct'
        Width = 34
        BandIndex = 5
        RowIndex = 0
        PopupControl = frmCasaContainer.TreeOrdonantari
        OnCloseUp = GridRegistruORDONANTARECloseUp
      end
      object GridRegistruID_REPARTITOR: TdxDBTreeListPopupColumn
        Caption = 'Repartitor'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        OnValidate = GridRegistruCODGESTValidate
        FieldName = 'ID_REPARTITOR'
        OnGetText = GridRegistruCODGESTGetText
        PopupControl = frmCasaContainer.TreeRepartitori
        OnCloseUp = GridRegistruCODGESTCloseUp
        OnPopup = GridRegistruCODGESTPopup
      end
      object GridRegistruCONT_CSP1: TdxDBTreeListColumn
        Caption = 'Cont'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        FieldName = 'CONT_CSP'
      end
      object GridRegistruID_GEST_DOCUM: TdxDBTreeListColumn
        Caption = 'Fct'
        Visible = False
        Width = 28
        BandIndex = 5
        RowIndex = 0
        FieldName = 'ID_GEST_DOCUM'
      end
      object GridRegistruTIP_DOC: TdxDBTreeListColumn
        Caption = 'TipDoc'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        FieldName = 'TIP_DOC'
      end
      object GridRegistruNR_DOCUM: TdxDBTreeListColumn
        Caption = 'Nr'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        FieldName = 'NR_DOCUM'
      end
      object GridRegistruDATA_DOCUM: TdxDBTreeListColumn
        Caption = 'Data'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        FieldName = 'DATA_DOCUM'
      end
      object GridRegistruTOTALDOC: TdxDBTreeListColumn
        Caption = 'Total'
        Width = 26
        BandIndex = 5
        RowIndex = 0
        FieldName = 'TOTALDOC'
      end
      object GridRegistruDETALIIBuget: TdxDBTreeListPopupColumn
        Caption = 'Clasificatie buget'
        BandIndex = 3
        RowIndex = 0
        FieldName = 'DETALII_BUGET'
        PopupFormBorderStyle = pbsSysPanel
        PopupHeight = 400
        PopupMinHeight = 400
        PopupMinWidth = 400
        PopupWidth = 400
        OnCloseUp = GridRegistruDETALIIBugetCloseUp
        OnInitPopup = GridRegistruDETALIIBugetInitPopup
      end
      object GridRegistruCOD_FUNCTIONAL: TdxDBTreeListColumn
        Caption = 'Cod functional'
        Visible = False
        Width = 26
        BandIndex = 3
        RowIndex = 0
        FieldName = 'COD_FUNCTIONAL'
      end
      object GridRegistruID_OI_UNITATI: TdxDBTreeListMaskColumn
        Caption = 'IdUnitate'
        Visible = False
        BandIndex = 3
        RowIndex = 0
        FieldName = 'ID_OI_UNITATI'
      end
      object GridRegistruCOD_ECONOMIC: TdxDBTreeListColumn
        Caption = 'Cod economic'
        Visible = False
        Width = 26
        BandIndex = 3
        RowIndex = 0
        FieldName = 'COD_ECONOMIC'
      end
      object GridRegistruID_OI_PROIECTE: TdxDBTreeListMaskColumn
        Caption = 'IdProiect'
        Visible = False
        BandIndex = 3
        RowIndex = 0
        FieldName = 'ID_OI_PROIECTE'
      end
      object GridRegistruID_ANGAJAMENTE_DEFALCARE: TdxDBTreeListMaskColumn
        Caption = 'IdAng'
        Visible = False
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_ANGAJAMENTE_DEFALCARE'
      end
      object GridRegistruID_ORDONANTARE_DEFALCARE: TdxDBTreeListMaskColumn
        Caption = 'IdOrd'
        Visible = False
        BandIndex = 3
        RowIndex = 0
        FieldName = 'ID_ORDONANTARE_DEFALCARE'
      end
    end
    object pnTop: TPanel
      Left = 0
      Top = 0
      Width = 1046
      Height = 65
      Align = alTop
      TabOrder = 1
      object gbInformation: TPanel
        Left = 1
        Top = 1
        Width = 1044
        Height = 41
        Align = alTop
        BevelInner = bvRaised
        BevelOuter = bvLowered
        Color = 16776176
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        TabOrder = 0
        OnDblClick = gbInformationDblClick
        DesignSize = (
          1044
          41)
        object Label1: TLabel
          Left = 473
          Top = 3
          Width = 93
          Height = 13
          Anchors = [akTop, akRight]
          Caption = 'Data de Inceput'
          ExplicitLeft = 365
        end
        object btnMemo: TSpeedButton
          Left = 967
          Top = 5
          Width = 14
          Height = 15
          Hint = 'Afiseaza banda de Explicatii Suplimentare'
          Action = Cmd_ShowDetail
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'D'
          Flat = True
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          Margin = 1
          ParentFont = False
          ParentShowHint = False
          ShowHint = True
          ExplicitLeft = 859
        end
        object btnRecalcSold: TSpeedButton
          Left = 949
          Top = 5
          Width = 14
          Height = 15
          Action = Cmd_RecalculateSold
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'R'
          Flat = True
          Margin = 1
          ParentShowHint = False
          ShowHint = True
          ExplicitLeft = 841
        end
        object btnSaveLocal: TSpeedButton
          Left = 930
          Top = 5
          Width = 14
          Height = 15
          Hint = 'Salveaza Local si Inchide Conexiunea'
          Action = Cmd_SaveLocal
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'S'
          Flat = True
          Margin = 0
          ParentShowHint = False
          ShowHint = True
          Spacing = 0
          ExplicitLeft = 822
        end
        object Label4: TLabel
          Left = 14
          Top = 1
          Width = 202
          Height = 13
          Caption = 'Informatii Casa/ Banca / Trezorerie'
          Transparent = True
        end
        object btnSummary: TSpeedButton
          Left = 982
          Top = 5
          Width = 14
          Height = 15
          Hint = 'Afiseaza banda de Sumar'
          Action = Cmd_ShowSummary
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = '$'
          Flat = True
          Margin = 1
          ParentShowHint = False
          ShowHint = True
          ExplicitLeft = 874
        end
        object btnLegenda: TSpeedButton
          Left = 998
          Top = 5
          Width = 14
          Height = 15
          Hint = 'Afiseaza Legenda pentru culori'
          Action = Cmd_ShowLegend
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'L'
          Flat = True
          Margin = 1
          ParentShowHint = False
          ShowHint = True
          ExplicitLeft = 890
        end
        object btnErrors: TSpeedButton
          Left = 1017
          Top = 5
          Width = 14
          Height = 15
          Hint = 'Afiseaza dialogul de cautare erori'
          Action = CmdErrors
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'E'
          Flat = True
          Margin = 1
          ParentShowHint = False
          ShowHint = True
          ExplicitLeft = 909
        end
        object ImgCasa: TImage
          Left = 4
          Top = 17
          Width = 19
          Height = 19
        end
        object btnPreferedHouse: TSpeedButton
          Left = 875
          Top = 7
          Width = 12
          Height = 15
          Hint = 'Seteaza Casa Preferata'
          AllowAllUp = True
          Anchors = [akTop, akRight]
          Caption = 'P'
          Flat = True
          Margin = 1
          ParentShowHint = False
          ShowHint = True
          OnClick = btnPreferedHouseClick
          ExplicitLeft = 767
        end
        object edCurentHouse: TdxPopupEdit
          Left = 25
          Top = 16
          Width = 408
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = []
          ParentFont = False
          Style.Edges = [edgLeft, edgTop, edgRight, edgBottom]
          TabOrder = 0
          Anchors = [akLeft, akTop, akRight]
          ReadOnly = False
          StyleController = StyleController
          HideEditCursor = True
          PopupAutoSize = False
          PopupControl = TreeStructura
          PopupFormBorderStyle = pbsSysPanel
          PopupMinWidth = 400
          PopupWidth = 400
          OnCloseUp = edCurentHouseCloseUp
          OnInitPopup = edCurentHouseInitPopup
          OnPopup = edCurentHousePopup
          StoredValues = 64
        end
        object edData: TdxDateEdit
          Left = 571
          Top = 1
          Width = 68
          TabOrder = 1
          Anchors = [akTop, akRight]
          StyleController = StyleController
          Date = 38040.000000000000000000
          DateOnError = deToday
          DateValidation = True
          UseEditMask = True
          OnDateChange = edDataDateChange
          OnDateValidateInput = edDataDateValidateInput
          StoredValues = 4
        end
        object chkExpand: TCheckBox
          Left = 967
          Top = 23
          Width = 59
          Height = 13
          Hint = 'Restrangere Expandare pe al doilea nivel'
          Anchors = [akTop, akRight, akBottom]
          Caption = 'Detalii'
          Checked = True
          ParentShowHint = False
          ShowHint = True
          State = cbChecked
          TabOrder = 2
          OnClick = chkExpandClick
        end
        object btnSwitchFilters: TSpinButton
          Left = 910
          Top = 5
          Width = 15
          Height = 30
          Anchors = [akTop, akRight, akBottom]
          Ctl3D = True
          DownGlyph.Data = {
            0E010000424D0E01000000000000360000002800000009000000060000000100
            200000000000D800000000000000000000000000000000000000008080000080
            8000008080000080800000808000008080000080800000808000008080000080
            8000008080000080800000808000000000000080800000808000008080000080
            8000008080000080800000808000000000000000000000000000008080000080
            8000008080000080800000808000000000000000000000000000000000000000
            0000008080000080800000808000000000000000000000000000000000000000
            0000000000000000000000808000008080000080800000808000008080000080
            800000808000008080000080800000808000}
          ParentCtl3D = False
          TabOrder = 3
          UpGlyph.Data = {
            0E010000424D0E01000000000000360000002800000009000000060000000100
            200000000000D800000000000000000000000000000000000000008080000080
            8000008080000080800000808000008080000080800000808000008080000080
            8000000000000000000000000000000000000000000000000000000000000080
            8000008080000080800000000000000000000000000000000000000000000080
            8000008080000080800000808000008080000000000000000000000000000080
            8000008080000080800000808000008080000080800000808000000000000080
            8000008080000080800000808000008080000080800000808000008080000080
            800000808000008080000080800000808000}
          OnDownClick = btnSwitchFiltersDownClick
          OnUpClick = btnSwitchFiltersUpClick
        end
        object gpTipDefalcare: TGroupBox
          Left = 664
          Top = 1
          Width = 244
          Height = 34
          Anchors = [akTop, akRight]
          Caption = 'Tip Defalcare'
          TabOrder = 4
          object rbCont: TRadioButton
            Tag = 1
            Left = 55
            Top = 11
            Width = 66
            Height = 21
            Caption = '&Conturi'
            TabOrder = 0
            OnClick = Cmd_SchimbaDefalcareExecute
          end
          object rbProiect: TRadioButton
            Tag = 2
            Left = 125
            Top = 14
            Width = 68
            Height = 15
            Caption = '&Proiecte'
            Checked = True
            TabOrder = 1
            TabStop = True
            OnClick = Cmd_SchimbaDefalcareExecute
          end
          object rbFara: TRadioButton
            Left = 5
            Top = 14
            Width = 52
            Height = 15
            Caption = '&Fara'
            TabOrder = 2
            OnClick = Cmd_SchimbaDefalcareExecute
          end
          object rbFact: TRadioButton
            Tag = 4
            Left = 199
            Top = 13
            Width = 42
            Height = 15
            Caption = 'Fc&t'
            TabOrder = 3
            Visible = False
            OnClick = Cmd_SchimbaDefalcareExecute
          end
        end
        object chkEfectiv: TCheckBox
          Left = 477
          Top = 21
          Width = 51
          Height = 13
          Hint = 'Se va face filtrarea pe zi'
          Anchors = [akTop, akRight]
          Caption = 'Pe zi'
          Checked = True
          State = cbChecked
          TabOrder = 5
          OnClick = chkEfectivClick
        end
        object edNrZile: TdxSpinEdit
          Left = 531
          Top = 17
          Width = 38
          Hint = 'Numarul de zile anterioare sau dupa Data de Inceput'
          TabOrder = 6
          OnKeyDown = edNrZileKeyDown
          Anchors = [akTop, akRight]
          StyleController = StyleController
          OnValidate = edNrZileValidate
        end
        object edListaData: TcxImageComboBox
          Left = 571
          Top = 20
          Anchors = [akTop, akRight]
          Enabled = False
          Properties.Items = <>
          Properties.OnChange = edListaDataPropertiesChange
          TabOrder = 7
          Width = 86
        end
      end
      object pnDecont: TPanel
        Left = 1
        Top = 43
        Width = 1044
        Height = 21
        Align = alBottom
        AutoSize = True
        BevelOuter = bvNone
        Color = 16776176
        TabOrder = 1
        DesignSize = (
          1044
          21)
        object lbNrDec: TLabel
          Left = 0
          Top = 3
          Width = 47
          Height = 13
          Caption = 'Nr/Data'
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
        end
        object btnFindDecont: TSpeedButton
          Left = 944
          Top = 1
          Width = 20
          Height = 20
          Hint = 'Cauta decont dupa detalii'
          Anchors = [akTop, akRight]
          Flat = True
          Glyph.Data = {
            06030000424D060300000000000036000000280000000F0000000F0000000100
            180000000000D002000000000000000000000000000000000000C0C0C0C0C0C0
            BCBCBCB4B4B48A8A8A8181827979886D6D887979888181828A8A8AB4B4B4BDBD
            BDC0C0C0C0C0C0434C41C0C0C0BCBCBCA7A7A79A9A9AD9D9D9DEDEF27C7CFF3F
            3FFF7C7CFFDEDEF2D8D8D89A9A9AA7A7A7BCBCBCC0C0C0434C41BDBDBDA7A7A7
            B6B6B6EFEFEFFFFFFFBCBCFF1717FF0000FF1717FFBDBDFFFFFFFFEFEFEFB6B6
            B6A7A7A7BDBDBD434C41B4B4B49A9A9AEFEFEFFFFFFFFFFFFFCDCDFF2525FF00
            00FF2525FFCDCDFFFFFFFFFFFFFFEFEFEF9A9A9AB4B4B4434C418A8A8AD8D8D8
            FFFFFFFFFFFFFFFFFFF9F9FFBDBDFF8585FFBDBDFFF9F9FFFFFFFFFFFFFFFFFF
            FFD8D8D88A8A8A434C41838383F2F2F2FFFFFFFFFFFFFFFFFFE7E7FF5757FF28
            28FF5454FFE5E5FFFFFFFFFFFFFFFFFFFFF2F2F2838383434C41888888FFFFFF
            FFFFFFFFFFFFFFFFFFECECFF5D5DFF0000FF1F1FFFC6C6FFFFFFFFFFFFFFFFFF
            FFFFFFFF888888434C41888888FFFFFFFFFFFFFFFFFFFFFFFFFCFCFFB2B2FF1C
            1CFF0000FF6363FFFFFFFFFFFFFFFFFFFFFFFFFF888888434C41888888FFFFFF
            FFFFFFFFFFFFFFFFFFFFFFFFEBEBFF6161FF0000FF1111FFD0D0FFFFFFFFFFFF
            FFFFFFFF888888434C41838383F2F2F2FFFFFFFFFFFF8383FF6060FF7D7DFF87
            87FF1E1EFF0101FF6767FFFFFFFFFFFFFFF2F2F2838383434C418A8A8AD8D8D8
            FFFFFFFFFFFF3838FF0000FF2525FF7070FF2525FF0000FF3838FFFFFFFFFFFF
            FFD8D8D88A8A8A434C41B4B4B49A9A9AEFEFEFFFFFFF5D5DFF0000FF0000FF00
            00FF0000FF0000FF5E5EFFFFFFFFEFEFEF9A9A9AB4B4B4434C41BDBDBDA7A7A7
            B6B6B6EFEFEFCCCCFF2828FF0707FF0000FF0707FF2828FFCCCCFFEFEFEFB5B5
            B5A7A7A7BDBDBD434C41C0C0C0BCBCBCA7A7A79A9A9AD4D4D9B1B1F28484FF75
            75FF8484FFB2B2F2D3D3D89A9A9AA7A7A7BCBCBCC0C0C0434C41C0C0C0C0C0C0
            BDBDBDB4B4B48A8A8A8181827D7D887A7A887D7D888181828A8A8AB4B4B4BDBD
            BDC0C0C0C0C0C0434C41}
          Margin = 0
          ParentShowHint = False
          ShowHint = True
          OnClick = btnFindDecontClick
          ExplicitLeft = 836
        end
        object Label2: TLabel
          Left = 202
          Top = 3
          Width = 36
          Height = 13
          Caption = 'Suma:'
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
        end
        object btnDelJust: TSpeedButton
          Left = 970
          Top = 1
          Width = 20
          Height = 20
          Hint = 'Sterge Decontul Curent'
          Anchors = [akTop, akRight]
          Flat = True
          Glyph.Data = {
            06030000424D060300000000000036000000280000000F0000000F0000000100
            180000000000D002000000000000000000000000000000000000000000000000
            0000000000001919193D3D4472727A7878807979807777774444441515150000
            00000000000000F9991700000000000003030321213F2929850808C20000CB00
            00CB0505C5343490616185717171373737020202000000F99917000000000003
            0808570C0CCD0000FF0000FF0000FF0000FF0000FF0000FF0A0AD43D3D917C7C
            80373737000000F999170000000000470000F30000FF0000FF0000FF0000FF00
            00FF0000FF0000FF0000FF0000F53D3D91717171151515F9991700001C0000C3
            0000FF1414FF8888FF0D0DFF0000FF0000FF0808FF8888FF1919FF0000FF0A0A
            D4616185444444F9991700004C0000FF0000FF7878FFFFFFFFB1B1FF0909FF03
            03FFA7A7FFFFFFFF8888FF0000FF0000FF343490777777F999170000B80000FF
            0000FF0606FF9898FFFFFFFFB5B5FFACACFFFFFFFFA6A6FF0808FF0000FF0000
            FF0505C6797980F999170000C30000FF0000FF0000FF0303FF9D9DFFFFFFFFFF
            FFFFACACFF0303FF0000FF0000FF0000FF0000CB787880F999170000C30000FF
            0000FF0000FF0808FFA6A6FFFFFFFFFFFFFFB5B5FF0909FF0000FF0000FF0000
            FF0000CB72727AF999170000B10000FF0000FF0A0AFFA3A3FFFFFFFFA7A7FF9D
            9DFFFFFFFFB0B0FF0D0DFF0000FF0000FF0808C23D3D44F9991700004C0000FF
            0000FF7878FFFFFFFFA3A3FF0808FF0303FF9898FFFFFFFF8888FF0000FF0000
            FF292985191919F999170000170000B90000FF1010FF7878FF0A0AFF0000FF00
            00FF0606FF7878FF1414FF0000FF0C0CCD21213F000000F99917000000000042
            0000F00000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000F30808
            57030303000000F999170000000000020000420000B90000FF0000FF0000FF00
            00FF0000FF0000FF0000C3000047000003000000000000F99917000000000000
            00000000001700004C0000B00000C30000C30000B800004C00001C0000000000
            00000000000000F99917}
          Margin = 0
          ParentShowHint = False
          ShowHint = True
          OnClick = btnDelJustClick
          ExplicitLeft = 862
        end
        object btnNewDecont: TSpeedButton
          Left = 995
          Top = 1
          Width = 20
          Height = 20
          Hint = 'Creaza un Document de decont'
          Anchors = [akTop, akRight]
          Flat = True
          Glyph.Data = {
            06030000424D060300000000000036000000280000000F0000000F0000000100
            180000000000D002000000000000000000000000000000000000010000080000
            3500004C00004A00004700002E00000400000000000000000000000000000000
            000000000000000000000700009F0000EA0000E40808C645459F4141B202021C
            0000000000000000000000000000000000000000000000000000000000430000
            B63636DEBDBDFFFFFF7474741904040000000000000000000000000000000000
            000000000000000000000000000000002F2F2FCCCCCCCDCDCD4C4C4C30303002
            0202000000000000000000000000000000000000000000000000000000000000
            000000464646E3E3E3F1F1F1DCDCDC2424243C3C3C3C3C3C3C3C3C3C3C3C3C3C
            3C3C3C3C202020000000000000000000000000636363F6F6F6FFFFFFEEEEEE5D
            5D5DB3B3B3F5F5F5DCDCDCF5F5F5DCDCDCF5F5F5696969000000000000000000
            000000838383FFFFFFFFFFFFFFCBCB717171C7C7C7E1E1E1D2D2D2E1E1E1D2D2
            D2E1E1E16868680000000000000000000000000202022E2E2EE7E7E7928F8F82
            8282E1E1E1FDFDFDDCDCDFF7F7FBDCDCDFFDFDFD6A6A6A000000000000000000
            0000000000000000002A2A2ADCDCDC5C5C5CA0A0A0E3E3E36666AE8F8FC76969
            AFE3E3E368686800000000000000000000000000000000000000000000000001
            01017E7E7EF5F5F57C7CBCD7D7EB7D7DBCF5F5F5696969000000000000000000
            050505303030343434363636323232ADADADD7D7D7EBEBEBAEAEC9AFAFD7AFAF
            CAEBEBEB696969000000000000000000181818CECECEDFDFDFE6E6E6D9D9D9EC
            ECECD8D8D8EDEDEDD8D8D8EDEDEDD8D8D8EDEDED696969000000000000000000
            181818A84D4DB83131B83B3BB82A2AB84040A43333B2B2B2C0C0C0B2B2B2C0C0
            C0B2B2B26666660000000000000000001818188C494990343490343490343490
            34348B3F3FA6A6A6A6A6A6A6A6A6A6A6A6A6A6A6717171000000000000000000
            0202020808080808080808080808080808080808080808080808080808080808
            08080808080808000000}
          Margin = 0
          ParentShowHint = False
          ShowHint = True
          OnClick = btnNewDecontClick
          ExplicitLeft = 887
        end
        object BtnModificaDecont: TSpeedButton
          Left = 1020
          Top = 1
          Width = 20
          Height = 20
          Hint = 'Modifica Decontul Curent'
          Anchors = [akTop, akRight]
          Flat = True
          Glyph.Data = {
            36040000424D3604000000000000360000002800000010000000100000000100
            2000000000000004000000000000000000000000000000000000000000000000
            00000000000000000000113A5200275D84004785B900326A8D00273139001414
            1400000000000000000000000000000000000000000000000000000000000000
            000000000000000000002A63800093C7F99090C9F9404084C9222266A99C9CAA
            B6A9161616000000000000000000000000000000000000000000000000000000
            000000000000000000004288A9E0E0F2FF535399D8191979BD484897C444448A
            C29F9FADBAAA1616160000000000000000000000000000000000000000000000
            000000000000000000000B293C0079B5D58F8FB6D15454C9E45A5ADFF57777D0
            ED4D4D99DAA4A4B0BAAA16161600000000000000000000000000000000000000
            000000000000000000002B2B2B007FA1B20075B8D6C1C1F6FD6262DFF75C5CE2
            F87878D3F0474796DBA6A6B1BAAA161616000000000000000000000000000000
            0000000000002C2C2C00B4B4B400E5E5E5AFAFD4E57676CBE7C7C7F7FD5D5DDC
            F55959E1F77A7AD4F1494997DC9C9CADBDAB1717170000000000000000000000
            00002D2D2D00B8B8B800E7E7E7FDFDFDFDFBFBECD6BDBDC39F7878D3EEC7C7F7
            FD5E5EDCF55A5AE2F77979D6F24D4D9EDEA0A0AEBAAB15151500000000002E2E
            2E00BBBBBB00E8E8E8FDFDFDFDFBFBECD6FDFDCD87FFFFD597C0C0CEB17C7CD4
            EDC3C3F6FD6B6BDDF66C6CCAED6262A2D763639CD06E19222A0030303000C0C0
            C000E9E9E9FDFDFDFDFBFBEBD3FFFFCC82FFFFD497FFFFD79DFFFFD69AB4B4C5
            A78080D5EDB1B1E3F98A8ABFE7ADADD3F6C3C3E0FC656299CC00C4C4C400EBEB
            EBFDFDFDFDFAFAFAFAFBFBF3E7FEFECE88FFFFD495FFFFD599FFFFCF8AFDFDE2
            BBAEAEE4F47676BDE7B3B3D2F0E5E5F3FFABABD2EF47417EB500CDCDCDFDFDFD
            FDFDFDFDFDFCFCFCFCF7F7F7F7FDFDF5EAFEFECF89FFFFCC82FDFDE2BBFDFDFD
            FDDCDCDCDC9191BACA5757A4D88484B0DB45459CD02A10374D00CECECEFDFDFD
            FDE0E0E0E0CAC5C5C500B1B1B100F7F7F7FBFBF3E8FDFDE3BCFDFDFDFDDEDEDE
            DEC2C2C2C2BC1010100000000000000000000000000000000000D0D0D0FDFDFD
            FDCDCDCDCDFF00000000A2A2A200F3F3F3FBFBFBFBFDFDFDFDE0E0E0E0C7C7C7
            C7BF101010000000000000000000000000000000000000000000D2D2D2FDFDFD
            FDE2E2E2E2CECECECEE0E0E0E0FDFDFDFDFDFDFDFDE2E2E2E2CBCBCBCBC31010
            1000000000000000000000000000000000000000000000000000D3D3D3FDFDFD
            FDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDE4E4E4E4CDCDCDCDC8111111000000
            0000000000000000000000000000000000000000000000000000D5D5D5D4D4D4
            D4D2D2D2D2D1D1D1D1D0D0D0D0CECECECECDCDCDCDCB11111100000000000000
            0000000000000000000000000000000000000000000000000000}
          Margin = 0
          ParentShowHint = False
          ShowHint = True
          OnClick = BtnModificaDecontClick
          ExplicitLeft = 912
        end
        object edtNrDecont: TcxSpinEdit
          Left = 55
          Top = 0
          TabOrder = 0
          Width = 52
        end
        object edtDataDecont: TcxDateEdit
          Left = 112
          Top = 0
          EditValue = 38040d
          TabOrder = 1
          Width = 86
        end
        object edtDetaliiDecont: TcxPopupEdit
          Left = 555
          Top = 0
          Anchors = [akLeft, akTop, akRight]
          Properties.PopupAutoSize = False
          Properties.PopupControl = pnDeconturi
          Properties.PopupSysPanelStyle = True
          Properties.PopupWidth = 400
          Properties.OnCloseQuery = edtDetaliiDecontPropertiesCloseQuery
          Properties.OnInitPopup = edtDetaliiDecontInitPopup
          TabOrder = 2
          Width = 380
        end
        object edtSumaDecont: TcxCurrencyEdit
          Left = 240
          Top = 0
          Properties.DisplayFormat = ',0.00 ;-,0.00 '
          Properties.ReadOnly = True
          TabOrder = 3
          Width = 85
        end
        object btnAdaugaDecont: TcxButton
          Left = 487
          Top = 0
          Width = 63
          Height = 21
          Caption = 'Adauga'
          TabOrder = 4
          OnClick = btnAdaugaDecontClick
        end
        object edtCodGest: TcxPopupEdit
          Left = 330
          Top = 0
          Properties.PopupControl = frmCasaContainer.TreeRepartitori
          Properties.PopupSysPanelStyle = True
          Properties.OnCloseUp = edtCodGestPropertiesCloseUp
          TabOrder = 5
          Width = 153
        end
      end
    end
    object TreeStructura: TdxDBTreeList
      Left = 304
      Top = 165
      Width = 449
      Height = 209
      SearchType = stContain
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'COD_CB'
      ParentField = 'COD_PARINTE'
      TabOrder = 2
      Visible = False
      OnDblClick = TreeStructuraDblClick
      OnKeyDown = TreeStructuraKeyDown
      DataSource = DTStructure
      LookAndFeel = lfUltraFlat
      OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEnterShowEditor, etoTabThrough]
      OptionsDB = [etoCancelOnExit, etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
      OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoUseBitmap, etoUseImageIndexForSelected]
      StateImages = ImagesStructura
      StateIndexFieldName = 'ICON'
      TreeLineColor = clGrayText
      object TreeStructuraCOD_CB: TdxDBTreeListMaskColumn
        Visible = False
        Width = 24
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_CB'
      end
      object TreeStructuraCOD_PARINTE: TdxDBTreeListMaskColumn
        Visible = False
        Width = 29
        BandIndex = 0
        RowIndex = 0
        FieldName = 'COD_PARINTE'
      end
      object TreeStructuraDENUMIRE: TdxDBTreeListMaskColumn
        Caption = 'Denumire Casa'
        Sorted = csDown
        Width = 325
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DENUMIRE'
        OnGetText = TreeStructuraDENUMIREGetText
      end
      object TreeStructuraDENV: TdxDBTreeListMaskColumn
        Visible = False
        Width = 24
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DENV'
      end
      object TreeStructuraC_O: TdxDBTreeListMaskColumn
        Visible = False
        Width = 24
        BandIndex = 0
        RowIndex = 0
        FieldName = 'C_O'
      end
      object TreeStructuraDATA_SOLD: TdxDBTreeListDateColumn
        Visible = False
        Width = 41
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DATA_SOLD'
      end
      object TreeStructuraCASIER: TdxDBTreeListImageColumn
        Alignment = taLeftJustify
        Caption = 'Este Casier'
        MinWidth = 16
        Visible = False
        Width = 19
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CASIER'
        Descriptions.Strings = (
          ''
          'Casier')
        ImageIndexes.Strings = (
          '0'
          '1')
        ShowDescription = True
        Values.Strings = (
          'False'
          'True')
      end
      object TreeStructuraVALIDATOR: TdxDBTreeListImageColumn
        Alignment = taLeftJustify
        Caption = 'Este Contabil'
        MinWidth = 16
        Visible = False
        Width = 19
        BandIndex = 0
        RowIndex = 0
        FieldName = 'VALIDATOR'
        Descriptions.Strings = (
          ''
          'Contabil')
        ImageIndexes.Strings = (
          '0'
          '1')
        ShowDescription = True
        Values.Strings = (
          'False'
          'True')
      end
      object TreeStructuraADMIN: TdxDBTreeListImageColumn
        Alignment = taLeftJustify
        Caption = 'Este Administrator'
        MinWidth = 16
        Visible = False
        Width = 29
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ADMIN'
        Descriptions.Strings = (
          ''
          'Administrator')
        ImageIndexes.Strings = (
          '0'
          '1')
        ShowDescription = True
        Values.Strings = (
          'False'
          'True')
      end
      object TreeStructuraIS_BANCA: TdxDBTreeListCheckColumn
        Visible = False
        Width = 35
        BandIndex = 0
        RowIndex = 0
        FieldName = 'IS_BANCA'
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object TreeStructuraIS_AVANS: TdxDBTreeListCheckColumn
        Visible = False
        Width = 35
        BandIndex = 0
        RowIndex = 0
        FieldName = 'IS_AVANS'
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object TreeStructuraIS_TEMPOR: TdxDBTreeListCheckColumn
        Visible = False
        Width = 35
        BandIndex = 0
        RowIndex = 0
        FieldName = 'IS_TEMPOR'
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object TreeStructuraID_REPARTITORI: TdxDBTreeListMaskColumn
        Visible = False
        Width = 34
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_REPARTITORI'
      end
      object TreeStructuraICON: TdxDBTreeListMaskColumn
        Visible = False
        Width = 24
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ICON'
      end
      object TreeStructuraID_VALUTA: TdxDBTreeListMaskColumn
        Visible = False
        Width = 24
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ID_VALUTA'
      end
      object TreeStructuraDESCRIERE: TdxDBTreeListMaskColumn
        Caption = 'Descriere'
        Visible = False
        Width = 33
        BandIndex = 0
        RowIndex = 0
        FieldName = 'DESCRIERE'
      end
      object TreeStructuraCRSP_LEI: TdxDBTreeListMaskColumn
        Caption = 'Cont Contabil'
        HeaderAlignment = taCenter
        Width = 97
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CRSP_LEI'
      end
    end
    object pnDeconturi: TPanel
      Left = 340
      Top = 288
      Width = 548
      Height = 194
      TabOrder = 3
      Visible = False
      DesignSize = (
        548
        194)
      object Bevel1: TBevel
        Left = 5
        Top = 159
        Width = 538
        Height = 3
        Anchors = [akLeft, akRight, akBottom]
        Shape = bsBottomLine
        ExplicitWidth = 455
      end
      object btnOkDecont: TcxButton
        Left = 431
        Top = 163
        Width = 48
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Ok'
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
        OnClick = btnOkDecontClick
      end
      object btnCancelDecont: TcxButton
        Left = 482
        Top = 163
        Width = 61
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = 'Cancel'
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
        OnClick = btnCancelDecontClick
      end
      object chkDif: TcxCheckBox
        Left = 16
        Top = 168
        Anchors = [akLeft, akBottom]
        Caption = 'Numai cele cu diferenta'
        TabOrder = 2
        OnClick = chkDifClick
      end
      object TreeDecontari: TcxDBTreeList
        Left = 3
        Top = 2
        Width = 539
        Height = 156
        Anchors = [akLeft, akTop, akRight, akBottom]
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = DTJustificari
        DataController.ParentField = 'COD'
        DataController.KeyField = 'COD'
        DefaultRowHeight = 18
        DragMode = dmAutomatic
        LookAndFeel.Kind = lfUltraFlat
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragExpand = False
        OptionsBehavior.IncSearch = True
        OptionsCustomizing.ColumnsQuickCustomization = True
        OptionsData.Editing = False
        OptionsData.AnsiSort = True
        OptionsData.Deleting = False
        OptionsData.SyncMode = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GridLineColor = clNone
        OptionsView.GroupFooters = tlgfVisibleWhenExpanded
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 3
        OnDblClick = TreeDecontariDblClick
        OnKeyDown = TreeDecontariKeyDown
        object TreeDecontariCOD: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          DataBinding.FieldName = 'COD'
          Options.Editing = False
          Width = 86
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariNR_DECONT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Nr. Decont'
          DataBinding.FieldName = 'NR_DECONT'
          Options.Editing = False
          Width = 79
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          SortIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariDATA_DECONT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikMask
          Caption.AlignVert = vaTop
          Caption.Text = 'Data Decont'
          DataBinding.FieldName = 'DATA_DECONT'
          Options.Editing = False
          Width = 70
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariCODGEST: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Id Repartitor'
          DataBinding.FieldName = 'CODGEST'
          Options.Editing = False
          Width = 405
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariCODSECTIE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Cod Intern'
          DataBinding.FieldName = 'CODSECTIE'
          Options.Editing = False
          Width = 47
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariNUME: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Nume Persoana'
          DataBinding.FieldName = 'NUME'
          Options.Editing = False
          Width = 104
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
          OnGetDisplayText = TreeDecontariNUMEGetDisplayText
        end
        object TreeDecontariDATA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikMask
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Data Avans'
          DataBinding.FieldName = 'DATA'
          Options.Editing = False
          Width = 47
          Position.ColIndex = 14
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariSUMA_DECONT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Suma decont'
          DataBinding.FieldName = 'SUMA_DECONT'
          Options.Editing = False
          Width = 47
          Position.ColIndex = 13
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariCHEIE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          DataBinding.FieldName = 'CHEIE'
          Options.Editing = False
          Width = 47
          Position.ColIndex = 12
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariCOD_CBT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Cod Casa Transfer'
          DataBinding.FieldName = 'COD_CBT'
          Options.Editing = False
          Options.TabStop = False
          Width = 73
          Position.ColIndex = 11
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariAVANS: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Avans'
          DataBinding.FieldName = 'AVANS'
          Options.Editing = False
          Width = 51
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariRETURNAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Returnat'
          DataBinding.FieldName = 'RETURNAT'
          Options.Editing = False
          Width = 50
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariJUSTIFICAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Justificat'
          DataBinding.FieldName = 'JUSTIFICAT'
          Options.Editing = False
          Width = 57
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariDIFERENTA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.Nullable = False
          Properties.ReadOnly = True
          Caption.AlignVert = vaTop
          Caption.Text = 'Diferenta'
          DataBinding.FieldName = 'DIFERENTA'
          Options.Editing = False
          Width = 43
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDecontariPROCENT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxProgressBarProperties'
          Visible = False
          Caption.AlignVert = vaTop
          Caption.Text = 'Procent Justificat'
          DataBinding.FieldName = 'PROCENT'
          Options.Editing = False
          Width = 47
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
  end
  object pnDetail: TcxCollapsedGroup
    Left = 0
    Top = 509
    Align = alBottom
    Caption = 'Detalii Suplimentare'
    ParentFont = False
    TabOrder = 1
    Visible = False
    Height = 91
    Width = 1046
    object Splitter1: TSplitter
      Left = 127
      Top = 18
      Width = -9
      Height = 71
      ExplicitLeft = 126
      ExplicitTop = 21
      ExplicitHeight = 69
    end
    object Splitter3: TSplitter
      Left = 856
      Top = 18
      Height = 71
      Align = alRight
      ExplicitLeft = 749
      ExplicitTop = 21
      ExplicitHeight = 69
    end
    object DBExplicCont: TdxDBMemo
      Left = 2
      Top = 18
      Width = 125
      Align = alLeft
      TabOrder = 0
      DataField = 'MEXPLIC'
      DataSource = DTRegistru
      MaxLength = 800
      OnChange = DBExplicContChange
      Height = 71
      StoredValues = 2
    end
    object DBExplicProj: TdxDBMemo
      Left = 118
      Top = 18
      Width = 738
      Align = alClient
      Enabled = False
      TabOrder = 2
      DataField = 'PEXPLIC'
      DataSource = DTRegistru
      MaxLength = 800
      ReadOnly = False
      Height = 71
      StoredValues = 66
    end
    object pnFilter: TPanel
      Left = 859
      Top = 18
      Width = 185
      Height = 71
      Align = alRight
      BevelInner = bvRaised
      BevelOuter = bvLowered
      Color = clWindow
      TabOrder = 1
      DesignSize = (
        185
        71)
      object lblFilter: TLabel
        Left = 8
        Top = 24
        Width = 5
        Height = 13
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object chkFilter: TCheckBox
        Left = 5
        Top = 3
        Width = 170
        Height = 17
        Caption = 'Filtru Activ / Inactiv'
        TabOrder = 0
        OnClick = chkFilterClick
      end
      object edtTextFiltru: TdxEdit
        Left = 8
        Top = 48
        Width = 169
        Hint = 'explicatie like '#39'%<|>%'#39
        TabOrder = 1
        OnDblClick = edtTextFiltruDblClick
        OnKeyPress = edtTextFiltruKeyPress
        Anchors = [akLeft, akTop, akRight]
        StyleController = StyleController
      end
    end
  end
  object pnSummary: TPanel
    Left = 0
    Top = 601
    Width = 1046
    Height = 42
    Align = alBottom
    AutoSize = True
    BevelInner = bvLowered
    TabOrder = 2
    Visible = False
    object SummStatus: TdxStatusBar
      Left = 2
      Top = 21
      Width = 1042
      Height = 19
      Panels = <
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 50
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Plati'
          Width = 70
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 20
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Filtrat Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Filtrat Plati'
          Width = 80
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end>
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Color = 16776176
    end
    object SelectedSumm: TdxStatusBar
      Left = 2
      Top = 2
      Width = 1042
      Height = 19
      Panels = <
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 50
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Plati'
          Width = 70
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 20
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Nivel2 Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Nivel2 Plati'
          Width = 80
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end>
      Font.Charset = ANSI_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Color = 16776176
    end
  end
  object DTRegistru: TDataSource
    DataSet = MemRegistru
    Left = 32
    Top = 120
  end
  object MemRegistru: TdxMemData
    Indexes = <>
    SortOptions = []
    SortedFields = 'SORTFIELD'
    BeforeDelete = MemRegistruBeforeDelete
    OnCalcFields = MemRegistruCalcFields
    OnNewRecord = MemRegistruNewRecord
    Left = 66
    Top = 120
  end
  object QryRegistru: TZQuery
    Connection = frmData.dbContabilitate
    Filtered = True
    SQL.Strings = (
      'select 1')
    Params = <>
    Left = 64
    Top = 160
  end
  object Cmd_RegistruCasa: TActionList
    Left = 32
    Top = 161
    object Cmd_JustificareAvans: TAction
      Caption = 'Justificare Avans'
      OnExecute = Cmd_JustificareAvansExecute
    end
    object Cmd_EchilibrarePlata: TAction
      Caption = 'Echilibreaza Plata-Incasare'
      ShortCut = 16416
      OnExecute = Cmd_EchilibrarePlataExecute
    end
    object Cmd_AdaugaPlata: TAction
      Caption = 'Adauga Plata Incasare'
      ShortCut = 45
      OnExecute = Cmd_AdaugaPlataExecute
    end
    object Cmd_AdaugaPozitieNoua: TAction
      Caption = 'Adauga Pozite Noua Plata/Incasare'
      ShortCut = 16429
      OnExecute = Cmd_AdaugaPozitieNouaExecute
    end
    object Cmd_AdaugaDefalcare: TAction
      Caption = 'Adaugare Defalcare Plata/Incasare'
      ShortCut = 8237
      OnExecute = Cmd_AdaugaDefalcareExecute
    end
    object Cmd_DeletePlata: TAction
      Caption = 'Stergere Plata'
      ShortCut = 16430
      OnExecute = Cmd_DeletePlataExecute
    end
    object Cmd_SalveazaPlata: TAction
      Caption = 'Salveaza Plata'
      ShortCut = 16467
    end
    object Cmd_TransferaPlata: TAction
      Caption = 'Transfer in alta casa'
      ShortCut = 16468
      OnExecute = Cmd_TransferaPlataExecute
    end
    object Cmd_AcceptaTransfer: TAction
      Caption = 'Accepta Transfer'
      ShortCut = 16449
      OnExecute = Cmd_AcceptaTransferExecute
    end
    object Cmd_Validate: TAction
      Caption = 'Validare'
      ShortCut = 116
      OnExecute = Cmd_ValidateExecute
    end
    object Cmd_UnValidate: TAction
      Caption = 'Scoate aTributul de validare'
      ShortCut = 117
      OnExecute = Cmd_UnValidateExecute
    end
    object Cmd_AnuleazaTransfer: TAction
      Caption = 'Anuleaza Transfer'
      OnExecute = Cmd_AnuleazaTransferExecute
    end
    object Cmd_GenereazaDiferenta: TAction
      Caption = 'Genereaza Diferenta'
      OnExecute = Cmd_GenereazaDiferentaExecute
    end
    object Cmd_Renumeroteaza: TAction
      Caption = 'Renumeroteaza'
      OnExecute = Cmd_RenumeroteazaExecute
    end
    object Cmd_RenumeroteazaAll: TAction
      Caption = 'Renumeroteaza Ecran'
      OnExecute = Cmd_RenumeroteazaAllExecute
    end
    object Cmd_Import: TAction
      Caption = 'Import din alta Casa'
      OnExecute = Cmd_ImportExecute
    end
    object CmdErrors: TAction
      Category = 'Utils'
      Caption = 'Lista de Verificari'
      OnExecute = CmdErrorsExecute
    end
    object Cmd_SaveLocal: TAction
      Category = 'Utils'
      Caption = 'Salveaza datele local'
      OnExecute = Cmd_SaveLocalExecute
    end
    object Cmd_RecalculateSold: TAction
      Category = 'Utils'
      Caption = 'Recalcul Sold'
      Hint = 'Recalcul Sold Incepand cu pozitia curenta'
      OnExecute = Cmd_RecalculateSoldExecute
    end
    object Cmd_ShowDetail: TAction
      Category = 'Utils'
      Caption = 'Afiseaza Detalii'
      OnExecute = Cmd_ShowDetailExecute
    end
    object Cmd_ShowSummary: TAction
      Category = 'Utils'
      Caption = 'Afisarea Banda Sumatoare'
      OnExecute = Cmd_ShowSummaryExecute
    end
    object Cmd_ShowLegend: TAction
      Category = 'Utils'
      Caption = 'Afiseaza Legenda'
      Hint = 'Afiseaza Legenda'
      OnExecute = Cmd_ShowLegendExecute
    end
    object Cmd_ValideazaIesire: TAction
      Caption = 'Valideaza Iesire'
      OnExecute = Cmd_ValideazaIesireExecute
    end
    object Cmd_Flag: TAction
      Caption = 'Flag Inregistrarea Curenta'
    end
    object Cmd_GotoRecord: TAction
      Caption = 'Pozitionare Inregistrare'
      Hint = 'Pozitionare pe Inregistrarea din casa de destinatie'
      OnExecute = Cmd_GotoRecordExecute
    end
    object Cmd_VenitCasa: TAction
      Caption = 'Import Casa/Banca'
      OnExecute = Cmd_VenitCasaExecute
    end
    object Cmd_SetBandSize: TAction
      Category = 'Utils'
      Caption = 'Setare Marime Banda'
      OnExecute = Cmd_SetBandSizeExecute
    end
    object Cmd_DispozitiePlata: TAction
      Caption = 'Tipareste Dispozitie Plata'
      OnExecute = Cmd_DispozitiePlataExecute
    end
    object CmdDecont: TAction
      Category = 'Decontari'
      Caption = 'Decontare Document Furnizor'
      OnExecute = CmdDecontExecute
    end
    object Cmd_TransferaPozitie: TAction
      Caption = 'Transfera pozitie'
      OnExecute = Cmd_TransferaPozitieExecute
    end
    object CmdCopyColumn: TAction
      Caption = 'Copiaza coloana curenta'
      ShortCut = 123
      OnExecute = CmdCopyColumnExecute
    end
  end
  object ImaginiEcl: TImageList
    Left = 32
    Top = 192
    Bitmap = {
      494C010106000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000000000000000
      0000840000008400000084000000840000008400000084000000840000008400
      0000840000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000084000000FFFFFF00FFFFFF00FFFFFF00C6C6C600FFFFFF00C6C6C600FFFF
      FF00840000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000084000000C6C6C600FFFFFF00C6C6C600FFFFFF00C6C6C600FFFFFF00C6C6
      C600840000000000000000000000000000000000000000000000FFFFFF008484
      84008484840084848400FFFFFF0084848400FFFFFF008484840084848400FFFF
      FF00FFFFFF000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000084000000FFFFFF00C6C6C600FFFFFF00C6C6C600FFFFFF00C6C6C600FFFF
      FF00840000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FF000000FF00
      000084000000C6C6C600FFFFFF00C6C6C600FFFFFF00C6C6C600FFFFFF00C6C6
      C600840000000000000000000000000000000000000000000000FFFFFF008484
      840084848400FFFFFF0084848400FFFFFF008484840084848400FFFFFF008484
      8400FFFFFF000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FF000000FFFF
      FF0084000000C6C6C60084000000840000008400000084000000840000008400
      0000840000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FF000000FFFF
      FF00840000008400000084000000840000008400000084000000840000008400
      0000840000000000000000000000000000000000000000000000FFFFFF000000
      FF000000FF000000FF00FFFFFF0084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FF000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF0000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00C6C6
      C600FFFFFF00C6C6C600FFFFFF000000000084848400FFFFFF00000000000000
      0000000000000000000084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008400000084000000FF000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF0000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000084840084848400000000008484
      840000000000C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000C6C6C600FF000000C6C6
      C600FF000000FF000000FF000000FF000000FF000000FF000000FF0000000000
      0000840000008400000084000000840000000000000000000000000000000000
      00000000000000000000000000000000000000FFFF0000000000000000000000
      000084848400C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF00FF000000FF00
      0000FF000000FF000000FF000000FF000000FF000000FF000000FF0000000000
      0000840000008400000084000000C6C6C600FF000000FF000000FF0000000000
      0000C6C6C60084848400FFFFFF000000000000FFFF0000000000000000000000
      0000C6C6C600C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000C6C6C600FFFFFF00C6C6
      C600FFFFFF00C6C6C600FFFFFF00C6C6C6008400000000000000000000000000
      000084000000840000008400000084848400FF00000000000000000000008400
      0000C6C6C600C6C6C600C6C6C6000000000000FFFF0000FFFF00000000000000
      000084848400C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF00C6C6C600FFFF
      FF00C6C6C600FFFFFF00C6C6C600FFFFFF008400000000000000000000000000
      0000840000000000000000000000840000000000000000FF000000FF00000000
      0000840000008484840084848400848484000000000000FFFF00000000000000
      000000FF0000C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000C6C6C600840000008400
      0000840000008400000084000000840000008400000000000000000000000000
      0000000000000000000084848400840000000000000000FF000000FF00000000
      000000000000FF0000000000000000000000000000000000FF000000FF00C6C6
      C600C6C6C600C6C6C60084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008400000084000000840000008400
      0000840000008400000084000000840000008400000000000000000000000000
      000000000000000000008400000000000000000000000000000000000000FF00
      0000000000000000000000000000000000000000000000000000000000008484
      8400848484008484840084848400000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C6000000000000000000000000000000
      000000000000FFFFFF00848484000000FF0084848400FFFFFF00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C60000000000000000000000000000FF
      FF00FFFFFF0000FFFF000000FF000000FF000000FF0000FFFF00FFFFFF0000FF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000FF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C60084848400C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600000000000000000000FFFF00FFFF
      FF0000FFFF00FFFFFF00848484000000FF0084848400FFFFFF0000FFFF00FFFF
      FF0000FFFF000000000000000000000000000000000000000000000000000000
      00000000FF000000FF000000FF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000FFFFFF0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C6000000000084848400C6C6C600C6C6C600C6C6C6008484840084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C6000000000000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF0000FFFF00FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF0000000000000000000000000000000000000000000000
      00000000FF000000FF000000FF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C6000000
      8400000084008484840084848400C6C6C6000000FF0000000000848484008484
      8400C6C6C600C6C6C600C6C6C600C6C6C60000000000FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0000FFFF000000FF0000FFFF00FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0000000000000000000000000000000000000000000000
      FF000000FF000000FF000000FF000000FF000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000FFFFFF00FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C6000000
      8400000084000000000084848400848484000000840000008400000000008484
      8400C6C6C600C6C6C600C6C6C600C6C6C600FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF000000FF008484840000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF000000000000000000000000000000FF000000
      FF000000FF000000FF000000FF000000FF000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000FFFFFF00FFFFFF0000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C6000000
      FF0000008400000084000000000000008400000084000000840000008400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C60000FFFF00FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0000FFFF000000FF000000FF00FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0000FFFF000000000000000000848484000000FF000000
      FF0000000000000000000000FF000000FF000000FF0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C6000000FF000000840000008400000084000000840000008400C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF0000FFFF000000FF000000FF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF0000000000848484000000FF00000000000000
      00000000000000000000000000000000FF000000FF0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C6000000FF000000840000008400000084008484840084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C60000FFFF00FFFFFF0000FFFF00FFFF
      FF00848484008484840000FFFF00FFFFFF00848484000000FF000000FF00FFFF
      FF0000FFFF00FFFFFF0000FFFF00000000000000000000000000000000000000
      00000000000000000000000000000000FF000000FF000000FF00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C6000000FF000000840000008400000084000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600FFFFFF0000FFFF00FFFFFF0000FF
      FF000000FF000000FF00FFFFFF0000FFFF00848484000000FF000000FF0000FF
      FF00FFFFFF0000FFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000FF000000FF00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000FFFFFF00FFFFFF0000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C6000000840000008400000084000000840000008400848484008484
      8400C6C6C600C6C6C600C6C6C600C6C6C60000000000FFFFFF0000FFFF00FFFF
      FF000000FF000000FF0084848400FFFFFF00848484000000FF000000FF00FFFF
      FF0000FFFF00FFFFFF0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000FF000000FF000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C6000000FF000000840000008400848484000000FF0000008400000000008484
      840084848400C6C6C600C6C6C600C6C6C6000000000000FFFF00FFFFFF0000FF
      FF00FFFFFF000000FF000000FF000000FF000000FF000000FF00FFFFFF0000FF
      FF00FFFFFF0000FFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000848484000000
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600000084000000840000000000C6C6C600C6C6C6000000FF00000084000000
      000084848400C6C6C600C6C6C600C6C6C600000000000000000000FFFF00FFFF
      FF0000FFFF00FFFFFF000000FF000000FF000000FF00FFFFFF0000FFFF00FFFF
      FF0000FFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000008484
      84000000FF000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF00FFFFFF000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C6000000
      FF000000FF0000008400C6C6C600C6C6C600C6C6C600C6C6C6000000FF000000
      840000000000C6C6C600C6C6C600C6C6C60000000000000000000000000000FF
      FF00FFFFFF0000FFFF00FFFFFF0000FFFF00FFFFFF0000FFFF00FFFFFF0000FF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000FF000000FF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFFFFF000000
      000000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C6000000
      FF00C6C6C600C6C6C600C6C6C600C6C6C6000000000000000000000000000000
      000000000000FFFFFF0000FFFF00FFFFFF0000FFFF00FFFFFF00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF0000000000000000000000000000000000C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF00F007800300000000F007800100000000
      F007800000000000F007800000000000C007800000000000C007800000000000
      C007800000000000C01F800000000000001F0000000000000010000000000000
      001000000000000000700000000000000076000000000000007C000000000000
      007D810000000000FFFFFFFF00000000FFFFFFFF00000000F83FFFFF00000000
      E00FF9FF00000000C007F0FF000000008003F0FF000000008003E07F00000000
      0001C07F000000000001843F0000000000011E3F000000000001FE1F00000000
      0001FF1F000000008003FF8F000000008003FFC700000000C007FFE300000000
      E00FFFF800000000F83FFFFF0000000000000000000000000000000000000000
      000000000000}
  end
  object MemCont: TdxMemData
    Indexes = <>
    SortOptions = []
    SortedFields = 'ID_PARINTE'
    Left = 64
    Top = 192
  end
  object MemProj: TdxMemData
    Indexes = <>
    SortOptions = []
    SortedFields = 'ID_PARINTE'
    Left = 64
    Top = 224
  end
  object MemReg: TdxMemData
    Indexes = <>
    SortOptions = []
    SortedFields = 'ID_PARINTE'
    Left = 64
    Top = 256
  end
  object ImaginiConturi: TImageList
    Left = 120
    Top = 160
    Bitmap = {
      494C010103000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400000000000000000000000000000000000000
      00000000000000000000000000000000000084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C6000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF00008484000000000000000000000000000000
      0000000000000000000000000000000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FF000000FF000000FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF00008484000084840000000000C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00FF000000C6C6C600FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000FFFF00FFFFFF0000FF
      FF0000FFFF00008484000000000000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000000000000000000000848400FFFFFF0000FF
      FF000084840000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000C6C6C600C6C6C600FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF000000000000FF000000FF000000FF00000000000000FFFF00FFFFFF000000
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00C6C6C600FF000000FF000000FF000000FF00
      0000FF000000FF000000FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000FF000000FF000000FF000000000000FFFFFF000000
      00000000000000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00C6C6C600FF000000FF000000FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF0000848400FFFFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF000000000000FF000000FF000000FF00000000000000FF
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF00FFFFFF0000FFFF000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      000000000000FFFFFF00FFFFFF000000000000FF000000FF000000FF000000FF
      00000000000000FFFF0000848400000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF0000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF0084840000848400000000000000FF000000FF000000FF
      00000000000000848400008484000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF008484840084848400848484008484
      840084848400FFFFFF00C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C6000000000084848400FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00848400000000000000FF000000FF000000FF000000FF
      000000000000FFFFFF00008484000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000008484840084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      000000000000FFFFFF0000FFFF0000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      000000000000000000000000000084848400FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0084848400000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00C1FF007FC107000080FF007F80010000
      0000007F00000000000000030000000000000001000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000080000000800000008000000080000000C001FE00C0010000
      E003FF01E0030000F07FFF83F07F000000000000000000000000000000000000
      000000000000}
  end
  object CheckList: TImageList
    Left = 120
    Top = 192
    Bitmap = {
      494C010106000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C60084848400C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600848484008484840084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400848484008484840084848400C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000FFFFFF00000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400C6C6C600848484008484840084848400C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF0000000000000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C60084848400C6C6C600C6C6C600C6C6C60084848400848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C6008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF00FFFFFFFF00000000FFFFFFFF00000000
      C003C00300000000DFFBDFFB00000000DFFBD9FB00000000DFFBD0FB00000000
      DFFBD07B00000000DFFBD63B00000000DFFBDF1B00000000DFFBDF8B00000000
      DFFBDFCB00000000DFFBDFEB00000000DFFBDFFB00000000C003C00300000000
      FFFFFFFF00000000FFFFFFFF00000000FFFFFFFFFFFFFFFFC001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000
      000000000000}
  end
  object GridRegistruPopup: TPopupMenu
    OnPopup = GridRegistruPopupPopup
    Left = 33
    Top = 256
    object AdaugaPlataIncasare: TMenuItem
      Action = Cmd_AdaugaPlata
    end
    object AdaugaPoziteNouaPlataIncasare1: TMenuItem
      Action = Cmd_AdaugaPozitieNoua
    end
    object AdaugareDefalcarePlataIncasare1: TMenuItem
      Action = Cmd_AdaugaDefalcare
    end
    object StergerePlata: TMenuItem
      Action = Cmd_DeletePlata
    end
    object EchilibreazaPlataIncasare: TMenuItem
      Action = Cmd_EchilibrarePlata
    end
    object Renumeroteaza: TMenuItem
      Action = Cmd_Renumeroteaza
    end
    object RenumeroteazaEcran1: TMenuItem
      Action = Cmd_RenumeroteazaAll
    end
    object SetareMarimeBanda1: TMenuItem
      Action = Cmd_SetBandSize
      Caption = 'Setare Marime Banda "Stare"'
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object PozitionareInregistrare1: TMenuItem
      Action = Cmd_GotoRecord
      Caption = 'Pozitionare Inregistrare ->'
    end
    object Transferinaltacasa: TMenuItem
      Action = Cmd_TransferaPozitie
    end
    object AcceptaTransfer: TMenuItem
      Action = Cmd_AcceptaTransfer
    end
    object AnuleazaTransfer: TMenuItem
      Action = Cmd_AnuleazaTransfer
    end
    object ImportCasaBanca1: TMenuItem
      Action = Cmd_VenitCasa
    end
    object N4: TMenuItem
      Caption = '-'
    end
    object GenereazaDiferenta: TMenuItem
      Action = Cmd_GenereazaDiferenta
    end
    object JustificareAvans: TMenuItem
      Action = Cmd_JustificareAvans
    end
    object TiparesteDispozitiePlata1: TMenuItem
      Action = Cmd_DispozitiePlata
    end
    object N2: TMenuItem
      Caption = '-'
    end
    object Validare: TMenuItem
      Tag = 2
      Action = Cmd_Validate
    end
    object DeValidare1: TMenuItem
      Tag = 2
      Action = Cmd_UnValidate
      Caption = 'DeValidare'
    end
    object FlagInregistrareaCurenta1: TMenuItem
      Action = Cmd_Flag
    end
    object N3: TMenuItem
      Caption = '-'
    end
    object ImportdinaltaCasa1: TMenuItem
      Action = Cmd_Import
    end
    object DecontareDocumentFurnizor1: TMenuItem
      Action = CmdDecont
    end
    object Copiazacoloanacurenta1: TMenuItem
      Action = CmdCopyColumn
    end
  end
  object DTJustificari: TDataSource
    DataSet = QryJustificari
    Left = 226
    Top = 240
  end
  object QryJustificari: TZQuery
    Connection = frmData.dbContabilitate
    Filtered = True
    SQL.Strings = (
      'EXEC SP_JUSTIFICARI :COD_CB')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
        Value = 12
      end>
    Left = 298
    Top = 240
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
        Value = 12
      end>
  end
  object ValidariList1: TImageList
    Height = 12
    Width = 12
    Left = 32
    Top = 360
    Bitmap = {
      494C01010300050004000C000C00FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000300000000C00000001002000000000000009
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000FF000000FF0000000000000000
      000000000000000000000000000000000000000000000000000000000000FF00
      0000FF0000008484000084840000848400008400000084000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      8400000084000000840000008400000084000000000000000000000000000000
      00000000000000000000000000000000000000FF000000FF0000000000000000
      000000000000000000000000000000000000000000000000000000000000FF00
      0000FFFF0000FFFF000084840000848400008400000084000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000008400000084000000
      8400000084000000840000008400000084000000840000008400000000000000
      0000000000000000000000FF000000FF000000FF000000840000008400000000
      000000000000000000000000000000000000000000000000000000000000C6DE
      C600C6DEC600FFFF0000FFFF0000FFFF00008484000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000840000008400C6C6
      C6000000FF000000FF000000FF00C6C6C6008484840000008400000000000000
      000000000000008400000084000000FF000000FF000000840000008400000000
      000000000000000000000000000000000000000000000000000000000000C6DE
      C600FFFFFF00C6DEC60084840000FFFF000084840000FF000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000084000000FF0084848400F7FF
      FF00848400000000FF0084848400FFFFFF008484840000008400000084000000
      000000000000008400000084000000FF00008484840000FF0000008400000000
      000000000000000000000000000000000000000000000000000000000000C6DE
      C600FFFFFF00C6DEC600FFFF0000FFFF000084840000FF000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000FF000000FF000000FF000000
      FF00F7FFFF00FFFFFF00FFFFFF000000FF000000FF000000FF00000084000000
      00000000000000FF000000FF000000FF0000848484008484840000FF00000084
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6DEC600C6DEC600C6DEC60084000000FF00000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000FF000000FF000000FF000000
      FF00F7FFFF00FFFFFF00FFFFFF000000FF000000FF000000FF00000084000000
      00000000000000FF00000084000000000000000000008484840000FF00000084
      0000000000000000000000000000000000000000000000000000000000000000
      000084840000C6DEC600FFFF0000840000008400000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000FF000000FF000000FF008484
      8400FFFFFF00C6C6C600F7FFFF00848400000000FF000000FF00000084000000
      0000000000000000000000000000000000000000000084848400848484000084
      0000008400000000000000000000000000000000000000000000000000008484
      0000C6DEC600C6DEC600C6DEC600848400008400000084000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000FF000000FF0084840000C6C6
      C6000000FF000000FF000000FF00C6C6C600848400000000FF00000000000000
      0000000000000000000000000000000000000000000000000000000000000084
      0000008400000084000000000000000000000000000000000000000000008484
      0000FFFFFF00FFFFFF00F7FFFF00FFFF00008484000084000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000FF00848400008484
      00000000FF000000FF000000FF000000FF000000FF000000FF00000000000000
      0000000000000000000000000000000000000000000000000000000000008484
      8400008400000084000000000000000000000000000000000000000000008484
      0000FFFFFF00FFFFFF00FFFFFF00C6DEC6008484000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000FF000000FF00C6C6
      C600C6C6C600C6C6C600C6C6C600848400000000FF0000008400000000000000
      0000000000000000000000000000000000000000000000000000000000008484
      840084848400008400000084000000000000000000000000000000000000C6DE
      C600C6DEC600C6DEC600C6DEC60084840000FF000000FF000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      FF000000FF00848400000000FF000000FF000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000FF000000FF00000000000000000000000000000000000000000000C6DE
      C600C6DEC600C6DEC600C6DEC600C6DEC600FF000000FF000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      28000000300000000C0000000100010000000000600000000000000000000000
      000000000000000000000000FFFFFF00F07F3FE030000000C03F3FE030000000
      801C1FE07000000080181FE03000000000181FE03000000000180FF070000000
      00198FF070000000001F87E030000000001FE3E030000000803FE3E070000000
      803FE1E030000000E0FFF3E03000000000000000000000000000000000000000
      000000000000}
  end
  object QryShare_Point: TZQuery
    Connection = frmData.dbContabilitate
    Filtered = True
    SQL.Strings = (
      'SELECT * FROM SHARE_POINT WHERE ID_UTILIZATOR = :ID_UTILIZATOR')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 69
    Top = 328
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object StyleController: TdxEditStyleController
    BorderStyle = xbsSingle
    ButtonStyle = btsSimple
    HotTrack = True
    Left = 110
    Top = 124
  end
  object ValidariList: TImageList
    Left = 64
    Top = 360
    Bitmap = {
      494C010107000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000C6C6C600C6C6
      C6008484000084848400C6C6C600C6C6C6000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600C6C6C6008484
      8400000000008484840084848400C6C6C600C6C6C60000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C6008484840000FF
      000000FF0000000000008484840084848400C6C6C600C6C6C600000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000848400000000FF000000FF000000FF000000FF00C6C6C6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C6008484000000FF000000FF
      000000FF000000FF0000000000008484840084848400C6C6C600C6C6C6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000848484000000FF000000FF000000FF000000FF000000FF000000FF008484
      8400000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C6008484000000FF000000FF
      000000FF000000FF000000FF0000000000008484840084848400C6C6C600C6C6
      C600000000000000000000000000000000000000000000000000000000000000
      0000FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000084
      8400000084000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF00848484000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C6008484000000FF000000FF
      000000FF00000084000000FF000000FF0000000000008484840084848400C6C6
      C600C6C6C6000000000000000000000000000000000000000000000000000000
      0000FFFFFF0000000000FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6000000
      FF000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF000000FF00C6C6C60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C6008484000000FF000000FF
      000000840000008400000084000000FF000000FF000000000000848484008484
      8400C6C6C600C6C6C60000000000000000000000000000000000000000000000
      0000FFFFFF0000000000FFFFFF00000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000FF000000
      FF000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF000000FF000000FF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C6008484000000FF000000FF
      00000084000084848400848484000084000000FF000000FF0000000000008484
      840084840000C6C6C600C6C6C60000000000000000000000000000000000FFFF
      FF0000000000FFFFFF0000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000FF000000
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000FF000000FF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600848400000084000000FF
      00000084000084840000C6C6C600848484000084000000FF0000008400000000
      00008484840084840000C6C6C600C6C6C600000000000000000000000000FFFF
      FF0000000000FFFFFF0000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000FF000000
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000FF000000FF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600848484000084
      0000C6C6C600C6C6C600C6C6C600C6C6C600848484000084000000FF00000000
      0000840084008484840084840000C6C6C6000000000000000000FFFFFF000000
      00000000FF0000000000FFFFFF00FFFFFF000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000FF000000
      FF000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF000000FF000000FF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C600C6C6
      C600C6C6C6000000000000000000C6C6C600C6C6C600848484000084000000FF
      0000000000008400840084848400848400000000000000000000FFFFFF000000
      00000000FF00000084000000000000000000FFFFFF0000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6000000
      FF000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF000000FF00C6C6C60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600848484000084
      000000FF000000000000840084008484000000000000FFFFFF00000000000000
      FF0000008400000084000000FF000000840000000000FFFFFF00000000000000
      0000000000000000000000000000000000000000000000000000000000008484
      84000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      8400000084000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600C6C6C6008484
      84000084000000FF0000000000008484000000000000FFFFFF00000000000000
      FF00000084000000FF0000008400000084000000840000000000FFFFFF000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000848484000000FF000000FF000000FF000000FF000000FF000000FF000084
      8400000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C600C6C6
      C60084848400008400000084000084840000FFFFFF00000000000000FF000000
      0000000000000000FF000000840000000000000000000000000000000000FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C6000000FF000000FF000000FF000000FF00C6C6C6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600C6C6C6008484000084840000C6C6C600FFFFFF000000000000000000FFFF
      FF00FFFFFF000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600C6C6C60000000000FFFFFF00FFFFFF00FFFFFF000000
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000C6C6C600C6C6
      C6008484000084848400C6C6C600C6C6C6000000000000000000000000000000
      0000000000000000000000000000000000000000000084000000840000008400
      0000840000008400000084000000840000008400000084000000840000008400
      0000840000008400000084000000840000000000000000000000C6C6C600C6C6
      C6008484000084848400C6C6C600C6C6C6000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FF
      FF00FFFFFF00F7FFFF00FFFFFF00FFFFFF0000000000C6C6C600C6C6C6008484
      8400000000008484840084848400C6C6C600C6C6C60000000000000000000000
      0000000000000000000000000000000000008400000084000000840000008400
      00008400000084008400C6DEC600C6DEC600C6DEC600C6DEC600840000008400
      00008400000084000000840000008400000000000000C6C6C600C6C6C6008484
      8400000000008484840084848400C6C6C600C6C6C60000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00848400000000FF000000FF000000FF000000FF00C6C6C600F7FF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF00C6C6C600C6C6C6008484840000FF
      000000FF0000000000008484840084848400C6C6C600C6C6C600000000000000
      0000000000000000000000000000000000008400000084000000840000008400
      0000FFFFFF00FFFFFF00F7FFFF00FFFFFF00F7FFFF00FFFFFF00FFFFFF00C6DE
      C60084000000840000008400000084000000C6C6C600C6C6C60084848400C6C6
      C600C6C6C600000000008484840084848400C6C6C600C6C6C600000000000000
      000000000000000000000000000000000000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00848484000000FF000000FF000000FF000000FF000000FF000000FF008484
      8400F7FFFF00F7FFFF00F7FFFF00FFFFFF00C6C6C6008484000000FF000000FF
      000000FF000000FF0000000000008484840084848400C6C6C600C6C6C6000000
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00840000008400000084000000C6C6C60084840000C6C6C600C6C6
      C600C6C6C600C6C6C600000000008484840084848400C6C6C600C6C6C6000000
      000000000000000000000000000000000000F7FFFF00F7FFFF00F7FFFF000084
      8400000084000000FF000000FF000000FF000000FF000000FF000000FF000000
      FF0084848400F7FFFF00F7FFFF00F7FFFF00C6C6C6008484000000FF000000FF
      000000FF000000FF000000FF0000000000008484840084848400C6C6C600C6C6
      C600000000000000000000000000000000008400000084000000C6DEC600FFFF
      FF00FFFFFF00F7FFFF0084000000840000008400000084848400FFFFFF00FFFF
      FF00FFFFFF00C6DEC6008400000084000000C6C6C60084840000C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600000000008484840084848400C6C6C600C6C6
      C60000000000000000000000000000000000FFFFFF00F7FFFF00C6C6C6000000
      FF000000FF0084840000C6C6C6000000FF000000FF00C6C6C600848400000000
      FF000000FF00C6C6C600F7FFFF00F7FFFF00C6C6C6008484000000FF000000FF
      000000FF00000084000000FF000000FF0000000000008484840084848400C6C6
      C600C6C6C6000000000000000000000000008400000084000000FFFFFF00FFFF
      FF00FFFFFF00840000008400000084000000840000008400000084000000FFFF
      FF00FFFFFF00FFFFFF008400000084000000C6C6C60084840000C6C6C600C6C6
      C600C6C6C60000840000C6C6C600C6C6C600000000008484840084848400C6C6
      C600C6C6C600000000000000000000000000F7FFFF00F7FFFF000000FF000000
      FF000000FF000000FF00C6C6C60084840000C6C6C600F7FFFF000000FF000000
      FF000000FF000000FF00F7FFFF00FFFFFF00C6C6C6008484000000FF000000FF
      000000840000008400000084000000FF000000FF000000000000848484008484
      8400C6C6C600C6C6C600000000000000000084000000C6DEC600FFFFFF00FFFF
      FF00848484008400000084000000840000008400000084000000840000008400
      0000FFFFFF00FFFFFF00C6DEC60084000000C6C6C60084840000C6C6C600C6C6
      C600008400000084000000840000C6C6C600C6C6C60000000000848484008484
      8400C6C6C600C6C6C6000000000000000000FFFFFF00F7FFFF000000FF000000
      FF000000FF000000FF0084848400F7FFFF00F7FFFF00848484000000FF000000
      FF000000FF000000FF00F7FFFF00FFFFFF00C6C6C6008484000000FF000000FF
      00000084000084848400848484000084000000FF000000FF0000000000008484
      840084840000C6C6C600C6C6C6000000000084000000C6DEC600FFFFFF00FFFF
      FF00840000008400000084000000840000008400000084000000840000008400
      0000F7FFFF00FFFFFF00C6DEC60084000000C6C6C60084840000C6C6C600C6C6
      C60000840000848484008484840000840000C6C6C600C6C6C600000000008484
      840084840000C6C6C600C6C6C60000000000FFFFFF00F7FFFF000000FF000000
      FF000000FF000000FF0084848400F7FFFF00F7FFFF00840084000000FF000000
      FF000000FF000000FF00F7FFFF00FFFFFF00C6C6C600848400000084000000FF
      00000084000084840000C6C6C600848484000084000000FF0000008400000000
      00008484840084840000C6C6C600C6C6C60084000000C6DEC600FFFFFF00FFFF
      FF00840000008400000084000000840000008400000084000000840000008400
      0000FFFFFF00FFFFFF00F7FFFF0084000000C6C6C6008484000000840000C6C6
      C6000084000084840000C6C6C6008484840000840000C6C6C600008400000000
      00008484840084840000C6C6C600C6C6C600FFFFFF00F7FFFF000000FF000000
      FF000000FF000000FF00F7FFFF00C6C6C600C6C6C600C6C6C6000000FF000000
      FF000000FF000000FF00F7FFFF00F7FFFF0000000000C6C6C600848484000084
      0000C6C6C600C6C6C600C6C6C600C6C6C600848484000084000000FF00000000
      0000840084008484840084840000C6C6C6008400000084848400FFFFFF00FFFF
      FF00C6DEC60084000000F7FFFF00840000008400000084000000840000008400
      0000FFFFFF00FFFFFF00C6DEC6008400000000000000C6C6C600848484000084
      0000C6C6C600C6C6C600C6C6C600C6C6C6008484840000840000C6C6C6000000
      0000840084008484840084840000C6C6C600F7FFFF00F7FFFF00C6C6C6000000
      FF000000FF0084840000C6C6C6000000FF000000FF00C6C6C600848400000000
      FF000000FF00C6C6C600F7FFFF00FFFFFF000000000000000000C6C6C600C6C6
      C600C6C6C6000000000000000000C6C6C600C6C6C600848484000084000000FF
      0000000000008400840084848400848400008400000084000000FFFFFF00FFFF
      FF00FFFFFF00C6DEC600F7FFFF0084000000840000008400000084000000FFFF
      FF00FFFFFF00FFFFFF0084848400840000000000000000000000C6C6C600C6C6
      C600C6C6C6000000000000000000C6C6C600C6C6C6008484840000840000C6C6
      C60000000000840084008484840084840000F7FFFF00F7FFFF00F7FFFF008484
      84000000FF000000FF000000FF000000FF000000FF000000FF000000FF000000
      840000008400F7FFFF00F7FFFF00F7FFFF000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600848484000084
      000000FF0000000000008400840084840000840000008400000084848400FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00840000008400000084840000FFFFFF00F7FF
      FF00FFFFFF00FFFFFF0084000000840000000000000000000000000000000000
      000000000000000000000000000000000000C6C6C600C6C6C600848484000084
      0000C6C6C600000000008400840084840000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00848484000000FF000000FF000000FF000000FF000000FF000000FF000084
      8400F7FFFF00F7FFFF00F7FFFF00FFFFFF000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600C6C6C6008484
      84000084000000FF00000000000084840000840000008400000084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000C6C6C600FFFFFF00F7FFFF00FFFF
      FF00FFFFFF008400000084000000840000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600C6C6C6008484
      840000840000C6C6C6000000000084840000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00C6C6C6000000FF000000FF000000FF000000FF00C6C6C600F7FF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C600C6C6
      C600848484000084000000840000848400008400000084000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0084000000C6C6C600FFFFFF00FFFFFF00C6DE
      C600840000008400000084000000840000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C600C6C6
      C60084848400008400000084000084840000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600C6C6C6008484000084840000C6C6C60084000000FF000000848484008484
      84008484840084840000C6DEC600840000008484840084848400840000008400
      0000840000008400000084000000840000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600C6C6C6008484000084840000C6C6C60000000000FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600C6C6C600000000008400000084000000840000008400
      0000840000008400000084000000840000008400000084000000840000008400
      0000840000008400000084000000840000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600C6C6C60000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF00C0FFFFFFFFFF0000807FFFFFFFFF0000
      003FFFFFF81F0000001FFFFFF00F0000000FF1FFE00700000007F1FFC0030000
      0003F1FFC00300000001E3FFCFF300000000E3FFCFF300008000C0FFC0030000
      C600C07FC00300009B00803FE00700006A80801FF00F000068C0000FF81F0000
      6AE0000FFFFF00009B7919FFFFFF00008001C0FF8000C0FF0000807F0000807F
      0000003F0000003F0000001F0000001F0000000F0000000F0000000700000007
      0000000300000003000000010000000100000000000000000000800000008000
      0000C6000000C6000000FF000000FF000000FF800000FF800000FFC00000FFC0
      0000FFE00000FFE08001FFF90000FFF900000000000000000000000000000000
      000000000000}
  end
  object QryStructure: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_GET_CASA_STRUCTURA :COD_UTILIZATOR,  :IS_ADMIN, :DISP_WA' +
        'Y')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 584
    Top = 144
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTStructure: TDataSource
    DataSet = QryStructure
    Left = 552
    Top = 144
  end
  object ImagesStructura: TImageList
    Left = 648
    Top = 208
    Bitmap = {
      494C010108000D00040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000003000000001002000000000000030
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF008484000084840000C6C6C600F7FFFF00F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00F7FFFF000000000000000000000000000000
      000000000000F7FFFF008484000084848400C6C6C600F7FFFF00F7FFFF00FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000C6C6C600848400008484840084840000C6C6C60084840000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C600848400008484840084840000C6C6C60084840000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C60084840000C6C6C6008484000084840000C6C6C600F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00000000000000
      0000FFFFFF00C6C6C60084840000848400008484840084840000C6C6C600F7FF
      FF00FFFFFF0000000000FFFFFF0000000000000000000000000000000000C6C6
      C600848400008484840084848400008484000084840000848400848400000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600848400008484840084848400848484008484840084848400848400000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00C6C6C600C6DEC600C6DEC600C6DEC60084840000848400008484
      0000F7FFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF0000000000FFFF
      FF00F7FFFF0084840000C6C6C600C6DEC600C6C6C60084840000848484008484
      0000C6DEC600F7FFFF00FFFFFF00FFFFFF000000000000000000000000008484
      0000848484008484000084840000008484000084840000848400008484008484
      0000000000000000000000000000000000000000000000000000000000008484
      000084848400C6C6C60084840000848484008484840084840000848484008484
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00C6DEC60084840000C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC6008484
      000084848400C6C6C600F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00FFFF
      FF00C6DEC60084840000C6DEC600F7FFFF00C6C6C600C6C6C600C6C6C6008484
      000084848400C6C6C600F7FFFF00FFFFFF0000000000C6C6C600848400008484
      840084848400848484000084840000FFFF0000FFFF0000FFFF00008484000084
      84008484840084840000C6C6C6000000000000000000C6C6C600848400008484
      8400848400008484840084848400C6C6C600C6DEC600C6DEC600848400008484
      84008484840084840000C6C6C60000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00C6C6C600C6C6C600C6DEC600C6DEC600C6DEC600F7FFFF00C6DEC600C6DE
      C600C6DEC60084848400C6C6C600FFFFFF00FFFFFF0000000000FFFFFF00FFFF
      FF0084840000C6C6C600C6DEC600C6C6C600C6DEC600C6DEC600C6C6C600C6C6
      C6008484000084848400C6C6C600F7FFFF00C6C6C60084848400848484008484
      8400848484008484000084840000848400008484000084840000C6DEC6008484
      840084848400008484008484840084840000C6C6C60084840000848484008484
      840084840000C6C6C60084840000C6C6C60084840000C6C6C600C6C6C6008484
      840084848400848484008484840084840000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF0084840000C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC600C6DEC600F7FF
      FF00C6DEC60084840000C6C6C600FFFFFF0000000000FFFFFF00FFFFFF00F7FF
      FF0084840000C6DEC600C6DEC600C6C6C600C6DEC600C6C6C600C6DEC600F7FF
      FF00C6C6C60084840000C6C6C600FFFFFF00848400008484840084848400C6C6
      C600F7FFFF00C6DEC60084848400000084000084840084848400848484008484
      840000848400008484000000840084848400848400008484840084848400C6C6
      C600F7FFFF00C6DEC60084848400000000008484840084848400848484008484
      840084848400848484000000000084848400FFFFFF00FFFFFF00FFFFFF00C6C6
      C600C6DEC600F7FFFF00F7FFFF00C6DEC600F7FFFF00C6DEC600C6DEC600C6DE
      C600C6DEC60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00C6C6
      C600C6C6C600F7FFFF00C6DEC600C6DEC600C6DEC600C6C6C600C6DEC600C6DE
      C600C6C6C60084840000F7FFFF00FFFFFF0084848400C6C6C600F7FFFF00F7FF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848400008484
      0000C6C6C60084848400848484008484840084848400C6C6C600F7FFFF00FFFF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848400008484
      0000C6C6C600848400008484840084848400FFFFFF00FFFFFF00FFFFFF00C6C6
      C600F7FFFF00C6DEC600C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC600C6DE
      C60084840000C6C6C600FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF008484
      0000C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6C6
      C60084840000C6C6C600FFFFFF00FFFFFF00C6C6C60084840000C6DEC600F7FF
      FF00F7FFFF00C6DEC600C6C6C60084840000848484000084840084840000F7FF
      FF00F7FFFF00F7FFFF008484840084840000C6C6C60084840000C6DEC600FFFF
      FF00F7FFFF00C6DEC600C6C6C60084840000848484008484840084840000F7FF
      FF00FFFFFF00F7FFFF008484000084840000FFFFFF00FFFFFF00F7FFFF00C6C6
      C600F7FFFF00F7FFFF00C6DEC600C6DEC600C6C6C600C6C6C600C6C6C600C6DE
      C60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600C6C6
      C600F7FFFF00F7FFFF00C6DEC600C6DEC600C6C6C600C6C6C600C6DEC600C6C6
      C60084848400F7FFFF00FFFFFF0000000000000000008484000084840000F7FF
      FF00F7FFFF00F7FFFF00C6C6C60084848400848484000084840084848400C6C6
      C600C6DEC6008484840084840000C6C6C600000000008484000084840000F7FF
      FF00FFFFFF00F7FFFF00C6C6C60084848400848400008484840084848400C6C6
      C600C6DEC600848400008484000084840000FFFFFF00FFFFFF00C6C6C600C6DE
      C600C6DEC600C6DEC600F7FFFF00F7FFFF00C6C6C6008484840084840000C6C6
      C60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C600C6C6
      C600C6C6C600C6DEC600F7FFFF00F7FFFF008484000084848400848400008484
      000084840000F7FFFF00FFFFFF00000000000000000000000000C6C6C6008484
      0000C6DEC600FFFFFF00F7FFFF008484000084840000C6C6C600848484000084
      840084848400C6C6C600C6C6C600C6C6C6000000000000000000C6C6C6008484
      0000C6DEC600FFFFFF00F7FFFF008484000084840000C6C6C600848400008484
      840084848400C6C6C600C6C6C600C6C6C600FFFFFF00F7FFFF00C6DEC6008484
      84008484840084848400C6DEC600C6DEC600C6C6C60084840000848400008484
      0000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      00008484000084840000C6C6C600C6DEC600C6C6C60084840000848400008484
      8400C6DEC600FFFFFF000000000000000000000000000000000000000000C6C6
      C60084840000C6DEC600FFFFFF00F7FFFF008484840084840000C6DEC6008484
      84008484840084840000C6C6C60000000000000000000000000000000000C6C6
      C60084840000C6DEC600FFFFFF00F7FFFF0084840000C6C6C600C6C6C6008484
      00008484840084840000C6C6C60000000000FFFFFF00F7FFFF00C6DEC6008484
      00008484840000FF000000FF00008484840084840000C6C6C600C6C6C6008484
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      00008484000084840000848484008484000084840000C6C6C600C6C6C6008484
      0000F7FFFF000000000000000000FFFFFF000000000000000000000000000000
      00008484000084840000C6C6C600848400008484840084840000C6C6C6008484
      0000848484008484840084848400C6C6C6000000000000000000000000000000
      00008484000084840000C6C6C60084840000848484008484000000000000C6C6
      C600848400008484840084848400C6C6C600FFFFFF00F7FFFF00C6DEC600C6C6
      C600C6DEC600C6DEC6008484840000FF000000FF00008484840084840000C6C6
      C600FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6DEC600C6C6
      C600C6C6C600C6C6C6008484000084848400848484008484000084840000C6C6
      C600FFFFFF000000000000000000000000000000000000000000000000000000
      000000000000C6C6C60084840000C6C6C600C6C6C600C6C6C60084840000C6C6
      C600C6C6C6008484840084848400848400000000000000000000000000000000
      000000000000C6C6C6008484000084840000C6C6C600C6C6C60084840000C6C6
      C60000000000848484008484840084848400F7FFFF00FFFFFF00FFFFFF00F7FF
      FF00C6C6C600C6C6C600C6C6C60084840000848400008484840084840000F7FF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00C6C6C600C6C6C600C6C6C60084840000848484008484840084840000F7FF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6008484
      0000848484008484000084848400840084000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6008484
      000084840000848400008484840084848400F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6DEC600C6C6C600C6C6C6008484000084840000C6C6C600FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFF
      FF00F7FFFF00C6DEC600C6C6C600C6C6C6008484000084840000C6C6C600F7FF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084840000C6C6C600848484000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084840000C6C6C60084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6C6C60084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF0000000000FFFF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6C6C60084840000C6DEC600FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C6008484840084840000848400000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C60084848400848400008484000000000000FFFFFF00C6DEC600C6DE
      C600C6C6C6008484840000848400008484000084840084848400C6C6C600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6C6C600C6C6
      C600C6C6C6008484840084848400848400008484840084848400C6C6C600C6C6
      C600C6C6C600C6C6C600F7FFFF000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00C6C6C60084848400C6DEC60084848400C6DEC600F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000000000000000FFFFFF000000
      0000FFFFFF00F7FFFF00C6C6C60084848400C6DEC60084848400C6C6C600F7FF
      FF00FFFFFF000000000000000000FFFFFF00FFFFFF00FFFFFF00C6DEC600C6DE
      C600C6DEC6008484840000848400008484008484840084848400C6DEC600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6DEC600C6C6
      C600C6C6C6008484840084848400848400008484000084848400C6C6C600C6DE
      C600C6C6C600C6C6C600F7FFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6C6C6008484000084848400848400008484840084848400848400008484
      000084840000C6DEC600F7FFFF00F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6C6C6008484000084848400848484008484000084848400848400008484
      0000C6C6C600C6C6C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600C6DE
      C600C6DEC600848484000084840000FFFF008484000084848400C6DEC600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6DEC600C6DE
      C600C6DEC6008484840084840000848400008484000084848400C6DEC600C6DE
      C600C6C6C600C6C6C600F7FFFF0000000000FFFFFF00C6DEC600848400008484
      8400848484008484840084840000848400008484840084840000848484008484
      0000008400000084000084848400C6DEC600FFFFFF00C6DEC600848400008484
      0000848400008484000084848400848484008484840084848400848484008484
      8400848484008484840084848400C6DEC600FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00C6DEC6008484840084840000C6C6C6008484000084848400F7FFFF00F7FF
      FF00C6DEC600C6C6C600F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00C6DEC60084840000C6C6C600C6C6C600C6C6C60084848400F7FFFF00F7FF
      FF00C6DEC600C6C6C600F7FFFF0000000000F7FFFF0084840000008400000084
      0000C6C6C600C6DEC60084848400848484008484840084840000848400008484
      000084840000008400000084000084848400F7FFFF0084840000848484008484
      8400C6C6C600C6DEC60084848400848484008484000084848400C6C6C6008484
      000084848400848484008484840084840000FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C600C6C6C6008484000084848400F7FFFF00F7FF
      FF00F7FFFF0084840000F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C600C6C6C600C6C6C60084840000F7FFFF00F7FF
      FF00C6DEC60084840000F7FFFF00000000008484840084840000848400008484
      000084840000C6DEC60084848400848484008484840084840000C6DEC6008484
      0000848400008484000000840000008400008484000084848400848484008484
      840084840000C6DEC60084848400848484008484000084848400C6C6C600C6C6
      C60084848400848484008484840084848400F7FFFF00C6C6C600C6DEC600F7FF
      FF00F7FFFF008484000084848400848484008484840084848400F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C60000000000F7FFFF00C6C6C600C6DEC600F7FF
      FF00F7FFFF008484000084848400848484008484840084848400FFFFFF00F7FF
      FF00F7FFFF0084848400C6C6C60000000000848400008484840084848400C6DE
      C600F7FFFF00C6DEC60084848400848484008484840000840000848484008484
      840084840000848400008484000000840000848400008484000084840000C6C6
      C600F7FFFF00C6DEC60084848400848484008484000084848400848400008484
      000084848400848484008484840084848400F7FFFF008484840084848400C6DE
      C600F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00C6DE
      C6008484840084848400C6DEC60000000000F7FFFF008484840084848400C6DE
      C600F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6DE
      C6008484840084848400C6DEC60000000000F7FFFF00C6DEC600C6DEC600F7FF
      FF00F7FFFF008484000084840000008400008484840000840000848400008484
      000084840000848400008484840084840000F7FFFF00C6DEC600C6DEC600F7FF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848484008484
      840084848400848484008484840084840000F7FFFF00C6C6C600008484008484
      8400C6C6C600F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00C6C6C6008484
      84000000840084848400F7FFFF0000000000F7FFFF00C6C6C600848484008484
      8400C6C6C600F7FFFF00FFFFFF00F7FFFF00F7FFFF00FFFFFF00C6C6C6008484
      84008484840084848400F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF008484
      0000848484008484840084840000848400008484000084840000848400008484
      0000848400008484840084840000F7FFFF00FFFFFF00FFFFFF00F7FFFF008484
      0000848484008484840084848400848484008484840084848400848484008484
      84008484840084848400C6C6C600F7FFFF00FFFFFF00C6C6C600008484000000
      840084848400C6C6C600F7FFFF00F7FFFF00F7FFFF00C6C6C600848484000000
      84000000840084840000F7FFFF0000000000FFFFFF00C6C6C600848484008484
      840084848400C6C6C600F7FFFF00FFFFFF00F7FFFF00C6C6C600848484000000
      00008484840084840000F7FFFF0000000000FFFFFF00C6DEC600848484008484
      0000848400008484000084840000848400008484000084840000848484008484
      8400C6DEC600C6DEC600F7FFFF00FFFFFF00FFFFFF00C6DEC600848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      0000C6C6C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600848484000000
      8400008484008484840084840000F7FFFF00C6C6C60084848400848484008484
      840084848400C6C6C600FFFFFF0000000000FFFFFF00C6DEC600848484008484
      8400848484008484840084840000F7FFFF00C6C6C60084848400848484008484
      840084848400C6C6C600FFFFFF0000000000F7FFFF00C6DEC600848484008484
      000084840000848400008484000084840000848400008484840084840000C6DE
      C600F7FFFF00F7FFFF00F7FFFF00FFFFFF00F7FFFF00C6C6C600848484008484
      8400848484008484840084848400848484008484840084848400C6C6C600F7FF
      FF00F7FFFF00F7FFFF00F7FFFF0000000000FFFFFF00F7FFFF00C6C6C6000084
      8400008484008484840084848400848484008484840084848400C6C6C600C6C6
      C600C6C6C600F7FFFF00FFFFFF0000000000F7FFFF00F7FFFF00C6C6C6008484
      8400848484008484840084848400848484008484840084848400C6DEC600C6DE
      C600C6C6C600F7FFFF00FFFFFF0000000000F7FFFF0084848400848400008484
      0000848484008484840084848400848484008484840084840000C6DEC600C6DE
      C6008484840084848400C6C6C600F7FFFF00F7FFFF0084840000848484008484
      8400848484008484000084848400848484008484000084848400C6C6C600C6C6
      C6008484840084848400C6C6C600F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6C6
      C6008484840084848400848484000084840000008400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6C6
      C6008484840084848400848484008484840084848400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000C6DEC60084848400848400008484
      000084848400C6DEC60084848400848484008484840084840000848400008484
      8400848400000084000084848400F7FFFF00F7FFFF0084848400848484008484
      840084840000C6DEC60084848400848484008484000084848400848400008484
      0000848484008484840084840000F7FFFF00FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00C6C6C60084848400848484000084840000008400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00C6C6C60084840000848484008484840084848400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF000000000000000000F7FFFF0084840000848484008484
      0000008400008484000084848400848484008484840084840000848400008484
      84008484000084848400C6DEC600F7FFFF00F7FFFF0084840000848484008484
      8400848484008484000084848400848484008484000084848400848400008484
      84008484840084840000C6DEC600FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00C6C6C600848400008484840084848400C6DEC600F7FFFF00F7FF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6C6C600848400008484840084848400C6DEC600F7FFFF00F7FF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00F7FFFF00848400008484
      8400848484008484840084840000008400008484840000840000848484008484
      840084840000C6DEC600F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      0000848400008484840084848400848484008484840084848400848484008484
      000084840000C6DEC600FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00C6DEC600C6C6C600C6C6C600F7FFFF00FFFFFF00FFFF
      FF00F7FFFF00F7FFFF00FFFFFF000000000000000000FFFFFF00F7FFFF00FFFF
      FF00FFFFFF00F7FFFF00C6DEC600C6C6C600C6DEC600F7FFFF00FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6DEC600C6DEC60084848400848484008484840084848400C6DEC600C6DE
      C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000F7FFFF00F7FF
      FF00C6DEC600C6C6C60084848400848484008484840084848400C6C6C600F7FF
      FF00F7FFFF0000000000FFFFFF00FFFFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000F7FFFF0000000000FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF0000000000FFFFFF0000000000F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6DEC60084840000C6DEC6008484840084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6DEC60084840000C6DEC6008484000084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00424D3E000000000000003E000000
      2800000040000000300000000100010000000000800100000000000000000000
      000000000000000000000000FFFFFF0000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000008000F803F03FF03F0000B005E01FE01F
      0000A000E00FE00F000040008001800100004000000000000000800000000000
      00000000000000000000000000000000000000018000800000000001C000C000
      00000003E001E00100000006F000F02000000007F800F80800000007FFC0FFC0
      00002007FFE0FFE000012003FFF0FFF0800180018000D0060001800100000000
      0001800100000000000100010000000000010001000000000001000100000000
      0001000100000000000100010000000000010001000000000001000100000001
      0001000100000000000100010000000000010003000000000001000100000000
      0001800100004004000140050000000000000000000000000000000000000000
      000000000000}
  end
  object DecontPopupMenu: TPopupMenu
    Left = 144
    Top = 240
    object Cmd_ReunesteDecont: TMenuItem
      Caption = 'Reunire Decont'
      OnClick = Cmd_ReunesteDecontClick
    end
  end
  object QryJustificariUpdate: TZQuery
    Connection = frmData.dbContabilitate
    Filtered = True
    SQL.Strings = (
      
        'UPDATE BREGISTRU SET NR_DECONT = :NEW_NR_DECONT, DATA_DECONT = :' +
        'NEW_DATA_DECONT, '
      
        'CODGEST = CASE WHEN PARENT_COD IS NULL THEN :NEW_CODGEST ELSE CO' +
        'DGEST END'
      'WHERE '
      '  NR_DECONT = :OLD_NR_DECONT '
      '  AND DATA_DECONT = :OLD_DATA_DECONT'
      
        '  AND (ISNULL(CODGEST,-1) = :OLD_CODGEST OR PARENT_COD IS NOT NU' +
        'LL)')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'NEW_NR_DECONT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'NEW_DATA_DECONT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'NEW_CODGEST'
        ParamType = ptUnknown
        Size = 50
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'OLD_NR_DECONT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'OLD_DATA_DECONT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'OLD_CODGEST'
        ParamType = ptUnknown
        Size = 50
      end>
    Left = 386
    Top = 240
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'NEW_NR_DECONT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'NEW_DATA_DECONT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'NEW_CODGEST'
        ParamType = ptUnknown
        Size = 50
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'OLD_NR_DECONT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'OLD_DATA_DECONT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'OLD_CODGEST'
        ParamType = ptUnknown
        Size = 50
      end>
  end
  object MemFact: TdxMemData
    Indexes = <>
    SortOptions = []
    SortedFields = 'ID_PARINTE'
    Left = 64
    Top = 288
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
    object cxStyle5: TcxStyle
      AssignedValues = [svColor, svTextColor]
      Color = clBtnFace
      TextColor = clBtnText
    end
    object cxStyle6: TcxStyle
      AssignedValues = [svColor, svFont, svTextColor]
      Color = clBtnFace
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clWindowText
    end
  end
end
