object frmListaContracteP: TfrmListaContracteP
  Left = 253
  Top = 144
  Caption = 'Contracte parinte'
  ClientHeight = 535
  ClientWidth = 1035
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object gridContracte: TcxGrid
    Left = 0
    Top = 27
    Width = 1035
    Height = 508
    Align = alClient
    TabOrder = 0
    object grdContracte: TcxGridDBTableView
      OnDblClick = grdContracteDblClick
      Navigator.Buttons.CustomButtons = <>
      DataController.DataSource = dsContracte
      DataController.KeyFieldNames = 'idContracte'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsCustomize.ColumnFiltering = False
      OptionsCustomize.ColumnGrouping = False
      OptionsCustomize.ColumnMoving = False
      OptionsCustomize.ColumnSorting = False
      OptionsCustomize.ColumnsQuickCustomization = True
      OptionsData.CancelOnExit = False
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsView.CellEndEllipsis = True
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      Styles.Header = style1
      object grdContracteidContracte: TcxGridDBColumn
        DataBinding.FieldName = 'idContracte'
        Visible = False
        HeaderAlignmentHorz = taCenter
        Width = 100
      end
      object grdContracteNrContract: TcxGridDBColumn
        Caption = 'Nr. contract'
        DataBinding.FieldName = 'NrContract'
        HeaderAlignmentHorz = taCenter
        Width = 269
      end
      object grdContracteDataContract: TcxGridDBColumn
        Caption = 'Data contract'
        DataBinding.FieldName = 'DataContract'
        HeaderAlignmentHorz = taCenter
        Width = 122
      end
      object grdContractePrestator: TcxGridDBColumn
        DataBinding.FieldName = 'Prestator'
        HeaderAlignmentHorz = taCenter
        Width = 200
      end
      object grdContracteValoare: TcxGridDBColumn
        DataBinding.FieldName = 'Valoare'
        HeaderAlignmentHorz = taCenter
        Width = 105
      end
      object grdContracteTipContract: TcxGridDBColumn
        Caption = 'Tip contract'
        DataBinding.FieldName = 'TipContract'
        HeaderAlignmentHorz = taCenter
        Width = 113
      end
      object grdContracteDataOrdinIncepere: TcxGridDBColumn
        Caption = 'Data inceput'
        DataBinding.FieldName = 'DataOrdinIncepere'
        HeaderAlignmentHorz = taCenter
        Width = 109
      end
      object grdContracteDataPVTerminare: TcxGridDBColumn
        Caption = 'Data sfarsit'
        DataBinding.FieldName = 'DataPVTerminare'
        HeaderAlignmentHorz = taCenter
        Width = 103
      end
      object grdContracteidParinte: TcxGridDBColumn
        DataBinding.FieldName = 'idParinte'
        Visible = False
      end
      object grdContracteStare: TcxGridDBColumn
        DataBinding.FieldName = 'Stare'
        Visible = False
      end
      object grdContracteidTipuriContracte: TcxGridDBColumn
        DataBinding.FieldName = 'idTipuriContracte'
        Visible = False
      end
      object grdContracteidStariContracte: TcxGridDBColumn
        DataBinding.FieldName = 'idStariContracte'
        Visible = False
      end
      object grdContracteDurataGarantieAni: TcxGridDBColumn
        DataBinding.FieldName = 'DurataGarantieAni'
        Visible = False
      end
      object grdContracteDurataOrdinLuni: TcxGridDBColumn
        DataBinding.FieldName = 'DurataOrdinLuni'
        Visible = False
      end
      object grdContracteProcentGarantieDepusa: TcxGridDBColumn
        DataBinding.FieldName = 'ProcentGarantieDepusa'
        Visible = False
      end
      object grdContracteProcentGarantieRetinuta: TcxGridDBColumn
        DataBinding.FieldName = 'ProcentGarantieRetinuta'
        Visible = False
      end
      object grdContracteNrOrdinIncepere: TcxGridDBColumn
        DataBinding.FieldName = 'NrOrdinIncepere'
        Visible = False
      end
      object grdContracteNrPVTerminare: TcxGridDBColumn
        DataBinding.FieldName = 'NrPVTerminare'
        Visible = False
      end
      object grdContracteNumarPVReceptie: TcxGridDBColumn
        DataBinding.FieldName = 'NumarPVReceptie'
        Visible = False
      end
      object grdContracteManProiectBeneficiar: TcxGridDBColumn
        DataBinding.FieldName = 'ManProiectBeneficiar'
        Visible = False
      end
      object grdContracteManProiectOfertant: TcxGridDBColumn
        DataBinding.FieldName = 'ManProiectOfertant'
        Visible = False
      end
      object grdContracteDurataOrdinAni: TcxGridDBColumn
        DataBinding.FieldName = 'DurataOrdinAni'
        Visible = False
      end
      object grdContracteDurataGarantieLuni: TcxGridDBColumn
        DataBinding.FieldName = 'DurataGarantieLuni'
        Visible = False
      end
      object grdContracteCursEuroData: TcxGridDBColumn
        DataBinding.FieldName = 'CursEuroData'
        Visible = False
      end
      object grdContracteCursEuro: TcxGridDBColumn
        DataBinding.FieldName = 'CursEuro'
        Visible = False
      end
      object grdContracteDataPVReceptie: TcxGridDBColumn
        DataBinding.FieldName = 'DataPVReceptie'
        Visible = False
      end
    end
    object grdAditionale: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      DataController.DetailKeyFieldNames = 'idParinte'
      DataController.KeyFieldNames = 'idContracte'
      DataController.MasterKeyFieldNames = 'idContracte'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsData.CancelOnExit = False
      OptionsData.Deleting = False
      OptionsData.DeletingConfirmation = False
      OptionsData.Editing = False
      OptionsData.Inserting = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      object grdAditionaleidContracte: TcxGridDBColumn
        DataBinding.FieldName = 'idContracte'
        Width = 100
      end
      object grdAditionaleidParinte: TcxGridDBColumn
        DataBinding.FieldName = 'idParinte'
        Width = 100
      end
      object grdAditionaleidInvContabilitate: TcxGridDBColumn
        DataBinding.FieldName = 'idInvContabilitate'
        Width = 100
      end
      object grdAditionaleidPrestatorContabilitate: TcxGridDBColumn
        DataBinding.FieldName = 'idPrestatorContabilitate'
        Width = 100
      end
      object grdAditionaleidBeneficiarContabilitate: TcxGridDBColumn
        DataBinding.FieldName = 'idBeneficiarContabilitate'
        Width = 100
      end
      object grdAditionaleValoare: TcxGridDBColumn
        DataBinding.FieldName = 'Valoare'
        Width = 100
      end
      object grdAditionaleNrContract: TcxGridDBColumn
        DataBinding.FieldName = 'NrContract'
        Width = 100
      end
      object grdAditionaleDataContract: TcxGridDBColumn
        DataBinding.FieldName = 'DataContract'
        Width = 100
      end
      object grdAditionaleTipContract: TcxGridDBColumn
        DataBinding.FieldName = 'TipContract'
        Width = 100
      end
      object grdAditionaleStare: TcxGridDBColumn
        DataBinding.FieldName = 'Stare'
        Width = 100
      end
      object grdAditionaleDataStart: TcxGridDBColumn
        DataBinding.FieldName = 'DataStart'
        Width = 100
      end
      object grdAditionaleDataStop: TcxGridDBColumn
        DataBinding.FieldName = 'DataStop'
        Width = 100
      end
      object grdAditionaleTermenFinalizare: TcxGridDBColumn
        DataBinding.FieldName = 'TermenFinalizare'
        Width = 100
      end
      object grdAditionaleDataOrdinIncepere: TcxGridDBColumn
        DataBinding.FieldName = 'DataOrdinIncepere'
        Width = 100
      end
      object grdAditionaleManProiectBeneficiar: TcxGridDBColumn
        DataBinding.FieldName = 'ManProiectBeneficiar'
        Width = 100
      end
      object grdAditionaleManProiectOfertant: TcxGridDBColumn
        DataBinding.FieldName = 'ManProiectOfertant'
        Width = 100
      end
      object grdAditionaleGarantieLucrariStart: TcxGridDBColumn
        DataBinding.FieldName = 'GarantieLucrariStart'
        Width = 100
      end
      object grdAditionaleGarantieLucrariStop: TcxGridDBColumn
        DataBinding.FieldName = 'GarantieLucrariStop'
        Width = 100
      end
    end
    object nivelContracte: TcxGridLevel
      GridView = grdContracte
    end
  end
  object pnl1: TPanel
    Left = 0
    Top = 0
    Width = 1035
    Height = 27
    Align = alTop
    BevelOuter = bvLowered
    Color = 12369018
    TabOrder = 1
    object btnReset: TSpeedButton
      Left = 738
      Top = 4
      Width = 23
      Height = 19
      Caption = 'R'
      OnClick = btnResetClick
    end
    object txtFiltruNrContr: TcxTextEdit
      Left = 68
      Top = 3
      Properties.OnChange = txtFiltruNrContrPropertiesChange
      TabOrder = 0
      Width = 121
    end
    object lbl1: TcxLabel
      Left = 3
      Top = 5
      Caption = 'Nr. contract:'
    end
    object cbxFiltruExecutant: TcxLookupComboBox
      Left = 468
      Top = 3
      Properties.DropDownAutoSize = True
      Properties.DropDownSizeable = True
      Properties.GridMode = True
      Properties.ImmediatePost = True
      Properties.KeyFieldNames = 'ID_REPARTITORI'
      Properties.ListColumns = <
        item
          FieldName = 'NUME'
        end>
      Properties.ListOptions.ShowHeader = False
      Properties.ListSource = dsListaExecutanti
      Properties.OnCloseUp = cbxFiltruExecutantPropertiesCloseUp
      TabOrder = 2
      Width = 269
    end
    object lbl2: TcxLabel
      Left = 202
      Top = 5
      Caption = 'Data contract:'
    end
    object lbl3: TcxLabel
      Left = 411
      Top = 5
      Caption = 'Executant:'
    end
    object dtFiltruDataContr: TcxDateEdit
      Left = 276
      Top = 3
      Properties.OnChange = dtFiltruDataContrPropertiesChange
      TabOrder = 5
      Width = 121
    end
  end
  object dsContracte: TDataSource
    DataSet = qryContracte
    Left = 80
    Top = 120
  end
  object cxstylrpstry1: TcxStyleRepository
    Left = 32
    Top = 64
    PixelsPerInch = 96
    object style1: TcxStyle
      AssignedValues = [svColor, svFont]
      Color = 9079296
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlack
      Font.Height = -13
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
    end
  end
  object dsListaExecutanti: TDataSource
    DataSet = qryListaExecutanti
    Left = 80
    Top = 88
  end
  object qryListaExecutanti: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from repartitori'
      'order by nume')
    Params = <>
    Left = 112
    Top = 88
  end
  object qryContracte: TZQuery
    SQL.Strings = (
      'exec spListaContracteParinte :idUtilizator')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idUtilizator'
        ParamType = ptUnknown
      end>
    Left = 112
    Top = 120
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idUtilizator'
        ParamType = ptUnknown
      end>
  end
end
