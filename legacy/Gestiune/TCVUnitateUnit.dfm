object frmSituatieUnitate: TfrmSituatieUnitate
  Left = 318
  Top = 211
  Caption = 'Situatia stocurilor pe unitate'
  ClientHeight = 679
  ClientWidth = 944
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
  PixelsPerInch = 96
  TextHeight = 13
  object pageStocuri: TcxPageControl
    Left = 0
    Top = 0
    Width = 944
    Height = 584
    Align = alClient
    TabOrder = 0
    Properties.ActivePage = tabStocuriClasic
    Properties.CustomButtons.Buttons = <>
    OnChange = pageStocuriChange
    ExplicitHeight = 638
    ClientRectBottom = 584
    ClientRectRight = 944
    ClientRectTop = 24
    object tabStocuriClasic: TcxTabSheet
      Caption = 'Stocuri Unitate'
      ImageIndex = 0
      ExplicitHeight = 614
      object Panel2: TPanel
        Left = 0
        Top = 0
        Width = 944
        Height = 560
        Align = alClient
        Caption = 'Panel2'
        TabOrder = 0
        ExplicitHeight = 614
        object splitterH: TSplitter
          Left = 1
          Top = 357
          Width = 942
          Height = 3
          Cursor = crVSplit
          Align = alBottom
          ExplicitTop = 411
        end
        object splitterV: TSplitter
          Left = 257
          Top = 1
          Height = 356
          ExplicitHeight = 410
        end
        object pnDetaliuMaterial: TPanel
          Left = 1
          Top = 360
          Width = 942
          Height = 199
          Align = alBottom
          TabOrder = 0
          ExplicitTop = 414
          object GridIstoricMaterial: TdxDBGrid
            Left = 1
            Top = 1
            Width = 940
            Height = 197
            SearchType = stStart
            Bands = <
              item
              end>
            DefaultLayout = True
            HeaderPanelRowCount = 1
            KeyField = 'ID'
            ShowSummaryFooter = True
            SummaryGroups = <>
            SummarySeparator = ', '
            Align = alClient
            TabOrder = 0
            DataSource = DTDocumente
            Filter.Criteria = {00000000}
            LookAndFeel = lfFlat
            OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
            OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
            OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
            ShowRowFooter = True
            object GridIstoricMaterialPREDATOR: TdxDBGridMaskColumn
              Caption = 'Predator'
              HeaderAlignment = taCenter
              Width = 100
              BandIndex = 0
              RowIndex = 0
              FieldName = 'PREDATOR'
            end
            object GridIstoricMaterialPRIMITOR: TdxDBGridMaskColumn
              Caption = 'Primitor'
              HeaderAlignment = taCenter
              Width = 130
              BandIndex = 0
              RowIndex = 0
              FieldName = 'PRIMITOR'
            end
            object GridIstoricMaterialCOD_DOCUM: TdxDBGridMaskColumn
              Caption = 'Doc'
              HeaderAlignment = taCenter
              Width = 48
              BandIndex = 0
              RowIndex = 0
              FieldName = 'COD_DOCUM'
            end
            object GridIstoricMaterialNR_DOCUM: TdxDBGridMaskColumn
              Caption = 'Nr'
              HeaderAlignment = taCenter
              Width = 60
              BandIndex = 0
              RowIndex = 0
              FieldName = 'NR_DOCUM'
            end
            object GridIstoricMaterialDATA_DOCUM: TdxDBGridDateColumn
              Caption = 'Data'
              HeaderAlignment = taCenter
              Sorted = csUp
              Width = 65
              BandIndex = 0
              RowIndex = 0
              FieldName = 'DATA_DOCUM'
            end
            object GridIstoricMaterialOPERATOR: TdxDBGridMaskColumn
              Caption = 'Operator'
              HeaderAlignment = taCenter
              Width = 80
              BandIndex = 0
              RowIndex = 0
              FieldName = 'OPERATOR'
            end
            object GridIstoricMaterialCANTITATE_BEFORE: TdxDBGridMaskColumn
              Caption = 'Inainte'
              HeaderAlignment = taCenter
              Width = 60
              BandIndex = 0
              RowIndex = 0
              FieldName = 'CANTITATE_BEFORE'
            end
            object GridIstoricMaterialCANTITATE: TdxDBGridMaskColumn
              Caption = 'Cantitate'
              HeaderAlignment = taCenter
              Width = 60
              BandIndex = 0
              RowIndex = 0
              FieldName = 'CANTITATE'
            end
            object GridIstoricMaterialCANTITATE_AFTER: TdxDBGridMaskColumn
              Caption = 'Dupa'
              HeaderAlignment = taCenter
              Width = 60
              BandIndex = 0
              RowIndex = 0
              FieldName = 'CANTITATE_AFTER'
            end
            object GridIstoricMaterialTIP_MATERIAL: TdxDBGridMaskColumn
              Caption = 'Tip Mat'
              HeaderAlignment = taCenter
              Width = 100
              BandIndex = 0
              RowIndex = 0
              FieldName = 'TIP_MATERIAL'
            end
            object GridIstoricMaterialSEMN: TdxDBGridImageColumn
              Alignment = taLeftJustify
              Caption = 'Semn'
              HeaderAlignment = taCenter
              MinWidth = 16
              Width = 60
              BandIndex = 0
              RowIndex = 0
              FieldName = 'SEMN'
              DefaultImages = False
              Descriptions.Strings = (
                'Scazut'
                'Ignorat'
                'Adunat')
              Images = frmData.SemnImagini
              ImageIndexes.Strings = (
                '1'
                '-1'
                '0')
              ShowDescription = True
              Values.Strings = (
                '-1'
                '0'
                '1')
            end
            object GridIstoricMaterialPRET_UNITAR: TdxDBGridCurrencyColumn
              Caption = 'Pret Unitar'
              HeaderAlignment = taCenter
              BandIndex = 0
              RowIndex = 0
              FieldName = 'PRET_UNITAR'
              Nullable = False
            end
            object GridIstoricMaterialVALOARE: TdxDBGridCurrencyColumn
              Caption = 'Valoare'
              HeaderAlignment = taCenter
              BandIndex = 0
              RowIndex = 0
              FieldName = 'VALOARE'
              Nullable = False
            end
          end
        end
        object pnGestiuni: TPanel
          Left = 1
          Top = 1
          Width = 256
          Height = 356
          Align = alLeft
          TabOrder = 1
          ExplicitHeight = 410
          object treeRepartitori: TcxDBTreeList
            Left = 1
            Top = 1
            Width = 254
            Height = 354
            Align = alClient
            Bands = <
              item
              end>
            DataController.DataSource = DTRepartitori
            DataController.ParentField = 'id_parinte'
            DataController.KeyField = 'id_repartitori'
            Navigator.Buttons.CustomButtons = <>
            OptionsData.CancelOnExit = False
            OptionsData.Editing = False
            OptionsData.Deleting = False
            OptionsView.ColumnAutoWidth = True
            OptionsView.CheckGroups = True
            OptionsView.ShowRoot = False
            RootValue = -1
            ScrollbarAnnotations.CustomAnnotations = <>
            TabOrder = 0
            OnNodeCheckChanged = treeRepartitoriNodeCheckChanged
            ExplicitHeight = 408
            object treeRepartitoriNUME: TcxDBTreeListColumn
              Caption.AlignHorz = taCenter
              Caption.Text = 'Gestiune'
              DataBinding.FieldName = 'nume'
              Width = 100
              Position.ColIndex = 0
              Position.RowIndex = 0
              Position.BandIndex = 0
              Summary.FooterSummaryItems = <>
              Summary.GroupFooterSummaryItems = <>
            end
          end
        end
        object pnListaStock: TPanel
          Left = 260
          Top = 1
          Width = 683
          Height = 356
          Align = alClient
          TabOrder = 2
          ExplicitHeight = 410
          object gridStocuri: TcxGrid
            Left = 1
            Top = 1
            Width = 681
            Height = 354
            Align = alClient
            TabOrder = 0
            LookAndFeel.Kind = lfFlat
            ExplicitHeight = 408
            object viewStocuri: TcxGridDBBandedTableView
              Navigator.Buttons.CustomButtons = <>
              ScrollbarAnnotations.CustomAnnotations = <>
              DataController.DataSource = DTStocuri
              DataController.Filter.MaxValueListCount = 1000
              DataController.Filter.Active = True
              DataController.KeyFieldNames = 'CODMAT'
              DataController.Summary.DefaultGroupSummaryItems = <>
              DataController.Summary.FooterSummaryItems = <>
              DataController.Summary.SummaryGroups = <>
              DataController.OnFilterRecord = viewStocuriDataControllerFilterRecord
              Filtering.ColumnPopup.MaxDropDownItemCount = 12
              OptionsBehavior.IncSearch = True
              OptionsBehavior.FocusCellOnCycle = True
              OptionsBehavior.ImmediateEditor = False
              OptionsData.CancelOnExit = False
              OptionsData.Deleting = False
              OptionsData.DeletingConfirmation = False
              OptionsData.Editing = False
              OptionsData.Inserting = False
              OptionsSelection.HideFocusRectOnExit = False
              OptionsView.GroupByBox = False
              OptionsView.GroupFooters = gfVisibleWhenExpanded
              Preview.AutoHeight = False
              Preview.MaxLineCount = 2
              Bands = <
                item
                  Caption = 'Material'
                end
                item
                  Caption = 'Gestiuni'
                end>
              object viewStocuriCODMAT: TcxGridDBBandedColumn
                Caption = 'Cod'
                DataBinding.FieldName = 'CODMAT'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 37
                Position.BandIndex = 0
                Position.ColIndex = 0
                Position.RowIndex = 0
              end
              object viewStocuriTIPMAT: TcxGridDBBandedColumn
                Caption = 'Tip Mat'
                DataBinding.FieldName = 'TIPMAT'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 96
                Position.BandIndex = 0
                Position.ColIndex = 1
                Position.RowIndex = 0
              end
              object viewStocuriDENMAT: TcxGridDBBandedColumn
                Caption = 'Den Mat'
                DataBinding.FieldName = 'DENMAT'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 105
                Position.BandIndex = 0
                Position.ColIndex = 2
                Position.RowIndex = 0
              end
              object viewStocuriUM: TcxGridDBBandedColumn
                DataBinding.FieldName = 'UM'
                PropertiesClassName = 'TcxMaskEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.MaxLength = 0
                Properties.ReadOnly = True
                HeaderAlignmentHorz = taCenter
                Width = 31
                Position.BandIndex = 0
                Position.ColIndex = 3
                Position.RowIndex = 0
              end
              object viewStocuriCANTITATE: TcxGridDBBandedColumn
                Caption = 'TOTAL'
                DataBinding.FieldName = 'CANTITATE'
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
                Width = 80
                Position.BandIndex = 0
                Position.ColIndex = 4
                Position.RowIndex = 0
              end
              object viewStocuriPRET_UNITAR: TcxGridDBBandedColumn
                Caption = 'Pret'
                DataBinding.FieldName = 'PRET_UNITAR'
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
                Width = 80
                Position.BandIndex = 0
                Position.ColIndex = 5
                Position.RowIndex = 0
              end
              object viewStocuriPRET_RECEPTIE: TcxGridDBBandedColumn
                Caption = 'Pret Receptie'
                DataBinding.FieldName = 'PRET_RECEPTIE'
                PropertiesClassName = 'TcxCurrencyEditProperties'
                Properties.Alignment.Horz = taLeftJustify
                Properties.Alignment.Vert = taTopJustify
                Properties.AssignedValues.MaxValue = True
                Properties.AssignedValues.MinValue = True
                Properties.DecimalPlaces = 2
                Properties.DisplayFormat = ',0.00 lei;-,0.00 lei'
                Properties.Nullable = False
                Properties.ReadOnly = True
                Visible = False
                HeaderAlignmentHorz = taCenter
                Width = 82
                Position.BandIndex = 0
                Position.ColIndex = 7
                Position.RowIndex = 0
              end
              object viewStocuriPRET_RECEPTIE_TVA: TcxGridDBBandedColumn
                Caption = 'Pret Rec. TVA'
                DataBinding.FieldName = 'PRET_RECEPTIE_TVA'
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
                Width = 75
                Position.BandIndex = 0
                Position.ColIndex = 6
                Position.RowIndex = 0
              end
            end
            object nivelStocuri: TcxGridLevel
              GridView = viewStocuri
            end
          end
        end
      end
    end
    object tabStocuriPivot: TcxTabSheet
      Caption = 'Stocuri Unitate'
      ImageIndex = 1
      ExplicitHeight = 614
      object pivotStocuri: TcxDBPivotGrid
        Left = 0
        Top = 0
        Width = 944
        Height = 560
        Align = alClient
        DataSource = dtStocuriPivot
        Groups = <>
        TabOrder = 0
        ExplicitHeight = 614
        object pivotStocuriid_gest_tip_stoc: TcxDBPivotGridField
          AreaIndex = 0
          DataBinding.FieldName = 'id_gest_tip_stoc'
          UniqueName = 'id_gest_tip_stoc'
        end
        object pivotStocuricodmat: TcxDBPivotGridField
          AreaIndex = 1
          DataBinding.FieldName = 'codmat'
          UniqueName = 'codmat'
        end
        object pivotStocurigestiune: TcxDBPivotGridField
          Area = faColumn
          AreaIndex = 0
          DataBinding.FieldName = 'gestiune'
          Visible = True
          UniqueName = 'gestiune'
        end
        object pivotStocurigestint: TcxDBPivotGridField
          AreaIndex = 2
          IsCaptionAssigned = True
          Caption = 'Gestiune Interna'
          DataBinding.FieldName = 'gestint'
          Visible = True
          UniqueName = 'Gestiune Interna'
        end
        object pivotStocurigest_predator: TcxDBPivotGridField
          AreaIndex = 3
          IsCaptionAssigned = True
          Caption = 'Predator'
          DataBinding.FieldName = 'gest_predator'
          Visible = True
          UniqueName = 'Predator'
        end
        object pivotStocurigest_primitor: TcxDBPivotGridField
          AreaIndex = 4
          IsCaptionAssigned = True
          Caption = 'Primitor'
          DataBinding.FieldName = 'gest_primitor'
          Visible = True
          UniqueName = 'Primitor'
        end
        object pivotStocuriprodus: TcxDBPivotGridField
          AreaIndex = 5
          IsCaptionAssigned = True
          Caption = 'Tip Produs'
          DataBinding.FieldName = 'produs'
          Visible = True
          UniqueName = 'Tip Produs'
        end
        object pivotStocuricod_docum: TcxDBPivotGridField
          AreaIndex = 6
          IsCaptionAssigned = True
          Caption = 'Tip Document'
          DataBinding.FieldName = 'cod_docum'
          Visible = True
          UniqueName = 'Tip Document'
        end
        object pivotStocurinr_docum: TcxDBPivotGridField
          AreaIndex = 7
          IsCaptionAssigned = True
          Caption = 'Nr Document'
          DataBinding.FieldName = 'nr_docum'
          Visible = True
          UniqueName = 'Nr Document'
        end
        object pivotStocuridata_docum: TcxDBPivotGridField
          AreaIndex = 8
          IsCaptionAssigned = True
          Caption = 'Data Document'
          DataBinding.FieldName = 'data_docum'
          Visible = True
          UniqueName = 'Data Document'
        end
        object pivotStocuritipmat: TcxDBPivotGridField
          Area = faRow
          AreaIndex = 0
          IsCaptionAssigned = True
          Caption = 'Tip Material'
          DataBinding.FieldName = 'tipmat'
          Visible = True
          UniqueName = 'Tip Material'
        end
        object pivotStocuridenmat: TcxDBPivotGridField
          Area = faRow
          AreaIndex = 1
          IsCaptionAssigned = True
          Caption = 'Den Material'
          DataBinding.FieldName = 'denmat'
          Visible = True
          UniqueName = 'Den Material'
        end
        object pivotStocurium: TcxDBPivotGridField
          AreaIndex = 9
          IsCaptionAssigned = True
          Caption = 'UM'
          DataBinding.FieldName = 'um'
          Visible = True
          UniqueName = 'UM'
        end
        object pivotStocuripret_unitar: TcxDBPivotGridField
          Area = faData
          AreaIndex = 2
          IsCaptionAssigned = True
          Caption = 'Pret Unitar'
          DataBinding.FieldName = 'pret_unitar'
          Visible = True
          UniqueName = 'Pret Unitar'
        end
        object pivotStocuristock: TcxDBPivotGridField
          Area = faData
          AreaIndex = 0
          IsCaptionAssigned = True
          Caption = 'Stoc'
          DataBinding.FieldName = 'stock'
          Visible = True
          UniqueName = 'Stoc'
        end
        object pivotStocuristockValoric: TcxDBPivotGridField
          Area = faData
          AreaIndex = 1
          IsCaptionAssigned = True
          Caption = 'Stoc Valoric'
          DataBinding.FieldName = 'stockValoric'
          Visible = True
          UniqueName = 'Stoc Valoric'
        end
        object pivotStocuricont: TcxDBPivotGridField
          AreaIndex = 10
          IsCaptionAssigned = True
          Caption = 'Cont Contabil'
          DataBinding.FieldName = 'cont'
          Visible = True
          UniqueName = 'Cont Contabil'
        end
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 584
    Width = 944
    Height = 95
    Align = alBottom
    TabOrder = 1
    object Label1: TLabel
      Left = 429
      Top = 8
      Width = 40
      Height = 13
      Caption = 'Tip Stoc'
    end
    object Label2: TLabel
      Left = 12
      Top = 8
      Width = 47
      Height = 13
      Caption = 'Cont Stoc'
    end
    object chkCuMiscari: TcxCheckBox
      Left = 240
      Top = 4
      Caption = 'Afiseaza pozitiile cu miscari'
      Properties.OnChange = chkCuMiscariPropertiesChange
      TabOrder = 0
    end
    object edtTipStoc: TcxImageComboBox
      Left = 473
      Top = 4
      Properties.Items = <>
      Properties.OnChange = edtTipStocChange
      TabOrder = 1
      Width = 272
    end
    object edtCont: TcxButtonEdit
      Left = 71
      Top = 4
      Properties.Buttons = <
        item
          Kind = bkEllipsis
        end>
      Properties.OnButtonClick = edtContPropertiesButtonClick
      TabOrder = 2
      Width = 139
    end
  end
  object DTRepartitori: TDataSource
    DataSet = QryRepartitori
    Left = 57
    Top = 137
  end
  object DTStocuri: TDataSource
    DataSet = QryStocuri
    Left = 57
    Top = 193
  end
  object DTDocumente: TDataSource
    DataSet = QryDocument
    Left = 57
    Top = 249
  end
  object QryRepartitori: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryRepartitoriAfterOpen
    SQL.Strings = (
      
        'select id_repartitori, nume, id_parinte = convert(int, null) fro' +
        'm repartitori where 1=0')
    Params = <>
    Left = 129
    Top = 137
  end
  object QryStocuri: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryStocuriAfterOpen
    SQL.Strings = (
      'EXEC SP_GETSTOCK_UNITATE_NEW :TIP_STOC, NULL, :CU_MISCARI, :CONT')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_STOC'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_MISCARI'
        ParamType = ptUnknown
        Size = 2
        Value = False
      end
      item
        DataType = ftUnknown
        Name = 'CONT'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 131
    Top = 193
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_STOC'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_MISCARI'
        ParamType = ptUnknown
        Size = 2
        Value = False
      end
      item
        DataType = ftUnknown
        Name = 'CONT'
        ParamType = ptUnknown
        Size = -1
      end>
  end
  object QryDocument: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_GETFISA_MATERIAL 1, :CODMAT')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end>
    DataSource = DTStocuri
    Left = 129
    Top = 249
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ppStocuri: TPopupMenu
    Left = 97
    Top = 305
    object ppFisaMaterial: TMenuItem
      Caption = 'Fisa Material'
      ShortCut = 16464
      OnClick = ppFisaMaterialClick
    end
  end
  object qryStocuriPivot: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryStocuriAfterOpen
    SQL.Strings = (
      
        'select * from vStocUnitate where id_gest_tip_stoc = :refTipStock' +
        ' and cont like :cont')
    Params = <
      item
        DataType = ftUnknown
        Name = 'refTipStock'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'CONT'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 131
    Top = 81
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'refTipStock'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'CONT'
        ParamType = ptUnknown
        Size = -1
      end>
  end
  object dtStocuriPivot: TDataSource
    DataSet = qryStocuriPivot
    Left = 57
    Top = 81
  end
end
