object frmImperechereTert: TfrmImperechereTert
  Left = 311
  Top = 77
  Caption = 'Analitic cont terti'
  ClientHeight = 507
  ClientWidth = 760
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object TreeCont: TcxDBTreeList
    Left = 485
    Top = 141
    Width = 281
    Height = 241
    Bands = <
      item
      end>
    DataController.DataSource = DTCont
    DataController.ParentField = 'parinte'
    DataController.KeyField = 'cont'
    DefaultRowHeight = 2
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsData.CancelOnExit = False
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.CellAutoHeight = True
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 0
    Visible = False
    OnDblClick = TreeContDblClick
    OnKeyDown = TreeContKeyDown
    object TreeContcont: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'cont'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeContdenumire: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Denumire'
      DataBinding.FieldName = 'denumire'
      Width = 283
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeContparinte: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'parinte'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeContfctcont: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'fctcont'
      Width = 44
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeContDescriere: TcxDBTreeListColumn
      Caption.Text = 'Selectie Cont'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = TreeContDescriereGetDisplayText
    end
  end
  object TreeTert: TcxDBTreeList
    Left = 502
    Top = 173
    Width = 233
    Height = 225
    Bands = <
      item
      end>
    DataController.DataSource = DTTert
    DataController.ParentField = 'id'
    DataController.KeyField = 'id'
    DefaultRowHeight = 2
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.CellAutoHeight = True
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 1
    Visible = False
    OnDblClick = TreeContDblClick
    OnKeyDown = TreeContKeyDown
    object TreeTertid: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'id'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeTertnume: TcxDBTreeListColumn
      DataBinding.FieldName = 'nume'
      Width = 217
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeTertcod_fiscal: TcxDBTreeListColumn
      Caption.Text = 'Cod Fiscal'
      DataBinding.FieldName = 'cod_fiscal'
      Width = 94
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxSplitter: TcxSplitter
    Left = 200
    Top = 0
    Width = 8
    Height = 507
    Cursor = crHSplit
    HotZoneClassName = 'TcxMediaPlayer9Style'
    AutoSnap = True
    MinSize = 1
    Control = pnLeft
    Color = 15788262
    ParentColor = False
  end
  object pnLeft: TPanel
    Left = 0
    Top = 0
    Width = 200
    Height = 507
    Align = alLeft
    BevelOuter = bvNone
    Color = 15788262
    TabOrder = 3
    DesignSize = (
      200
      507)
    object label1: TLabel
      Left = 1
      Top = 19
      Width = 31
      Height = 13
      Caption = 'Cont : '
    end
    object cxGridTert: TcxGrid
      Left = 0
      Top = 59
      Width = 200
      Height = 448
      Align = alBottom
      Anchors = [akLeft, akTop, akRight, akBottom]
      TabOrder = 0
      LookAndFeel.Kind = lfOffice11
      object GridTert: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        OnFocusedRecordChanged = GridTertFocusedRecordChanged
        DataController.DataSource = DTTert
        DataController.KeyFieldNames = 'Id'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsBehavior.IncSearch = True
        OptionsData.CancelOnExit = False
        OptionsData.Deleting = False
        OptionsData.DeletingConfirmation = False
        OptionsData.Editing = False
        OptionsData.Inserting = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        object GridTertId: TcxGridDBColumn
          DataBinding.FieldName = 'Id'
          Visible = False
        end
        object GridTertNume: TcxGridDBColumn
          DataBinding.FieldName = 'NumeTert'
          Width = 111
        end
        object GridTertCodFiscal: TcxGridDBColumn
          DataBinding.FieldName = 'CodFiscal'
          Visible = False
          SortIndex = 0
          SortOrder = soAscending
          Width = 102
        end
        object GridTertSuma: TcxGridDBColumn
          DataBinding.FieldName = 'Suma'
          Width = 71
        end
      end
      object cxGridTertLevel: TcxGridLevel
        GridView = GridTert
      end
    end
    object edPopCont: TcxPopupEdit
      Left = 32
      Top = 13
      Anchors = [akLeft, akTop, akRight]
      Properties.PopupControl = TreeCont
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = edPopContPropertiesCloseUp
      Properties.OnPopup = edPopContPropertiesPopup
      TabOrder = 1
      Width = 142
    end
    object btnSetupConturi: TcxButton
      Left = 176
      Top = 13
      Width = 23
      Height = 20
      Hint = 'Setare conturi lichidare'
      Anchors = [akTop, akRight]
      Caption = '...'
      ParentShowHint = False
      ShowHint = True
      TabOrder = 2
      OnClick = btnSetupConturiClick
    end
    object chkFilter: TcxCheckBox
      Left = 8
      Top = 41
      Caption = 'Numai cei cu Sold Restant'
      Properties.OnChange = chkFilterPropertiesChange
      TabOrder = 3
    end
  end
  object pnAll: TPanel
    Left = 208
    Top = 0
    Width = 552
    Height = 507
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 4
    object pnTop: TPanel
      Left = 0
      Top = 0
      Width = 552
      Height = 147
      Align = alTop
      BevelOuter = bvNone
      Color = clWhite
      Constraints.MinHeight = 30
      TabOrder = 0
      DesignSize = (
        552
        147)
      object Label2: TLabel
        Left = 6
        Top = 7
        Width = 76
        Height = 13
        Caption = 'Detalii Tert : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clTeal
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 12
        Top = 22
        Width = 540
        Height = 7
        Anchors = [akLeft, akTop, akRight]
        Shape = bsBottomLine
      end
      object Label5: TLabel
        Left = 7
        Top = 38
        Width = 56
        Height = 13
        Caption = 'Nume Tert: '
      end
      object Label6: TLabel
        Left = 7
        Top = 59
        Width = 43
        Height = 13
        Caption = 'Tip Tert: '
      end
      object Label7: TLabel
        Left = 7
        Top = 80
        Width = 42
        Height = 13
        Caption = 'Adresa : '
      end
      object Label8: TLabel
        Left = 262
        Top = 36
        Width = 43
        Height = 13
        Caption = 'Contact: '
      end
      object Label9: TLabel
        Left = 262
        Top = 59
        Width = 21
        Height = 13
        Caption = 'Tel: '
      end
      object Label10: TLabel
        Left = 262
        Top = 82
        Width = 23
        Height = 13
        Caption = 'Fax :'
      end
      object Label11: TLabel
        Left = 262
        Top = 105
        Width = 31
        Height = 13
        Caption = 'Email :'
      end
      object Label12: TLabel
        Left = 262
        Top = 128
        Width = 35
        Height = 13
        Caption = 'Grupa :'
      end
      object edNumeRepartitor: TcxDBLabel
        Left = 83
        Top = 6
        AutoSize = True
        DataBinding.DataField = 'Nume'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
      end
      object btnEditRepartitor: TcxButton
        Left = 330
        Top = 1
        Width = 105
        Height = 22
        Anchors = [akTop, akRight]
        Caption = 'Editare tert ...'
        LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        OnClick = btnEditRepartitorClick
      end
      object cxDBLabel1: TcxDBLabel
        Left = 70
        Top = 34
        DataBinding.DataField = 'Nume'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 170
      end
      object cxDBLabel2: TcxDBLabel
        Left = 70
        Top = 57
        DataBinding.DataField = 'TipTert'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 170
      end
      object cxDBLabel3: TcxDBLabel
        Left = 70
        Top = 80
        DataBinding.DataField = 'ADRESA'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Properties.WordWrap = True
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 61
        Width = 170
      end
      object cxDBLabel4: TcxDBLabel
        Left = 318
        Top = 34
        DataBinding.DataField = 'PERSOANA_CONTACT'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 173
      end
      object cxDBLabel5: TcxDBLabel
        Left = 318
        Top = 57
        DataBinding.DataField = 'TELEFON'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 173
      end
      object cxDBLabel6: TcxDBLabel
        Left = 318
        Top = 80
        DataBinding.DataField = 'FAX'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 173
      end
      object cxDBLabel7: TcxDBLabel
        Left = 318
        Top = 103
        DataBinding.DataField = 'EMAIL'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 173
      end
      object cxDBLabel8: TcxDBLabel
        Left = 318
        Top = 126
        DataBinding.DataField = 'DenGrupa'
        DataBinding.DataSource = DTTert
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        Height = 17
        Width = 173
      end
    end
    object cxSplitterTop: TcxSplitter
      Left = 0
      Top = 147
      Width = 552
      Height = 8
      HotZoneClassName = 'TcxMediaPlayer9Style'
      AlignSplitter = salTop
      Control = pnTop
      ExplicitWidth = 8
    end
    object pnInfo: TPanel
      Left = 0
      Top = 155
      Width = 552
      Height = 352
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 2
      object cxGridInfo: TcxGrid
        Left = 0
        Top = 32
        Width = 552
        Height = 320
        Align = alClient
        TabOrder = 0
        LevelTabs.Slants.Kind = skCutCorner
        LookAndFeel.Kind = lfOffice11
        object GridInfo: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          DataController.DataSource = DTDateTert
          DataController.KeyFieldNames = 'id'
          DataController.Summary.DefaultGroupSummaryItems = <
            item
              Kind = skSum
              Position = spFooter
              Column = GridInfoobligatie
            end
            item
              Kind = skSum
              Position = spFooter
              Column = GridInfostingere
            end
            item
              Kind = skSum
              Position = spFooter
              Column = GridInforamasObligatie
            end>
          DataController.Summary.FooterSummaryItems = <
            item
              Format = ',0.00'
              Kind = skSum
              Column = GridInforamasObligatie
            end
            item
              Format = ',0.00'
              Kind = skSum
              Column = GridInfoobligatie
            end
            item
              Format = ',0.00'
              Kind = skSum
              Column = GridInfostingere
            end>
          DataController.Summary.SummaryGroups = <>
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsView.GridLineColor = clSilver
          OptionsView.GridLines = glVertical
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfAlwaysVisible
          Styles.StyleSheet = frmData.GridTableViewStyleSheetCustom
          object GridInfoid: TcxGridDBColumn
            DataBinding.FieldName = 'id'
            Visible = False
          end
          object GridInfodata: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'data'
            SortIndex = 0
            SortOrder = soDescending
            Width = 60
          end
          object GridInfocont: TcxGridDBColumn
            Caption = 'Cont'
            DataBinding.FieldName = 'cont'
            Width = 86
          end
          object GridInfoNumeTert: TcxGridDBColumn
            DataBinding.FieldName = 'NumeRepartitor'
            Visible = False
            Width = 135
          end
          object GridInfoDocument: TcxGridDBColumn
            DataBinding.FieldName = 'Document'
            Width = 110
          end
          object GridInfoTotalDocStingere: TcxGridDBColumn
            Caption = 'Stins'
            DataBinding.FieldName = 'TotalDocStingere'
            Width = 145
          end
          object GridInfocodRep: TcxGridDBColumn
            DataBinding.FieldName = 'codRep'
            Visible = False
          end
          object GridInfoeste_plata: TcxGridDBColumn
            DataBinding.FieldName = 'este_plata'
            Visible = False
            Width = 75
          end
          object GridInfoSemn: TcxGridDBColumn
            DataBinding.FieldName = 'Semn'
            Visible = False
            Width = 32
          end
          object GridInfoModul: TcxGridDBColumn
            DataBinding.FieldName = 'Modul'
            PropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.Items = <>
            Visible = False
          end
          object GridInfoid_unic_modul: TcxGridDBColumn
            DataBinding.FieldName = 'id_unic_modul'
            Visible = False
          end
          object GridInfoid_document_modul: TcxGridDBColumn
            DataBinding.FieldName = 'id_document_modul'
            Visible = False
          end
          object GridInfodocStingere: TcxGridDBColumn
            DataBinding.FieldName = 'docStingere'
            Visible = False
          end
          object GridInfoplati: TcxGridDBColumn
            DataBinding.FieldName = 'plati'
            Visible = False
          end
          object GridInfoexplicatie: TcxGridDBColumn
            Caption = 'Explicatie Pozitie document'
            DataBinding.FieldName = 'explicatie'
            Visible = False
            Width = 331
          end
          object GridInfoobligatie: TcxGridDBColumn
            Caption = 'Obligatie Inscrisa'
            DataBinding.FieldName = 'TotalDocument'
            Width = 104
          end
          object GridInfostingere: TcxGridDBColumn
            Caption = 'Stingere'
            DataBinding.FieldName = 'TotalStingere'
            Width = 63
          end
          object GridInforamasObligatie: TcxGridDBColumn
            Caption = 'Ramas de stins'
            DataBinding.FieldName = 'TotalramasDocument'
            Width = 110
          end
          object GridInforamasPlata: TcxGridDBColumn
            DataBinding.FieldName = 'ramasPlata'
            Visible = False
            Width = 57
          end
          object GridInfoCodFiscal: TcxGridDBColumn
            DataBinding.FieldName = 'CodFiscal'
            Visible = False
            Width = 51
          end
          object GridInfocontCSP: TcxGridDBColumn
            DataBinding.FieldName = 'contCSP'
            Visible = False
          end
          object GridInforepCSP: TcxGridDBColumn
            DataBinding.FieldName = 'repCSP'
            Visible = False
          end
          object GridInfoNrDocument: TcxGridDBColumn
            DataBinding.FieldName = 'NrDocument'
            Visible = False
          end
          object GridInfoTotalramasObligatie: TcxGridDBColumn
            DataBinding.FieldName = 'TotalramasObligatie'
            Visible = False
          end
          object GridInfoTotalRamasPlata: TcxGridDBColumn
            DataBinding.FieldName = 'TotalRamasPlata'
            Visible = False
          end
          object GridInfoTotalDocument: TcxGridDBColumn
            DataBinding.FieldName = 'TotalDocument'
            Visible = False
          end
          object GridInfoTotalStingere: TcxGridDBColumn
            DataBinding.FieldName = 'TotalStingere'
            Visible = False
          end
          object GridInfoTotalramasDocument: TcxGridDBColumn
            DataBinding.FieldName = 'TotalramasDocument'
            Visible = False
          end
          object GridInfoNumeRepartitor: TcxGridDBColumn
            DataBinding.FieldName = 'NumeRepartitor'
            Visible = False
          end
          object GridInfoDenCont: TcxGridDBColumn
            DataBinding.FieldName = 'DenCont'
            Visible = False
          end
          object GridInfodataScadenta: TcxGridDBColumn
            Caption = 'Data Scadenta'
            DataBinding.FieldName = 'dataScadenta'
            Width = 91
          end
        end
        object GridInfo2: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          DataController.DataSource = DTDateTert2
          DataController.DetailKeyFieldNames = 'Document'
          DataController.KeyFieldNames = 'id'
          DataController.MasterKeyFieldNames = 'Document'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsView.GridLineColor = clSilver
          OptionsView.GridLines = glVertical
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfAlwaysVisible
          Styles.StyleSheet = frmData.GridTableViewStyleSheetCustom
          object GridInfo2data: TcxGridDBColumn
            DataBinding.FieldName = 'data'
            SortIndex = 0
            SortOrder = soAscending
            Width = 64
          end
          object GridInfo2cont: TcxGridDBColumn
            DataBinding.FieldName = 'cont'
            Width = 64
          end
          object GridInfo2Document: TcxGridDBColumn
            DataBinding.FieldName = 'Document'
            Width = 73
          end
          object GridInfo2explicatie: TcxGridDBColumn
            DataBinding.FieldName = 'explicatie'
            Width = 64
          end
          object GridInfo2dataScadenta: TcxGridDBColumn
            DataBinding.FieldName = 'dataScadenta'
            Width = 77
          end
          object GridInfo2codRep: TcxGridDBColumn
            DataBinding.FieldName = 'codRep'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2este_plata: TcxGridDBColumn
            DataBinding.FieldName = 'este_plata'
            Visible = False
            Width = 64
          end
          object GridInfo2Semn: TcxGridDBColumn
            DataBinding.FieldName = 'Semn'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2id: TcxGridDBColumn
            DataBinding.FieldName = 'id'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2Modul: TcxGridDBColumn
            DataBinding.FieldName = 'Modul'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2id_unic_modul: TcxGridDBColumn
            DataBinding.FieldName = 'id_unic_modul'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2id_document_modul: TcxGridDBColumn
            DataBinding.FieldName = 'id_document_modul'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2obligatie: TcxGridDBColumn
            DataBinding.FieldName = 'obligatie'
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 75
          end
          object GridInfo2docStingere: TcxGridDBColumn
            DataBinding.FieldName = 'docStingere'
            Width = 64
          end
          object GridInfo2stingere: TcxGridDBColumn
            DataBinding.FieldName = 'stingere'
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2ramasObligatie: TcxGridDBColumn
            DataBinding.FieldName = 'ramasObligatie'
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2plati: TcxGridDBColumn
            DataBinding.FieldName = 'plati'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2ramasPlata: TcxGridDBColumn
            DataBinding.FieldName = 'ramasPlata'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2contCSP: TcxGridDBColumn
            DataBinding.FieldName = 'contCSP'
            Visible = False
            Width = 64
          end
          object GridInfo2repCSP: TcxGridDBColumn
            DataBinding.FieldName = 'repCSP'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2NrDocument: TcxGridDBColumn
            DataBinding.FieldName = 'NrDocument'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
          end
          object GridInfo2TotalramasObligatie: TcxGridDBColumn
            DataBinding.FieldName = 'TotalramasObligatie'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2TotalRamasPlata: TcxGridDBColumn
            DataBinding.FieldName = 'TotalRamasPlata'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2TotalDocStingere: TcxGridDBColumn
            DataBinding.FieldName = 'TotalDocStingere'
            Visible = False
            Width = 64
          end
          object GridInfo2TotalDocument: TcxGridDBColumn
            DataBinding.FieldName = 'TotalDocument'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2TotalStingere: TcxGridDBColumn
            DataBinding.FieldName = 'TotalStingere'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2TotalramasDocument: TcxGridDBColumn
            DataBinding.FieldName = 'TotalramasDocument'
            Visible = False
            FooterAlignmentHorz = taRightJustify
            GroupSummaryAlignment = taRightJustify
            Width = 64
          end
          object GridInfo2NumeRepartitor: TcxGridDBColumn
            DataBinding.FieldName = 'NumeRepartitor'
            Visible = False
            Width = 64
          end
          object GridInfo2DenCont: TcxGridDBColumn
            DataBinding.FieldName = 'DenCont'
            Visible = False
            Width = 64
          end
          object GridInfo2DenModul: TcxGridDBColumn
            DataBinding.FieldName = 'DenModul'
            Visible = False
            Width = 64
          end
        end
        object GridInfoLevel: TcxGridLevel
          GridView = GridInfo
          object GridInfoLevel2: TcxGridLevel
            GridView = GridInfo2
          end
        end
      end
      object pnControlBar: TPanel
        Left = 0
        Top = 0
        Width = 552
        Height = 32
        Align = alTop
        BevelOuter = bvLowered
        Color = 15658734
        Ctl3D = True
        ParentCtl3D = False
        TabOrder = 1
        object Label3: TLabel
          Left = 10
          Top = 10
          Width = 34
          Height = 13
          Caption = 'Arata : '
        end
        object Label4: TLabel
          Left = 234
          Top = 8
          Width = 51
          Height = 13
          Caption = 'Detaliere : '
        end
        object edtInfoMod: TcxImageComboBox
          Left = 42
          Top = 6
          EditValue = 0
          ParentFont = False
          Properties.Items = <
            item
              Description = 'Toata relatia cu tertul'
              ImageIndex = 0
              Value = 0
            end
            item
              Description = 'Obligatile stinse'
              Value = 1
            end
            item
              Description = 'Obligatile ramase nestinse'
              Value = 2
            end
            item
              Description = 'Plati/Incasari ramase nefolosite'
              Value = 3
            end
            item
              Description = 'Toate obligatile indiferent daca sunt stinse sau nu'
              Value = 4
            end
            item
              Description = 'Toate platile/incasarile indiferent daca sunt stinse sau nu'
              Value = 5
            end>
          Properties.OnChange = edtInfoModPropertiesChange
          Style.Font.Charset = DEFAULT_CHARSET
          Style.Font.Color = clWindowText
          Style.Font.Height = -11
          Style.Font.Name = 'MS Sans Serif'
          Style.Font.Style = [fsBold]
          Style.IsFontAssigned = True
          TabOrder = 0
          Width = 186
        end
        object edtInfoDetaliere: TcxImageComboBox
          Left = 290
          Top = 5
          EditValue = 1
          ParentFont = False
          Properties.Items = <
            item
              Description = 'la nivel de document'
              ImageIndex = 0
              Value = 0
            end
            item
              Description = 'la nivel de pozitie document'
              Value = 1
            end>
          Properties.OnChange = edtInfoDetalierePropertiesChange
          Style.Font.Charset = DEFAULT_CHARSET
          Style.Font.Color = clWindowText
          Style.Font.Height = -11
          Style.Font.Name = 'MS Sans Serif'
          Style.Font.Style = [fsBold]
          Style.IsFontAssigned = True
          TabOrder = 1
          Width = 183
        end
      end
    end
  end
  object DTCont: TDataSource
    DataSet = qryCont
    Left = 4
    Top = 247
  end
  object qryCont: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spTertGetConturi')
    Params = <>
    Left = 37
    Top = 246
  end
  object DTTert: TDataSource
    DataSet = qryTert
    Left = 6
    Top = 278
  end
  object qryTert: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spTertGetTertiCont :Cont')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 200
      end>
    Left = 38
    Top = 278
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 200
      end>
  end
  object DTDateTert: TDataSource
    DataSet = qryDateTert
    Left = 6
    Top = 325
  end
  object qryDateTert: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spTertDocumenteNeplatite :Cont, :CodRep, 0, :tipDate')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CodRep'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'tipDate'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 38
    Top = 326
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CodRep'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'tipDate'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = cxGridInfo
    PopupMenus = <>
    Left = 373
    Top = 259
  end
  object DTDateTert2: TDataSource
    DataSet = qryDateTert2
    Left = 5
    Top = 358
  end
  object qryDateTert2: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spTertDocumenteNeplatite :Cont, :CodRep, 1, :tipDate')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CodRep'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'tipDate'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 37
    Top = 358
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'Cont'
        ParamType = ptUnknown
        Size = 100
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CodRep'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'tipDate'
        ParamType = ptUnknown
        Size = 4
      end>
  end
end
