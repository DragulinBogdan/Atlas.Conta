object frmUrmarireExecutie: TfrmUrmarireExecutie
  Left = 332
  Top = 201
  Caption = 'Urmarire Executie Bugetara'
  ClientHeight = 608
  ClientWidth = 1061
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
  object pnTools: TPanel
    Left = 0
    Top = 0
    Width = 1061
    Height = 62
    Align = alTop
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 0
    object LbTipClasificatie: TLabel
      Left = 10
      Top = 12
      Width = 76
      Height = 13
      Caption = 'Tip clasificatie : '
    end
    object LbDataRaportare: TLabel
      Left = 256
      Top = 12
      Width = 92
      Height = 13
      Caption = 'Data de raportare : '
    end
    object LbClasificatie: TLabel
      Left = 8
      Top = 39
      Width = 114
      Height = 13
      Caption = 'Clasa curent selectata : '
    end
    object lbRevizie: TLabel
      Left = 311
      Top = 39
      Width = 35
      Height = 13
      Caption = 'Revizie'
    end
    object lbGestiune: TLabel
      Left = 520
      Top = 12
      Width = 51
      Height = 13
      Caption = 'Gestiune : '
    end
    object edDataRaportare: TcxDateEdit
      Left = 356
      Top = 8
      Properties.OnChange = edDataRaportarePropertiesChange
      TabOrder = 0
      Width = 121
    end
    object edTipClasificatie: TcxImageComboBox
      Left = 88
      Top = 8
      EditValue = '0'
      Properties.Items = <
        item
          Description = 'Pe Functional'
          ImageIndex = 0
          Value = 0
        end
        item
          Description = 'Pe Economic'
          Value = 1
        end>
      Properties.OnChange = edTipClasificatiePropertiesChange
      TabOrder = 1
      Width = 161
    end
    object edClasificatie: TcxImageComboBox
      Left = 128
      Top = 35
      Properties.Items = <>
      Properties.OnChange = edTipClasificatiePropertiesChange
      TabOrder = 2
      Width = 177
    end
    object edRevizie: TcxImageComboBox
      Left = 372
      Top = 35
      Properties.Items = <>
      Properties.OnChange = edReviziePropertiesChange
      TabOrder = 3
      Width = 105
    end
    object edGestiune: TcxImageComboBox
      Left = 576
      Top = 8
      Properties.Items = <>
      Properties.OnChange = edGestiunePropertiesChange
      TabOrder = 4
      Width = 185
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 567
    Width = 1061
    Height = 41
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 1
    object ChkArataFaraPlanificare: TcxCheckBox
      Left = 8
      Top = 11
      Caption = 'Arata pozitiile fara planificare'
      State = cbsChecked
      TabOrder = 0
      OnClick = ChkArataFaraPlanificareClick
    end
  end
  object pnClient: TPanel
    Left = 0
    Top = 62
    Width = 1061
    Height = 505
    Align = alClient
    BevelInner = bvLowered
    BevelOuter = bvNone
    TabOrder = 2
    object splitterV: TcxSplitter
      Left = 779
      Top = 1
      Width = 8
      Height = 503
      AlignSplitter = salRight
    end
    object pnBugetare: TPanel
      Left = 1
      Top = 1
      Width = 778
      Height = 503
      Align = alClient
      BevelInner = bvRaised
      BevelOuter = bvNone
      TabOrder = 0
      object splitterH: TcxSplitter
        Left = 1
        Top = 233
        Width = 776
        Height = 8
        Cursor = crVSplit
        AlignSplitter = salTop
        Control = BugetMaster
      end
      object BugetMaster: TPanel
        Left = 1
        Top = 1
        Width = 776
        Height = 232
        Align = alTop
        BevelOuter = bvLowered
        TabOrder = 0
        object TreeMaster: TcxDBTreeList
          Left = 1
          Top = 1
          Width = 774
          Height = 230
          Align = alClient
          Bands = <
            item
            end>
          DataController.DataSource = DTMaster
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID'
          Navigator.Buttons.CustomButtons = <>
          OptionsCustomizing.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Deleting = False
          OptionsView.ColumnAutoWidth = True
          RootValue = -1
          TabOrder = 0
          OnFocusedNodeChanged = TreeMasterFocusedNodeChanged
          object TreeMasterCAPTURA: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 200
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterDENUMIRE: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 300
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterASIGNAT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Asignat'
            DataBinding.FieldName = 'ASIGNAT'
            Width = 100
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterCOD_ECONOMIC: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa Economica'
            DataBinding.FieldName = 'COD_ECONOMIC'
            Width = 100
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterCOD_FUNCTIONAL: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa Functionala'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            Width = 100
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterPLANIFICAT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Buget'
            DataBinding.FieldName = 'PLANIFICAT'
            Width = 100
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterANGAJAT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Angajat'
            DataBinding.FieldName = 'ANGAJAT'
            Width = 100
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterCONSUM: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Consum'
            DataBinding.FieldName = 'CONSUM'
            Width = 100
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterPLATIT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Platit'
            DataBinding.FieldName = 'PLATIT'
            Width = 100
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterDISPONIBIL: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Disponibil'
            DataBinding.FieldName = 'DISPONIBIL'
            Width = 100
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterPROC_ANGAJAT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_ANGAJAT'
            Width = 100
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterPROC_PLATIT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_PLATIT'
            Width = 100
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterPROC_CONSUM: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_CONSUM'
            Width = 100
            Position.ColIndex = 12
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeMasterRAMAS_DE_REALIZAT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ramas'
            DataBinding.FieldName = 'RAMAS_DE_REALIZAT'
            Width = 100
            Position.ColIndex = 13
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
      object pnBugetChild: TPanel
        Left = 1
        Top = 241
        Width = 776
        Height = 261
        Align = alClient
        BevelOuter = bvLowered
        TabOrder = 1
        object TreeChild: TcxDBTreeList
          Left = 1
          Top = 1
          Width = 774
          Height = 259
          Align = alClient
          Bands = <
            item
            end>
          DataController.DataSource = DTChild
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID'
          Navigator.Buttons.CustomButtons = <>
          OptionsCustomizing.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Deleting = False
          OptionsView.ColumnAutoWidth = True
          PopupMenu = ppFisaBugetara
          RootValue = -1
          TabOrder = 0
          object TreeChildCAPTURA: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 194
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildDENUMIRE: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 472
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildASIGNAT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Asignat'
            DataBinding.FieldName = 'ASIGNAT'
            Width = 65
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildCOD_ECONOMIC: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'COD_ECONOMIC'
            Width = 145
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildCOD_FUNCTIONAL: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            Width = 162
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildPLANIFICAT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Buget'
            DataBinding.FieldName = 'PLANIFICAT'
            Width = 99
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildANGAJAT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Angajat'
            DataBinding.FieldName = 'ANGAJAT'
            Width = 120
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildCONSUM: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Consum'
            DataBinding.FieldName = 'CONSUM'
            Width = 126
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildPLATIT: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Platit'
            DataBinding.FieldName = 'PLATIT'
            Width = 105
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildDISPONIBIL: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Disponibil'
            DataBinding.FieldName = 'DISPONIBIL'
            Width = 128
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildPROC_ANGAJAT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_ANGAJAT'
            Width = 143
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildPROC_PLATIT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_PLATIT'
            Width = 125
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildPROC_CONSUM: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'PROC_CONSUM'
            Width = 140
            Position.ColIndex = 12
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object TreeChildRAMAS_DE_REALIZAT: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            DataBinding.FieldName = 'RAMAS_DE_REALIZAT'
            Width = 194
            Position.ColIndex = 13
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
    object vBuget: TcxDBVerticalGrid
      Left = 787
      Top = 1
      Width = 273
      Height = 503
      Align = alRight
      LookAndFeel.Kind = lfFlat
      OptionsView.CellTextMaxLineCount = 3
      OptionsView.AutoScaleBands = False
      OptionsView.GridLineColor = clBtnFace
      OptionsView.RowHeaderMinWidth = 30
      OptionsView.RowHeaderWidth = 136
      OptionsView.ValueWidth = 64
      Navigator.Buttons.CustomButtons = <>
      TabOrder = 2
      DataController.DataSource = DTChild
      Version = 1
      object vBugetCOD_ECONOMIC: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Clasa Economica'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'COD_ECONOMIC'
        ID = 0
        ParentID = -1
        Index = 0
        Version = 1
      end
      object vBugetCOD_FUNCTIONAL: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Clasa Functionala'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'COD_FUNCTIONAL'
        ID = 1
        ParentID = -1
        Index = 1
        Version = 1
      end
      object vBugetDENUMIRE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Denumire Indicator'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'DENUMIRE'
        ID = 2
        ParentID = -1
        Index = 2
        Version = 1
      end
      object vBugetAN_FISCAL: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'An Fiscal'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'AN_FISCAL'
        ID = 3
        ParentID = -1
        Index = 3
        Version = 1
      end
      object vBugetCategoryRow1: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Angajat %'
        ID = 4
        ParentID = -1
        Index = 4
        Version = 1
      end
      object vBugetDBEditorRow1: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat % Trim. I'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 5
        ParentID = -1
        Index = 5
        Version = 1
      end
      object vBugetDBEditorRow2: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat % Trim. II'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 6
        ParentID = -1
        Index = 6
        Version = 1
      end
      object vBugetDBEditorRow3: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat % Trim. III'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 7
        ParentID = -1
        Index = 7
        Version = 1
      end
      object vBugetDBEditorRow4: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat % Trim. IV'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 8
        ParentID = -1
        Index = 8
        Version = 1
      end
      object vBugetCategoryRow2: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Realizat %'
        ID = 9
        ParentID = -1
        Index = 9
        Version = 1
      end
      object vBugetDBEditorRow5: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat % Trim. I'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 10
        ParentID = -1
        Index = 10
        Version = 1
      end
      object vBugetDBEditorRow6: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat % Trim. II'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 11
        ParentID = -1
        Index = 11
        Version = 1
      end
      object vBugetDBEditorRow7: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat % Trim. III'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 12
        ParentID = -1
        Index = 12
        Version = 1
      end
      object vBugetDBEditorRow8: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat % Trim. IV'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 13
        ParentID = -1
        Index = 13
        Version = 1
      end
      object vBugetDBEditorRow9: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat An Fiscal'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 14
        ParentID = -1
        Index = 14
        Version = 1
      end
      object vBugetDBEditorRow10: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat An Fiscal'
        Properties.EditPropertiesClassName = 'TcxTextEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        ID = 15
        ParentID = -1
        Index = 15
        Version = 1
      end
      object vBugetPLANIFICAT: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat An'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.UseThousandSeparator = True
        Properties.DataBinding.FieldName = 'PLANIFICAT'
        ID = 16
        ParentID = -1
        Index = 16
        Version = 1
      end
      object vBugetANGAJAT: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat An'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.UseThousandSeparator = True
        Properties.DataBinding.FieldName = 'ANGAJAT'
        ID = 17
        ParentID = -1
        Index = 17
        Version = 1
      end
      object vBugetANGAJAT1: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat Trim. I'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ANGAJAT1'
        ID = 18
        ParentID = -1
        Index = 18
        Version = 1
      end
      object vBugetPLANIFICAT1: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. I'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT1'
        ID = 19
        ParentID = -1
        Index = 19
        Version = 1
      end
      object vBugetPLANIFICAT2: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. II'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT2'
        ID = 20
        ParentID = -1
        Index = 20
        Version = 1
      end
      object vBugetANGAJAT2: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat Trim. II'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ANGAJAT2'
        ID = 21
        ParentID = -1
        Index = 21
        Version = 1
      end
      object vBugetANGAJAT3: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat Trim. III'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ANGAJAT3'
        ID = 22
        ParentID = -1
        Index = 22
        Version = 1
      end
      object vBugetPLANIFICAT3: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. III'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT3'
        ID = 23
        ParentID = -1
        Index = 23
        Version = 1
      end
      object vBugetPLANIFICAT4: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. IV'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT4'
        ID = 24
        ParentID = -1
        Index = 24
        Version = 1
      end
      object vBugetANGAJAT4: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Angajat Trim. IV'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ANGAJAT4'
        ID = 25
        ParentID = -1
        Index = 25
        Version = 1
      end
      object vBugetPLATIT: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat An'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATIT'
        ID = 26
        ParentID = -1
        Index = 26
        Version = 1
      end
      object vBugetPLANIFICAT5: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat An'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT'
        ID = 27
        ParentID = -1
        Index = 27
        Version = 1
      end
      object vBugetPLATIT1: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat Trim. I'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATIT1'
        ID = 28
        ParentID = -1
        Index = 28
        Version = 1
      end
      object vBugetPLANIFICAT11: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. I'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT1'
        ID = 29
        ParentID = -1
        Index = 29
        Version = 1
      end
      object vBugetPLATIT2: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat Trim. II'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATIT2'
        ID = 30
        ParentID = -1
        Index = 30
        Version = 1
      end
      object vBugetPLANIFICAT21: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. II'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT2'
        ID = 31
        ParentID = -1
        Index = 31
        Version = 1
      end
      object vBugetPLATIT3: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat Trim. III'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATIT3'
        ID = 32
        ParentID = -1
        Index = 32
        Version = 1
      end
      object vBugetPLANIFICAT31: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. III'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT3'
        ID = 33
        ParentID = -1
        Index = 33
        Version = 1
      end
      object vBugetPLATIT4: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Realizat Trim. IV'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATIT4'
        ID = 34
        ParentID = -1
        Index = 34
        Version = 1
      end
      object vBugetPLANIFICAT41: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Planificat Trim. IV'
        Properties.EditPropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.EditProperties.Alignment.Horz = taRightJustify
        Properties.EditProperties.AssignedValues.MaxValue = True
        Properties.EditProperties.AssignedValues.MinValue = True
        Properties.EditProperties.DecimalPlaces = 2
        Properties.EditProperties.DisplayFormat = ',0;-,0'
        Properties.EditProperties.Nullable = False
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLANIFICAT4'
        ID = 35
        ParentID = -1
        Index = 35
        Version = 1
      end
    end
  end
  object DTMaster: TDataSource
    DataSet = QryMaster
    Left = 120
    Top = 144
  end
  object DTChild: TDataSource
    DataSet = QryChild
    Left = 120
    Top = 192
  end
  object QryMaster: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_BUGET_SITUATIE_EXECUTIE :COD_BUGET, '#39#39', :DATA, :FUNCTION' +
        'AL, :DIVIZOR, :REVIZIE')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'FUNCTIONAL'
        ParamType = ptUnknown
        Size = 2
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DIVIZOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'REVIZIE'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 184
    Top = 144
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'FUNCTIONAL'
        ParamType = ptUnknown
        Size = 2
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DIVIZOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'REVIZIE'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object QryChild: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_BUGET_SITUATIE_EXECUTIE :COD_BUGET, :RADACINA, :DATA, :F' +
        'UNCTIONAL, :DIVIZOR, :REVIZIE')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'RADACINA'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'FUNCTIONAL'
        ParamType = ptUnknown
        Size = 2
        Value = True
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DIVIZOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'REVIZIE'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 184
    Top = 192
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'RADACINA'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'FUNCTIONAL'
        ParamType = ptUnknown
        Size = 2
        Value = True
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DIVIZOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'REVIZIE'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ppFisaBugetara: TPopupMenu
    Left = 258
    Top = 144
    object ppFisaBugetaraLaZi: TMenuItem
      Caption = 'Fisa Bugetara La Zi'
      OnClick = ppFisaBugetaraLaZiClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object ppFisaBugetaraTrim1: TMenuItem
      Caption = 'Fisa Bugetara Trim. 1'
      OnClick = ppFisaBugetaraTrim1Click
    end
    object ppFisaBugetaraTrim2: TMenuItem
      Caption = 'Fisa Bugetara Trim. 2'
      OnClick = ppFisaBugetaraTrim2Click
    end
    object ppFisaBugetaraTrim3: TMenuItem
      Caption = 'Fisa Bugetara Trim. 3'
      OnClick = ppFisaBugetaraTrim3Click
    end
    object ppFisaBugetaraTrim4: TMenuItem
      Caption = 'Fisa Bugetara Trim. 4'
      OnClick = ppFisaBugetaraTrim4Click
    end
  end
end
