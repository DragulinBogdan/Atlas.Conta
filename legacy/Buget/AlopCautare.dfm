object frmCautareAlop: TfrmCautareAlop
  Left = 210
  Top = 160
  Width = 926
  Height = 623
  Caption = 'Cautare ALOP'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsMDIChild
  OldCreateOrder = False
  Position = poDefault
  Visible = True
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object grpAngajament: TGroupBox
    Left = 0
    Top = 0
    Width = 918
    Height = 201
    Align = alTop
    TabOrder = 0
    object rgCautare: TRadioGroup
      Left = 584
      Top = 8
      Width = 257
      Height = 185
      Caption = 'Criterii de cautare'
      ItemIndex = 0
      Items.Strings = (
        'Urmarire angajament'
        'Urmarire factura'
        'Urmarire plata')
      TabOrder = 0
      OnClick = rgCautareClick
    end
    object gbxFactura: TGroupBox
      Left = 8
      Top = 72
      Width = 569
      Height = 57
      Caption = 'Factura'
      TabOrder = 1
      Visible = False
      object Label2: TLabel
        Left = 41
        Top = 12
        Width = 53
        Height = 13
        Caption = 'Nr obligatie'
      end
      object Label3: TLabel
        Left = 193
        Top = 12
        Width = 65
        Height = 13
        Caption = 'Data obligatie'
      end
      object Label4: TLabel
        Left = 348
        Top = 12
        Width = 37
        Height = 13
        Caption = 'Furnizor'
      end
      object ckbNrFct: TcxCheckBox
        Left = 16
        Top = 28
        TabOrder = 0
        Width = 25
      end
      object edtNrObligatie: TcxTextEdit
        Left = 40
        Top = 29
        TabOrder = 1
        Width = 105
      end
      object ckbDataFct: TcxCheckBox
        Left = 160
        Top = 28
        TabOrder = 2
        Width = 25
      end
      object edtDataFactura: TcxDateEdit
        Left = 192
        Top = 29
        TabOrder = 3
        Width = 121
      end
      object ckbFurnizFct: TcxCheckBox
        Left = 320
        Top = 28
        TabOrder = 4
        Width = 25
      end
      object cbxFurnizFact: TcxComboBox
        Left = 344
        Top = 28
        TabOrder = 5
        Width = 161
      end
    end
    object gbxAngajamente: TGroupBox
      Left = 8
      Top = 8
      Width = 569
      Height = 57
      Caption = 'gbxAngajamente'
      TabOrder = 2
      object lblFurnizor: TLabel
        Left = 40
        Top = 16
        Width = 37
        Height = 13
        Caption = 'Furnizor'
      end
      object Label1: TLabel
        Left = 231
        Top = 18
        Width = 61
        Height = 13
        Caption = 'Departament'
      end
      object Label5: TLabel
        Left = 432
        Top = 17
        Width = 81
        Height = 13
        Caption = 'Data angajament'
      end
      object ckbFurnizor: TcxCheckBox
        Left = 8
        Top = 32
        TabOrder = 0
        Width = 25
      end
      object cbxFurnizor: TcxComboBox
        Left = 32
        Top = 32
        TabOrder = 1
        Width = 161
      end
      object ckbDepartament: TcxCheckBox
        Left = 208
        Top = 32
        TabOrder = 2
        Width = 25
      end
      object cbxDepartament: TcxComboBox
        Left = 232
        Top = 32
        TabOrder = 3
        Width = 161
      end
      object ckbDataAngajament: TcxCheckBox
        Left = 400
        Top = 32
        TabOrder = 4
        Width = 25
      end
      object edtDataAngajament: TcxDateEdit
        Left = 432
        Top = 32
        TabOrder = 5
        Width = 121
      end
    end
    object gbxPlata: TGroupBox
      Left = 8
      Top = 136
      Width = 569
      Height = 57
      Caption = 'Plata'
      TabOrder = 3
      Visible = False
      object Label6: TLabel
        Left = 41
        Top = 12
        Width = 37
        Height = 13
        Caption = 'Nr plata'
      end
      object Label7: TLabel
        Left = 193
        Top = 12
        Width = 49
        Height = 13
        Caption = 'Data plata'
      end
      object Label8: TLabel
        Left = 356
        Top = 11
        Width = 46
        Height = 13
        Caption = 'Repartitor'
      end
      object ckbNumar: TcxCheckBox
        Left = 16
        Top = 28
        TabOrder = 0
        Width = 25
      end
      object edtNrplata: TcxTextEdit
        Left = 40
        Top = 29
        TabOrder = 1
        Width = 105
      end
      object ckbData: TcxCheckBox
        Left = 160
        Top = 28
        TabOrder = 2
        Width = 25
      end
      object edtPlata: TcxDateEdit
        Left = 192
        Top = 29
        TabOrder = 3
        Width = 121
      end
      object ckbREpartitor: TcxCheckBox
        Left = 321
        Top = 27
        TabOrder = 4
        Width = 25
      end
      object cbxRepartitor: TcxComboBox
        Left = 352
        Top = 27
        TabOrder = 5
        Width = 161
      end
    end
  end
  object Panel2: TPanel
    Left = 0
    Top = 201
    Width = 918
    Height = 33
    Align = alTop
    TabOrder = 1
    object btnCautare: TcxButton
      Left = 16
      Top = 6
      Width = 113
      Height = 25
      Caption = 'Cautare'
      TabOrder = 0
      OnClick = btnCautareClick
    end
    object btnPrintare: TcxButton
      Left = 176
      Top = 6
      Width = 91
      Height = 25
      Caption = 'Printare'
      TabOrder = 1
      OnClick = btnPrintareClick
    end
  end
  object pcALOP: TcxPageControl
    Left = 0
    Top = 234
    Width = 918
    Height = 362
    ActivePage = tabAngajamente
    Align = alClient
    TabOrder = 2
    ClientRectBottom = 362
    ClientRectRight = 918
    ClientRectTop = 24
    object tabAngajamente: TcxTabSheet
      Caption = 'Angajamente'
      ImageIndex = 1
      object cxGrid1: TcxGrid
        Left = 0
        Top = 0
        Width = 918
        Height = 338
        Align = alClient
        TabOrder = 0
        object cxGrid1DBTableView1: TcxGridDBTableView
          NavigatorButtons.ConfirmDelete = False
          DataController.DataSource = DataSource1
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          object cxGrid1DBTableView1id_alop_angajamente: TcxGridDBColumn
            DataBinding.FieldName = 'id_alop_angajamente'
            Visible = False
          end
          object cxGrid1DBTableView1data_emitere: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'data_emitere'
            PropertiesClassName = 'TcxDateEditProperties'
            Width = 83
          end
          object cxGrid1DBTableView1id_departament: TcxGridDBColumn
            Caption = 'Departament'
            DataBinding.FieldName = 'id_departament'
            PropertiesClassName = 'TcxLookupComboBoxProperties'
            Properties.KeyFieldNames = 'ID_REPARTITORI'
            Properties.ListColumns = <
              item
                FieldName = 'NUME'
              end>
            Properties.ListSource = frmData.DTRepartitori
            Width = 157
          end
          object cxGrid1DBTableView1numar: TcxGridDBColumn
            Caption = 'Nr'
            DataBinding.FieldName = 'numar'
            Width = 60
          end
          object cxGrid1DBTableView1id_lst_repartitori: TcxGridDBColumn
            DataBinding.FieldName = 'id_lst_repartitori'
            PropertiesClassName = 'TcxLookupComboBoxProperties'
            Properties.KeyFieldNames = 'ID_REPARTITORI'
            Properties.ListColumns = <
              item
                FieldName = 'NUME'
              end>
            Properties.ListSource = DTREP
            Width = 157
          end
          object cxGrid1DBTableView1cod_functional: TcxGridDBColumn
            DataBinding.FieldName = 'cod_functional'
            Width = 118
          end
          object cxGrid1DBTableView1cod_economic: TcxGridDBColumn
            DataBinding.FieldName = 'cod_economic'
            Width = 113
          end
          object cxGrid1DBTableView1id_alop_angajamente_defalcare: TcxGridDBColumn
            DataBinding.FieldName = 'id_alop_angajamente_defalcare'
            Visible = False
          end
          object cxGrid1DBTableView1aprobate: TcxGridDBColumn
            Caption = 'Aprobate'
            DataBinding.FieldName = 'aprobate'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Width = 90
          end
          object cxGrid1DBTableView1total_angajate: TcxGridDBColumn
            Caption = 'Total Angajate'
            DataBinding.FieldName = 'total_angajate'
            Width = 63
          end
          object cxGrid1DBTableView1disponibil: TcxGridDBColumn
            Caption = 'Disponibil'
            DataBinding.FieldName = 'disponibil'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Width = 60
          end
          object cxGrid1DBTableView1angajat: TcxGridDBColumn
            Caption = 'Angajat'
            DataBinding.FieldName = 'angajat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Width = 60
          end
          object cxGrid1DBTableView1ramas_de_angajat: TcxGridDBColumn
            Caption = 'Ramas de ang'
            DataBinding.FieldName = 'ramas_de_angajat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Width = 80
          end
        end
        object cxGrid1Level1: TcxGridLevel
          GridView = cxGrid1DBTableView1
        end
      end
    end
    object tabLichidare: TcxTabSheet
      Caption = 'Lichidare'
      ImageIndex = 0
      OnShow = tabLichidareShow
      object Panel1: TPanel
        Left = 0
        Top = 0
        Width = 918
        Height = 338
        Align = alClient
        TabOrder = 0
        object GroupBox1: TGroupBox
          Left = 1
          Top = 1
          Width = 916
          Height = 336
          Align = alClient
          Caption = 'Lichidare'
          TabOrder = 0
          object Splitter1: TSplitter
            Left = 2
            Top = 15
            Width = 912
            Height = 3
            Cursor = crVSplit
            Align = alTop
          end
          object cxGrid2: TcxGrid
            Left = 2
            Top = 18
            Width = 912
            Height = 316
            Align = alClient
            TabOrder = 0
            object cxGrid2DBTableView1: TcxGridDBTableView
              NavigatorButtons.ConfirmDelete = False
              DataController.DataSource = DTLichidari
              DataController.Summary.DefaultGroupSummaryItems = <>
              DataController.Summary.FooterSummaryItems = <>
              DataController.Summary.SummaryGroups = <>
              object cxGrid2DBTableView1Nr_docum: TcxGridDBColumn
                DataBinding.FieldName = 'Nr_docum'
                Width = 58
              end
              object cxGrid2DBTableView1tipdoc: TcxGridDBColumn
                DataBinding.FieldName = 'tipdoc'
                Width = 50
              end
              object cxGrid2DBTableView1data_docum: TcxGridDBColumn
                DataBinding.FieldName = 'data_docum'
                Width = 86
              end
              object cxGrid2DBTableView1id_predator: TcxGridDBColumn
                DataBinding.FieldName = 'id_predator'
                PropertiesClassName = 'TcxLookupComboBoxProperties'
                Properties.KeyFieldNames = 'ID_REPARTITORI'
                Properties.ListColumns = <
                  item
                    FieldName = 'NUME'
                  end>
                Properties.ListSource = DTREP
                Width = 106
              end
              object cxGrid2DBTableView1cod_functional: TcxGridDBColumn
                DataBinding.FieldName = 'cod_functional'
                Width = 125
              end
              object cxGrid2DBTableView1cod_economic: TcxGridDBColumn
                DataBinding.FieldName = 'cod_economic'
                Width = 119
              end
              object cxGrid2DBTableView1suma: TcxGridDBColumn
                DataBinding.FieldName = 'suma'
                PropertiesClassName = 'TcxCurrencyEditProperties'
                Width = 115
              end
              object cxGrid2DBTableView1id_primitor: TcxGridDBColumn
                DataBinding.FieldName = 'id_primitor'
                PropertiesClassName = 'TcxLookupComboBoxProperties'
                Properties.KeyFieldNames = 'ID_REPARTITORI'
                Properties.ListColumns = <
                  item
                    FieldName = 'NUME'
                  end>
                Properties.ListSource = DTREP
                Width = 150
              end
            end
            object cxGrid2Level1: TcxGridLevel
              GridView = cxGrid2DBTableView1
            end
          end
        end
      end
    end
    object tabOrdonantare: TcxTabSheet
      Caption = 'Ordonantare'
      ImageIndex = 2
      object GroupBox2: TGroupBox
        Left = 0
        Top = 0
        Width = 918
        Height = 338
        Align = alClient
        Caption = 'Ordonantare'
        TabOrder = 0
        object cxGrid3: TcxGrid
          Left = 2
          Top = 15
          Width = 914
          Height = 321
          Align = alClient
          TabOrder = 0
          object cxGrid3DBTableView1: TcxGridDBTableView
            NavigatorButtons.ConfirmDelete = False
            DataController.DataSource = DTOrdonantare
            DataController.Summary.DefaultGroupSummaryItems = <>
            DataController.Summary.FooterSummaryItems = <>
            DataController.Summary.SummaryGroups = <>
            object cxGrid3DBTableView1id_alop_angajamente: TcxGridDBColumn
              DataBinding.FieldName = 'id_alop_angajamente'
              Visible = False
            end
            object cxGrid3DBTableView1numar: TcxGridDBColumn
              DataBinding.FieldName = 'numar'
              Width = 36
            end
            object cxGrid3DBTableView1data_emitere: TcxGridDBColumn
              DataBinding.FieldName = 'data_emitere'
              PropertiesClassName = 'TcxDateEditProperties'
              Width = 79
            end
            object cxGrid3DBTableView1id_departament: TcxGridDBColumn
              DataBinding.FieldName = 'id_departament'
              PropertiesClassName = 'TcxLookupComboBoxProperties'
              Properties.KeyFieldNames = 'ID_REPARTITORI'
              Properties.ListColumns = <
                item
                  FieldName = 'NUME'
                end>
              Properties.ListSource = DTREP
              Width = 137
            end
            object cxGrid3DBTableView1departament: TcxGridDBColumn
              DataBinding.FieldName = 'departament'
              Width = 142
            end
            object cxGrid3DBTableView1documente_lichidate: TcxGridDBColumn
              DataBinding.FieldName = 'documente_lichidate'
              Width = 200
            end
            object cxGrid3DBTableView1suma_datorata: TcxGridDBColumn
              DataBinding.FieldName = 'suma_datorata'
              Width = 88
            end
            object cxGrid3DBTableView1suma_avans: TcxGridDBColumn
              DataBinding.FieldName = 'suma_avans'
              Width = 96
            end
            object cxGrid3DBTableView1suma_plata: TcxGridDBColumn
              DataBinding.FieldName = 'suma_plata'
              Width = 104
            end
            object cxGrid3DBTableView1id_repartitori: TcxGridDBColumn
              DataBinding.FieldName = 'id_repartitori'
              PropertiesClassName = 'TcxLookupComboBoxProperties'
              Properties.KeyFieldNames = 'ID_REPARTITORI'
              Properties.ListColumns = <
                item
                  FieldName = 'NUME'
                end>
              Properties.ListSource = DTREP
              Width = 110
            end
          end
          object cxGrid3Level1: TcxGridLevel
            GridView = cxGrid3DBTableView1
          end
        end
      end
    end
    object tabPlata: TcxTabSheet
      Caption = 'Plata'
      ImageIndex = 3
      object GroupBox3: TGroupBox
        Left = 0
        Top = 0
        Width = 918
        Height = 338
        Align = alClient
        Caption = 'Plata'
        TabOrder = 0
        object cxGrid4: TcxGrid
          Left = 2
          Top = 15
          Width = 914
          Height = 321
          Align = alClient
          TabOrder = 0
          object cxGrid4DBTableView1: TcxGridDBTableView
            NavigatorButtons.ConfirmDelete = False
            DataController.DataSource = DTPLata
            DataController.Summary.DefaultGroupSummaryItems = <>
            DataController.Summary.FooterSummaryItems = <>
            DataController.Summary.SummaryGroups = <>
            object cxGrid4DBTableView1tipdoc: TcxGridDBColumn
              DataBinding.FieldName = 'tipdoc'
            end
            object cxGrid4DBTableView1nrdoc: TcxGridDBColumn
              DataBinding.FieldName = 'nrdoc'
            end
            object cxGrid4DBTableView1explicatie: TcxGridDBColumn
              DataBinding.FieldName = 'explicatie'
              Width = 175
            end
            object cxGrid4DBTableView1codgest: TcxGridDBColumn
              DataBinding.FieldName = 'codgest'
              PropertiesClassName = 'TcxLookupComboBoxProperties'
              Properties.KeyFieldNames = 'ID_REPARTITORI'
              Properties.ListColumns = <
                item
                  FieldName = 'NUME'
                end>
              Properties.ListSource = DTREP
              Width = 275
            end
            object cxGrid4DBTableView1cont_csp: TcxGridDBColumn
              DataBinding.FieldName = 'cont_csp'
              Width = 140
            end
            object cxGrid4DBTableView1suma: TcxGridDBColumn
              DataBinding.FieldName = 'suma'
              Width = 118
            end
            object cxGrid4DBTableView1Column1: TcxGridDBColumn
              DataBinding.FieldName = 'data'
              PropertiesClassName = 'TcxDateEditProperties'
              Properties.ShowTime = False
            end
          end
          object cxGrid4Level1: TcxGridLevel
            GridView = cxGrid4DBTableView1
          end
        end
      end
    end
  end
  object aQ: TZQuery
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    LockType = ltReadOnly
    Params = <>
    SQL.Strings = (
      
        'select a.id_alop_angajamente, a.data_emitere, a.id_departament, ' +
        'a.numar, a.id_lst_repartitori, a.cod_functional, '
      
        'b.cod_economic, b.id_alop_angajamente_defalcare, b.aprobate, b.t' +
        'otal_angajate, b.disponibil, b.angajat, b.ramas_de_angajat '
      'from alop_angajamente a '
      
        'join alop_angajamente_defalcare b on (a.id_alop_angajamente=b.id' +
        '_alop_angajamente)'
      
        'order by a.id_lst_repartitori, a.data_emitere,a.id_alop_angajame' +
        'nte')
    Left = 744
    Top = 120
  end
  object DataSource1: TDataSource
    DataSet = aQ
    Left = 776
    Top = 121
  end
  object QLichidare: TZQuery
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    LockType = ltReadOnly
    Params = <
      item
        Name = 'id_angajamente_defalcare'
        DataType = ftInteger
        Size = -1
        Value = 0
      end>
    SQL.Strings = (
      
        'select  a.valoare_receptie_tva as suma , a.cod_economic, a.cod_f' +
        'unctional,'
      
        'b.data_docum, b.Nr_docum, b.id_primitor, b.id_predator, (select ' +
        'cod_docum from gest_tip_docum where id_gest_tip_docum = b.id_ges' +
        't_tip_docum) as tipdoc, a.id_gest_itemsi, '
      'a.id_angajamente_defalcare, '
      'a.id_gest_docum'
      'from gest_itemsi a'
      
        'join gest_docum b on (a.id_gest_docum=b.id_gest_docum) and (a.st' +
        'are=1) and (b.stare=1)'
      'and a.id_angajamente_defalcare =:id_angajamente_defalcare')
    Left = 880
    Top = 136
  end
  object DTLichidari: TDataSource
    DataSet = QLichidare
    Left = 848
    Top = 136
  end
  object QOrdonantare: TZQuery
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    LockType = ltReadOnly
    Params = <
      item
        Name = 'id_alop_angajamente'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      
        'select id_alop_angajamente, numar, data_emitere, id_departament,' +
        ' departament,'
      
        'documente_lichidate, suma_datorata, suma_avans, suma_plata, id_r' +
        'epartitori '
      'from alop_ordonantare '
      'where id_alop_angajamente=:id_alop_angajamente')
    Left = 736
    Top = 168
  end
  object DTOrdonantare: TDataSource
    DataSet = QOrdonantare
    Left = 768
    Top = 168
  end
  object QPlata: TZQuery
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    LockType = ltReadOnly
    Params = <
      item
        Name = 'id_gest_itemsi'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = 0
      end>
    SQL.Strings = (
      
        'select c.tipdoc, c.nrdoc, c.explicatie, c.codgest, c.cont_csp, a' +
        '.suma, c.data, c.cod '
      'from gest_defalcare_decontari a'
      
        'join gest_decontari b on (a.id_gest_decontari=b.id_gest_decontar' +
        'i)'
      'join bregistru c on (b.id_bregistru=c.cod)'
      'where a.id_gest_itemsi =:id_gest_itemsi')
    Left = 600
    Top = 144
  end
  object DTPLata: TDataSource
    DataSet = QPlata
    Left = 632
    Top = 144
  end
  object ADOQuery1: TZQuery
    Connection = frmData.dbContabilitate
    CursorType = ctStatic
    Params = <>
    SQL.Strings = (
      'SELECT * FROM REPARTITORI'
      'ORDER BY NUME')
    Left = 600
    Top = 104
  end
  object DTREP: TDataSource
    DataSet = ADOQuery1
    Left = 632
    Top = 104
  end
end
