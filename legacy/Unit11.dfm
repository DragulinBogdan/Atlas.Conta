object Form1: TForm1
  Left = 0
  Top = 0
  Caption = 'Defalcare Angajamente'
  ClientHeight = 677
  ClientWidth = 1348
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object gridAngajamente: TcxGrid
    Left = 0
    Top = 0
    Width = 1348
    Height = 677
    Align = alClient
    TabOrder = 0
    object viewAngajamente: TcxGridDBBandedTableView
      Navigator.Buttons.CustomButtons = <>
      Navigator.Visible = True
      FindPanel.DisplayMode = fpdmAlways
      ScrollbarAnnotations.CustomAnnotations = <>
      OnFocusedRecordChanged = viewAngajamenteFocusedRecordChanged
      DataController.DataSource = dtAngajamente
      DataController.KeyFieldNames = 'ID_ALOP_ANGAJAMENTE'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      EditForm.CaptionMask = 'Modificare Angajament'
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      Bands = <
        item
          Width = 1388
        end
        item
          Visible = False
        end>
      object viewAngajamentenr_contract: TcxGridDBBandedColumn
        DataBinding.FieldName = 'nr_contract'
        Visible = False
        Width = 222
        Position.BandIndex = 0
        Position.ColIndex = 0
        Position.RowIndex = 0
      end
      object viewAngajamenteid_alop_angajamente: TcxGridDBBandedColumn
        DataBinding.FieldName = 'id_alop_angajamente'
        Visible = False
        Width = 151
        Position.BandIndex = 0
        Position.ColIndex = 1
        Position.RowIndex = 0
      end
      object viewAngajamentenumar: TcxGridDBBandedColumn
        Caption = 'Nr.Angajament'
        DataBinding.FieldName = 'numar'
        Width = 103
        Position.BandIndex = 0
        Position.ColIndex = 2
        Position.RowIndex = 0
      end
      object viewAngajamentedata_emitere: TcxGridDBBandedColumn
        Caption = 'Data Emitere'
        DataBinding.FieldName = 'data_emitere'
        Width = 118
        Position.BandIndex = 0
        Position.ColIndex = 3
        Position.RowIndex = 0
      end
      object viewAngajamenteid_lst_repartitori: TcxGridDBBandedColumn
        DataBinding.FieldName = 'id_lst_repartitori'
        Visible = False
        Width = 103
        Position.BandIndex = 0
        Position.ColIndex = 4
        Position.RowIndex = 0
      end
      object viewAngajamentecod_functional: TcxGridDBBandedColumn
        Caption = 'Cod functional'
        DataBinding.FieldName = 'cod_functional'
        Width = 116
        Position.BandIndex = 0
        Position.ColIndex = 6
        Position.RowIndex = 0
      end
      object viewAngajamentecodurieconomice: TcxGridDBBandedColumn
        Caption = 'Cod economic'
        DataBinding.FieldName = 'codurieconomice'
        Width = 223
        Position.BandIndex = 0
        Position.ColIndex = 7
        Position.RowIndex = 0
      end
      object viewAngajamentedisponibil: TcxGridDBBandedColumn
        Caption = 'Disponibil'
        DataBinding.FieldName = 'disponibil'
        Width = 150
        Position.BandIndex = 0
        Position.ColIndex = 8
        Position.RowIndex = 0
      end
      object viewAngajamenteramas_de_angajat: TcxGridDBBandedColumn
        Caption = 'Ramas de distribuit'
        DataBinding.FieldName = 'ramas_de_angajat'
        Width = 160
        Position.BandIndex = 0
        Position.ColIndex = 9
        Position.RowIndex = 0
      end
      object viewAngajamenteNUME_REPARTITOR: TcxGridDBBandedColumn
        Caption = 'Beneficiar'
        DataBinding.FieldName = 'NUME_REPARTITOR'
        Width = 442
        Position.BandIndex = 0
        Position.ColIndex = 5
        Position.RowIndex = 0
      end
    end
    object viewDetalii: TcxGridDBBandedTableView
      Navigator.Buttons.CustomButtons = <>
      Navigator.Visible = True
      ScrollbarAnnotations.CustomAnnotations = <>
      DataController.DataSource = dtDefalcare
      DataController.DetailKeyFieldNames = 'ID_ALOP_ANGAJAMENTE'
      DataController.KeyFieldNames = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
      DataController.MasterKeyFieldNames = 'ID_ALOP_ANGAJAMENTE'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      OptionsBehavior.EditMode = emModalEditForm
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      Bands = <
        item
          Options.HoldOwnColumnsOnly = True
          Width = 1357
        end>
      object viewDetaliiID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBBandedColumn
        DataBinding.FieldName = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
        Visible = False
        Position.BandIndex = 0
        Position.ColIndex = 0
        Position.RowIndex = 0
      end
      object viewDetaliiID_ALOP_ANGAJAMENTE: TcxGridDBBandedColumn
        DataBinding.FieldName = 'ID_ALOP_ANGAJAMENTE'
        Visible = False
        Position.BandIndex = 0
        Position.ColIndex = 1
        Position.RowIndex = 0
      end
      object viewDetaliiCOD_ECONOMIC: TcxGridDBBandedColumn
        Caption = 'Cod economic'
        DataBinding.FieldName = 'COD_ECONOMIC'
        Width = 223
        Position.BandIndex = 0
        Position.ColIndex = 6
        Position.RowIndex = 0
      end
      object viewDetaliiAPROBATE: TcxGridDBBandedColumn
        DataBinding.FieldName = 'APROBATE'
        Visible = False
        Width = 202
        Position.BandIndex = 0
        Position.ColIndex = 7
        Position.RowIndex = 0
      end
      object viewDetaliiTOTAL_ANGAJATE: TcxGridDBBandedColumn
        DataBinding.FieldName = 'TOTAL_ANGAJATE'
        Visible = False
        Width = 67
        Position.BandIndex = 0
        Position.ColIndex = 8
        Position.RowIndex = 0
      end
      object viewDetaliiDISPONIBIL: TcxGridDBBandedColumn
        Caption = 'Disponibil'
        DataBinding.FieldName = 'DISPONIBIL'
        Width = 154
        Position.BandIndex = 0
        Position.ColIndex = 9
        Position.RowIndex = 0
      end
      object viewDetaliiANGAJAT_VALUTA: TcxGridDBBandedColumn
        Caption = 'Angajat'
        DataBinding.FieldName = 'ANGAJAT_VALUTA'
        Width = 54
        Position.BandIndex = 0
        Position.ColIndex = 10
        Position.RowIndex = 0
      end
      object viewDetaliiANGAJAT: TcxGridDBBandedColumn
        Caption = 'Angajat'
        DataBinding.FieldName = 'ANGAJAT'
        Visible = False
        Width = 78
        Position.BandIndex = 0
        Position.ColIndex = 11
        Position.RowIndex = 0
      end
      object viewDetaliiRAMAS_DE_ANGAJAT: TcxGridDBBandedColumn
        Caption = 'Disponibil de angajat'
        DataBinding.FieldName = 'RAMAS_DE_ANGAJAT'
        Width = 77
        Position.BandIndex = 0
        Position.ColIndex = 12
        Position.RowIndex = 0
      end
      object viewDetaliinumar: TcxGridDBBandedColumn
        Caption = 'Nr.Angajament'
        DataBinding.FieldName = 'numar'
        Width = 147
        Position.BandIndex = 0
        Position.ColIndex = 2
        Position.RowIndex = 0
      end
      object viewDetaliicod_functional: TcxGridDBBandedColumn
        Caption = 'Cod functional'
        DataBinding.FieldName = 'cod_functional'
        Width = 117
        Position.BandIndex = 0
        Position.ColIndex = 5
        Position.RowIndex = 0
      end
      object viewDetaliidata_emitere: TcxGridDBBandedColumn
        Caption = 'Data emitere'
        DataBinding.FieldName = 'data_emitere'
        Width = 147
        Position.BandIndex = 0
        Position.ColIndex = 3
        Position.RowIndex = 0
      end
      object viewDetaliiid_lst_repartitori: TcxGridDBBandedColumn
        DataBinding.FieldName = 'id_lst_repartitori'
        Visible = False
        Width = 32
        Position.BandIndex = 0
        Position.ColIndex = 13
        Position.RowIndex = 0
      end
      object viewDetaliiNUME_REPARTITOR: TcxGridDBBandedColumn
        Caption = 'Beneficiar'
        DataBinding.FieldName = 'NUME_REPARTITOR'
        Width = 367
        Position.BandIndex = 0
        Position.ColIndex = 4
        Position.RowIndex = 0
      end
    end
    object nivelAngajament: TcxGridLevel
      GridView = viewAngajamente
      object nivelDefalcare: TcxGridLevel
        GridView = viewDetalii
      end
    end
  end
  object zConnection: TZConnection
    UTF8StringsAsWideField = True
    PreprepareSQL = False
    Catalog = 'Conta_2024'
    Connected = True
    Port = 0
    Database = 'Conta_2024'
    User = 'ATSUserPrivilegiat'
    Password = 'QAZ}"?xcv><M'
    Protocol = 'mssql'
    Left = 392
    Top = 240
  end
  object qryAngajamente: TZQuery
    Connection = zConnection
    Active = True
    SQL.Strings = (
      'SELECT '
      '    a.nr_contract,'
      '    a.id_alop_angajamente,'
      '    a.numar,'
      '    a.data_emitere,'
      '    a.id_lst_repartitori,'
      '    a.cod_functional,'
      '    a.codurieconomice,'
      '    d.disponibil,'
      '    d.ramas_de_angajat,'
      '    r.NUME AS NUME_REPARTITOR  -- <-- numele beneficiarului'
      ''
      'FROM alop_angajamente a'
      'OUTER APPLY ('
      '    SELECT '
      '        ramas_de_angajat = SUM(ramas_de_angajat),'
      '        disponibil = SUM(disponibil)'
      '    FROM alop_angajamente_defalcare'
      '    WHERE ID_ALOP_ANGAJAMENTE = a.ID_ALOP_ANGAJAMENTE'
      ') AS d'
      ''
      'LEFT JOIN repartitori r'
      '    ON a.id_lst_repartitori = r.ID_REPARTITORI')
    Params = <>
    Left = 288
    Top = 232
    object qryAngajamentenr_contract: TStringField
      FieldName = 'nr_contract'
      Size = 255
    end
    object qryAngajamenteid_alop_angajamente: TIntegerField
      FieldName = 'id_alop_angajamente'
      ReadOnly = True
    end
    object qryAngajamentenumar: TStringField
      FieldName = 'numar'
      Size = 50
    end
    object qryAngajamentedata_emitere: TDateTimeField
      FieldName = 'data_emitere'
    end
    object qryAngajamenteid_lst_repartitori: TIntegerField
      FieldName = 'id_lst_repartitori'
    end
    object qryAngajamentecod_functional: TStringField
      FieldName = 'cod_functional'
      Size = 100
    end
    object qryAngajamentecodurieconomice: TStringField
      FieldName = 'codurieconomice'
      Size = 255
    end
    object qryAngajamentedisponibil: TFloatField
      FieldName = 'disponibil'
    end
    object qryAngajamenteramas_de_angajat: TFloatField
      FieldName = 'ramas_de_angajat'
    end
    object qryAngajamenteNUME_REPARTITOR: TStringField
      FieldName = 'NUME_REPARTITOR'
      Size = 255
    end
  end
  object qryDefalcare: TZQuery
    Connection = zConnection
    Active = True
    SQL.Strings = (
      'SELECT '
      '    d.ID_ALOP_ANGAJAMENTE_DEFALCARE,'
      '    d.ID_ALOP_ANGAJAMENTE,'
      '    d.COD_ECONOMIC,'
      '    d.APROBATE,'
      '    d.TOTAL_ANGAJATE,'
      '    d.DISPONIBIL,'
      '    d.ANGAJAT_VALUTA,'
      '    d.ANGAJAT,'
      '    d.RAMAS_DE_ANGAJAT,'
      ''
      ''
      '    a.numar,'
      '    a.cod_functional,'
      '    a.data_emitere,'
      '    a.id_lst_repartitori,'
      ''
      ''
      '    r.NUME AS NUME_REPARTITOR'
      ''
      'FROM alop_angajamente_defalcare d'
      'INNER JOIN alop_angajamente a'
      '    ON d.ID_ALOP_ANGAJAMENTE = a.ID_ALOP_ANGAJAMENTE'
      'LEFT JOIN repartitori r'
      '    ON a.id_lst_repartitori = r.ID_REPARTITORI')
    Params = <>
    MasterSource = dtAngajamente
    Left = 288
    Top = 304
    object qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
      ReadOnly = True
    end
    object qryDefalcareID_ALOP_ANGAJAMENTE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE'
    end
    object qryDefalcareCOD_ECONOMIC: TStringField
      FieldName = 'COD_ECONOMIC'
      Size = 100
    end
    object qryDefalcareAPROBATE: TFloatField
      FieldName = 'APROBATE'
    end
    object qryDefalcareTOTAL_ANGAJATE: TFloatField
      FieldName = 'TOTAL_ANGAJATE'
    end
    object qryDefalcareDISPONIBIL: TFloatField
      FieldName = 'DISPONIBIL'
    end
    object qryDefalcareANGAJAT_VALUTA: TFloatField
      FieldName = 'ANGAJAT_VALUTA'
    end
    object qryDefalcareANGAJAT: TFloatField
      FieldName = 'ANGAJAT'
    end
    object qryDefalcareRAMAS_DE_ANGAJAT: TFloatField
      FieldName = 'RAMAS_DE_ANGAJAT'
    end
    object qryDefalcarenumar: TStringField
      FieldName = 'numar'
      Size = 50
    end
    object qryDefalcarecod_functional: TStringField
      FieldName = 'cod_functional'
      Size = 100
    end
    object qryDefalcaredata_emitere: TDateTimeField
      FieldName = 'data_emitere'
    end
    object qryDefalcareid_lst_repartitori: TIntegerField
      FieldName = 'id_lst_repartitori'
    end
    object qryDefalcareNUME_REPARTITOR: TStringField
      FieldName = 'NUME_REPARTITOR'
      Size = 255
    end
  end
  object dtAngajamente: TDataSource
    DataSet = qryAngajamente
    Left = 160
    Top = 232
  end
  object dtDefalcare: TDataSource
    DataSet = qryDefalcare
    Left = 160
    Top = 304
  end
end
