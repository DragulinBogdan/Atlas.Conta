object frmCBPozitie: TfrmCBPozitie
  Left = 353
  Top = 131
  AutoScroll = False
  Caption = 'Pozitie plata/Incasare'
  ClientHeight = 433
  ClientWidth = 718
  Color = 14737632
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object PageRegistru: TcxPageControl
    Left = 0
    Top = 0
    Width = 718
    Height = 433
    ActivePage = tabGeneral
    Align = alClient
    Style = 9
    TabOrder = 0
    TabSlants.Kind = skCutCorner
    TabSlants.Positions = [spLeft, spRight]
    ClientRectBottom = 433
    ClientRectRight = 718
    ClientRectTop = 20
    object tabGeneral: TcxTabSheet
      Caption = 'Detalii Primare'
      Color = 14737632
      ImageIndex = 0
      ParentColor = False
      object gbInfo: TcxGroupBox
        Left = 0
        Top = 120
        Align = alTop
        Caption = 'Detalii primare'
        TabOrder = 1
        Height = 130
        Width = 718
        object Label1: TLabel
          Left = 16
          Top = 15
          Width = 67
          Height = 13
          Caption = 'Tip Document'
        end
        object Label2: TLabel
          Left = 152
          Top = 15
          Width = 34
          Height = 13
          Caption = 'Nr Doc'
        end
        object Label3: TLabel
          Left = 248
          Top = 15
          Width = 46
          Height = 13
          Caption = 'Data Doc'
        end
        object Label4: TLabel
          Left = 22
          Top = 47
          Width = 36
          Height = 13
          Caption = 'Valoare'
        end
        object Label6: TLabel
          Tag = -15
          Left = 320
          Top = 47
          Width = 83
          Height = 13
          Caption = 'Data Curs Valutar'
        end
        object Label7: TLabel
          Tag = -15
          Left = 232
          Top = 47
          Width = 48
          Height = 13
          Caption = 'Tip Valuta'
        end
        object Label8: TLabel
          Tag = -15
          Left = 600
          Top = 47
          Width = 69
          Height = 13
          Caption = 'Valoare Valuta'
        end
        object Label9: TLabel
          Left = 16
          Top = 87
          Width = 54
          Height = 13
          Caption = 'Explicatie : '
        end
        object Label5: TLabel
          Tag = -15
          Left = 464
          Top = 47
          Width = 59
          Height = 13
          Caption = 'Curs Schimb'
        end
        object edtTipDoc: TcxDBImageComboBox
          Left = 8
          Top = 27
          DataBinding.DataField = 'ID_CB_TIP_DOCUM'
          DataBinding.DataSource = DTRegistru
          Properties.Items = <>
          TabOrder = 0
          Width = 105
        end
        object edtNrDoc: TcxDBTextEdit
          Left = 128
          Top = 27
          DataBinding.DataField = 'NR_DOCUM'
          DataBinding.DataSource = DTRegistru
          TabOrder = 1
          Width = 89
        end
        object edtDataDoc: TcxDBDateEdit
          Left = 240
          Top = 27
          DataBinding.DataField = 'DATA_DOCUM'
          DataBinding.DataSource = DTRegistru
          Properties.SaveTime = False
          Properties.ShowTime = False
          TabOrder = 2
          Width = 121
        end
        object edtEsteValuta: TcxDBCheckBox
          Tag = -15
          Left = 144
          Top = 63
          Caption = 'Este Valuta'
          DataBinding.DataField = 'ESTE_VALUTA'
          DataBinding.DataSource = DTRegistru
          Properties.NullStyle = nssUnchecked
          TabOrder = 3
          Width = 81
        end
        object edtDataCursValutar: TcxDBDateEdit
          Tag = -15
          Left = 312
          Top = 63
          DataBinding.DataField = 'CURS_SCHIMB'
          DataBinding.DataSource = DTRegistru
          Properties.SaveTime = False
          Properties.ShowTime = False
          TabOrder = 4
          Width = 113
        end
        object edtTipValuta: TcxDBImageComboBox
          Tag = -15
          Left = 224
          Top = 63
          DataBinding.DataField = 'ID_VALUTA'
          DataBinding.DataSource = DTRegistru
          Properties.Items = <>
          TabOrder = 5
          Width = 81
        end
        object edtValoareValuta: TcxDBCurrencyEdit
          Tag = -15
          Left = 580
          Top = 63
          DataBinding.DataField = 'VALOARE_VALUTA'
          DataBinding.DataSource = DTRegistru
          TabOrder = 6
          Width = 121
        end
        object edtExplicatie: TcxDBMemo
          Left = 80
          Top = 87
          DataBinding.DataField = 'EXPLICATIE'
          DataBinding.DataSource = DTRegistru
          TabOrder = 7
          Height = 34
          Width = 625
        end
        object edValoare: TcxDBCurrencyEdit
          Left = 8
          Top = 62
          DataBinding.DataField = 'VALOARE'
          DataBinding.DataSource = DTRegistru
          TabOrder = 8
          Width = 121
        end
        object edtCursSchimb: TcxDBCurrencyEdit
          Tag = -15
          Left = 444
          Top = 63
          DataBinding.DataField = 'CURS_SCHIMB'
          DataBinding.DataSource = DTRegistru
          TabOrder = 9
          Width = 121
        end
      end
      object cxGroupBox1: TcxGroupBox
        Left = 0
        Top = 65
        Align = alTop
        Caption = 'Repartitie Contabila'
        TabOrder = 2
        Height = 55
        Width = 718
        object Label10: TLabel
          Left = 16
          Top = 13
          Width = 63
          Height = 13
          Caption = 'Cont Contabil'
        end
        object Label11: TLabel
          Left = 160
          Top = 13
          Width = 46
          Height = 13
          Caption = 'Repartitor'
        end
        object Label12: TLabel
          Left = 472
          Top = 13
          Width = 100
          Height = 13
          Caption = 'Document de lichidat'
        end
        object edtCont: TcxDBPopupEdit
          Left = 16
          Top = 26
          DataBinding.DataSource = DTContabil
          Properties.MaxLength = 0
          TabOrder = 0
          Width = 121
        end
        object edtRep: TcxDBPopupEdit
          Left = 160
          Top = 26
          DataBinding.DataSource = DTContabil
          Properties.MaxLength = 0
          TabOrder = 1
          Width = 297
        end
        object edtFCT: TcxDBPopupEdit
          Left = 472
          Top = 26
          DataBinding.DataField = 'EXPLICATIE'
          DataBinding.DataSource = DTContabil
          Properties.MaxLength = 0
          TabOrder = 2
          Width = 233
        end
      end
      object cxGridConta: TcxGrid
        Left = 0
        Top = 280
        Width = 718
        Height = 133
        Align = alClient
        TabOrder = 3
        LookAndFeel.Kind = lfOffice11
        object GridConta: TcxGridDBTableView
          NavigatorButtons.ConfirmDelete = False
          DataController.DataSource = DTContabil
          DataController.KeyFieldNames = 'ID_CB_CONTABIL'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          object GridContaID_CB_CONTABIL: TcxGridDBColumn
            Caption = 'Id'
            DataBinding.FieldName = 'ID_CB_CONTABIL'
            Visible = False
          end
          object GridContaID_CB_REGISTRU: TcxGridDBColumn
            Caption = 'CodReg'
            DataBinding.FieldName = 'ID_CB_REGISTRU'
            Visible = False
          end
          object GridContaESTE_PLATA: TcxGridDBColumn
            Caption = 'Este Plata'
            DataBinding.FieldName = 'ESTE_PLATA'
            Visible = False
          end
          object GridContaEXPLICATIE: TcxGridDBColumn
            Caption = 'Explicatie'
            DataBinding.FieldName = 'EXPLICATIE'
            Width = 131
          end
          object GridContaVALOARE: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'VALOARE'
            Width = 110
          end
          object GridContaCONT_DEBIT: TcxGridDBColumn
            Caption = 'Debit'
            DataBinding.FieldName = 'CONT_DEBIT'
            Width = 54
          end
          object GridContaREPARTITOR_DEBIT: TcxGridDBColumn
            Caption = 'Rep Debit'
            DataBinding.FieldName = 'REPARTITOR_DEBIT'
            Width = 49
          end
          object GridContaCONT_CREDIT: TcxGridDBColumn
            Caption = 'Credit'
            DataBinding.FieldName = 'CONT_CREDIT'
            Width = 51
          end
          object GridContaREPARTITOR_CREDIT: TcxGridDBColumn
            Caption = 'Rep Credit'
            DataBinding.FieldName = 'REPARTITOR_CREDIT'
            Width = 76
          end
          object GridContaCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Cod Functional'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            Width = 105
          end
          object GridContaCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Cod Economic'
            DataBinding.FieldName = 'COD_ECONOMIC'
            Width = 140
          end
          object GridContaID_UTILIZATOR: TcxGridDBColumn
            Caption = 'Id Utilizator'
            DataBinding.FieldName = 'ID_UTILIZATOR'
            Visible = False
            VisibleForCustomization = False
          end
          object GridContaSTARE: TcxGridDBColumn
            Caption = 'Stare'
            DataBinding.FieldName = 'STARE'
            Visible = False
            VisibleForCustomization = False
          end
          object GridContaID_CNOTE: TcxGridDBColumn
            Caption = 'IdNote'
            DataBinding.FieldName = 'ID_CNOTE'
            Visible = False
          end
          object GridContaID_TCV: TcxGridDBColumn
            Caption = 'IdTcv'
            DataBinding.FieldName = 'ID_TCV'
            Visible = False
          end
          object GridContaID_CASA: TcxGridDBColumn
            Caption = 'IdCasa'
            DataBinding.FieldName = 'ID_CASA'
            Visible = False
          end
        end
        object GridContaL: TcxGridLevel
          GridView = GridConta
        end
      end
      object gbInfoEntitate: TcxGroupBox
        Left = 0
        Top = 0
        Align = alTop
        Caption = 'Entitate'
        TabOrder = 0
        Height = 65
        Width = 718
        object Label13: TLabel
          Left = 136
          Top = 21
          Width = 85
          Height = 13
          Caption = 'Cont Casa/Banca'
        end
        object Label14: TLabel
          Left = 264
          Top = 21
          Width = 45
          Height = 13
          Caption = 'Denumire'
        end
        object edContCasa: TcxDBPopupEdit
          Left = 136
          Top = 36
          DataBinding.DataField = 'ID_CB_ENTITATI'
          DataBinding.DataSource = DTRegistru
          Properties.MaxLength = 0
          TabOrder = 0
          Width = 121
        end
        object edDenCasa: TcxDBImageComboBox
          Left = 264
          Top = 36
          DataBinding.DataField = 'ID_CB_ENTITATI'
          DataBinding.DataSource = DTRegistru
          Properties.Items = <>
          TabOrder = 1
          Width = 441
        end
        object edtEstePlata: TcxDBCheckBox
          Left = 8
          Top = 12
          Caption = 'Plata'
          DataBinding.DataField = 'ESTE_PLATA'
          DataBinding.DataSource = DTRegistru
          Properties.NullStyle = nssUnchecked
          TabOrder = 2
          Width = 57
        end
        object edtEsteIncasare: TcxDBCheckBox
          Left = 8
          Top = 32
          Caption = 'Incasare'
          DataBinding.DataField = 'ESTE_PLATA'
          DataBinding.DataSource = DTRegistru
          Properties.DisplayChecked = 'False'
          Properties.DisplayUnchecked = 'True'
          Properties.NullStyle = nssUnchecked
          Properties.ValueChecked = False
          Properties.ValueUnchecked = 'True'
          TabOrder = 3
          Width = 121
        end
      end
      object pnBottom: TPanel
        Left = 0
        Top = 250
        Width = 718
        Height = 30
        Align = alTop
        BevelInner = bvLowered
        Color = 14737632
        TabOrder = 4
        object btnAdd: TcxButton
          Left = 96
          Top = 4
          Width = 75
          Height = 22
          Caption = 'Adauga'
          TabOrder = 0
        end
        object cxButton2: TcxButton
          Left = 240
          Top = 4
          Width = 75
          Height = 22
          Caption = 'cxButton1'
          TabOrder = 1
        end
        object cxButton3: TcxButton
          Left = 536
          Top = 0
          Width = 75
          Height = 22
          Caption = 'cxButton1'
          TabOrder = 2
        end
      end
    end
    object tabDocConex: TcxTabSheet
      Caption = 'Documente Conexe'
      ImageIndex = 1
    end
    object tabContabil: TcxTabSheet
      Caption = 'Contabilitate'
      ImageIndex = 2
    end
  end
  object qryContabil: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = qryContabilNewRecord
    SQL.Strings = (
      
        'SELECT * FROM  CB_CONTABIL WHERE ID_CB_REGISTRU = :ID_CB_REGISTR' +
        'U')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_CB_REGISTRU'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 56
    Top = 408
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_CB_REGISTRU'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object qryRegistru: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM CB_REGISTRU WHERE ID_CB_REGISTRU = :ID_CB_REGISTRU'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_CB_REGISTRU'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 64
    Top = 376
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_CB_REGISTRU'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTContabil: TDataSource
    DataSet = qryContabil
    Left = 24
    Top = 408
  end
  object DTRegistru: TDataSource
    DataSet = qryRegistru
    Left = 32
    Top = 376
  end
end
