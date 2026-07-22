object frmAlopAngDisponibil: TfrmAlopAngDisponibil
  Left = 273
  Top = 155
  Caption = 'Distributie Angajament'
  ClientHeight = 668
  ClientWidth = 1120
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnShow = FormShow
  DesignSize = (
    1120
    668)
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 648
    Top = 40
    Width = 32
    Height = 13
    Caption = 'Label1'
  end
  object Panel1: TPanel
    Left = 0
    Top = 627
    Width = 1120
    Height = 41
    Align = alBottom
    TabOrder = 0
    object cxButton1: TcxButton
      Left = 885
      Top = 6
      Width = 75
      Height = 27
      Align = alCustom
      Anchors = [akRight, akBottom]
      Caption = 'Salvare'
      ModalResult = 1
      TabOrder = 0
      OnClick = cxButton1Click
    end
    object cxButton2: TcxButton
      Left = 985
      Top = 6
      Width = 75
      Height = 27
      Align = alCustom
      Anchors = [akRight, akBottom]
      Caption = 'Stergere'
      ModalResult = 2
      TabOrder = 1
    end
  end
  object Panel2: TPanel
    Left = 0
    Top = 0
    Width = 1120
    Height = 177
    Align = alTop
    Caption = 'Panel2'
    TabOrder = 1
    object grDetaliiAngajament: TcxGroupBox
      Left = 1
      Top = 1
      Align = alClient
      Caption = 'Detalii Angajament Curent'
      TabOrder = 0
      Height = 175
      Width = 747
      object Panel4: TPanel
        Left = 2
        Top = 18
        Width = 743
        Height = 41
        Align = alTop
        TabOrder = 0
        object Button1: TButton
          Left = 645
          Top = 5
          Width = 81
          Height = 30
          Align = alCustom
          Anchors = [akRight, akBottom]
          Caption = 'Cauta'
          TabOrder = 0
          Visible = False
          OnClick = Button1Click
        end
      end
      object griddefalcare: TcxGrid
        Left = 2
        Top = 59
        Width = 743
        Height = 114
        Align = alClient
        TabOrder = 1
        object griddefalcareDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = dtAngDefalcare
          DataController.KeyFieldNames = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          object griddefalcareDBTableView1COD_ECONOMIC: TcxGridDBColumn
            Caption = 'Cod Economic'
            DataBinding.FieldName = 'COD_ECONOMIC'
            Options.Editing = False
            Width = 122
          end
          object griddefalcareDBTableView1TOTAL_ANGAJATE: TcxGridDBColumn
            DataBinding.FieldName = 'TOTAL_ANGAJATE'
            Visible = False
            Width = 168
          end
          object griddefalcareDBTableView1DISPONIBIL: TcxGridDBColumn
            Caption = 'Disponibil'
            DataBinding.FieldName = 'DISPONIBIL'
            Width = 76
          end
          object griddefalcareDBTableView1ANGAJAT_VALUTA: TcxGridDBColumn
            Caption = 'Angajat'
            DataBinding.FieldName = 'ANGAJAT_VALUTA'
            Width = 97
          end
          object griddefalcareDBTableView1ANGAJAT: TcxGridDBColumn
            DataBinding.FieldName = 'ANGAJAT'
            Visible = False
            Width = 144
          end
          object griddefalcareDBTableView1RAMAS_DE_ANGAJAT: TcxGridDBColumn
            Caption = 'Ramas de angajat'
            DataBinding.FieldName = 'RAMAS_DE_ANGAJAT'
            Width = 137
          end
          object griddefalcareDBTableView1ID_LST_REPARTITORI: TcxGridDBColumn
            Caption = 'Beneficiari'
            DataBinding.FieldName = 'ID_LST_REPARTITORI'
            Width = 108
          end
          object griddefalcareDBTableView1data_curs: TcxGridDBColumn
            Caption = 'Data Emitere'
            DataBinding.FieldName = 'data_curs'
            Width = 201
          end
        end
        object niveldefalcare: TcxGridLevel
          GridView = griddefalcareDBTableView1
        end
      end
    end
    object cxGroupBox1: TcxGroupBox
      Left = 756
      Top = 1
      Align = alRight
      Caption = 'Total pe cod economic'
      TabOrder = 1
      Height = 175
      Width = 363
      object StaticText2: TStaticText
        Left = 45
        Top = 104
        Width = 117
        Height = 17
        Caption = 'Total Ramas de angajat'
        TabOrder = 0
      end
      object edDisponibil: TcxDBCurrencyEdit
        Left = 200
        Top = 55
        DataBinding.DataField = 'ANGAJAT_VALUTA'
        DataBinding.DataSource = dtAngDefalcare
        Enabled = False
        Properties.Alignment.Horz = taRightJustify
        Properties.ReadOnly = True
        TabOrder = 1
        Width = 105
      end
      object edRamasDeAngajat: TcxDBCurrencyEdit
        Left = 200
        Top = 100
        DataBinding.DataField = 'RAMAS_DE_ANGAJAT'
        DataBinding.DataSource = dtAngDefalcare
        Enabled = False
        Properties.Alignment.Horz = taRightJustify
        Properties.ReadOnly = True
        TabOrder = 2
        Width = 105
      end
    end
    object cxSplitter2: TcxSplitter
      Left = 748
      Top = 1
      Width = 8
      Height = 175
      AlignSplitter = salRight
    end
  end
  object cxSplitter1: TcxSplitter
    Left = 0
    Top = 177
    Width = 1120
    Height = 8
    AlignSplitter = salTop
  end
  object Panel3: TPanel
    Left = 0
    Top = 185
    Width = 1120
    Height = 442
    Align = alClient
    Caption = 'Panel3'
    TabOrder = 3
    object cxGrid1: TcxGrid
      Left = 1
      Top = 1
      Width = 1118
      Height = 440
      Align = alClient
      TabOrder = 0
      object cxGrid1DBTableView1: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        DataController.DataSource = dtDefalcareSubpoz
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsView.ColumnAutoWidth = True
        object cxGrid1DBTableView1ID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn
          DataBinding.FieldName = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
          Width = 212
        end
        object cxGrid1DBTableView1COD_ECONOMIC: TcxGridDBColumn
          Caption = 'Cod Economic'
          DataBinding.FieldName = 'COD_ECONOMIC'
          Options.Editing = False
          Width = 124
        end
        object cxGrid1DBTableView1TOTAL_ANGAJATE: TcxGridDBColumn
          DataBinding.FieldName = 'TOTAL_ANGAJATE'
          Visible = False
          Width = 129
        end
        object cxGrid1DBTableView1DISPONIBIL: TcxGridDBColumn
          DataBinding.FieldName = 'DISPONIBIL'
          Visible = False
          Width = 85
        end
        object cxGrid1DBTableView1ANGAJAT_VALUTA: TcxGridDBColumn
          Caption = 'Suma angajata'
          DataBinding.FieldName = 'ANGAJAT_VALUTA'
          Width = 143
        end
        object cxGrid1DBTableView1ANGAJAT: TcxGridDBColumn
          DataBinding.FieldName = 'ANGAJAT'
          Visible = False
          Width = 114
        end
        object cxGrid1DBTableView1RAMAS_DE_ANGAJAT: TcxGridDBColumn
          Caption = 'Ramas de angajat'
          DataBinding.FieldName = 'RAMAS_DE_ANGAJAT'
          Width = 141
        end
        object cxGrid1DBTableView1data_curs: TcxGridDBColumn
          Caption = 'Data Emitere'
          DataBinding.FieldName = 'data_curs'
        end
        object cxGrid1DBTableView1ID_LST_REPARTITORI: TcxGridDBColumn
          Caption = 'Beneficiari'
          DataBinding.FieldName = 'ID_LST_REPARTITORI'
          Width = 133
        end
        object cxGrid1DBTableView1ID_POZITIE_PARINTE: TcxGridDBColumn
          DataBinding.FieldName = 'ID_POZITIE_PARINTE'
        end
      end
      object cxGrid1Level1: TcxGridLevel
        GridView = cxGrid1DBTableView1
      end
    end
  end
  object cxLabel1: TcxLabel
    Left = 16
    Top = 34
    Caption = 'Nr. angajament'
  end
  object txtCautareNumar: TcxTextEdit
    Left = 98
    Top = 30
    TabOrder = 5
    OnKeyDown = txtCautareNumarKeyDown
    Width = 121
  end
  object StaticText1: TStaticText
    Left = 801
    Top = 60
    Width = 103
    Height = 17
    Anchors = [akTop, akRight]
    Caption = 'Total Disponibil Initial'
    TabOrder = 6
  end
  object btnAdaugaSubpozitie: TcxButton
    Left = 36
    Top = 633
    Width = 112
    Height = 27
    Align = alCustom
    Anchors = [akLeft, akBottom]
    Caption = 'Adauga subpozitie'
    TabOrder = 7
    OnClick = btnAdaugaSubpozitieClick
  end
  object btnStergeSubpozitie: TcxButton
    Left = 170
    Top = 632
    Width = 150
    Height = 28
    Align = alCustom
    Anchors = [akLeft, akBottom]
    Caption = 'Sterge subpozitie'
    TabOrder = 8
    OnClick = btnStergeSubpozitieClick
  end
  object dtAngajamente: TDataSource
    DataSet = qryAngajamente
    Left = 48
    Top = 320
  end
  object qryAngajamente: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM alop_angajamente WHERE 1 = 0')
    Params = <>
    Left = 136
    Top = 320
    object qryAngajamenteID_ALOP_ANGAJAMENTE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE'
      ReadOnly = True
    end
    object qryAngajamenteID_UTILIZATORI: TIntegerField
      FieldName = 'ID_UTILIZATORI'
    end
    object qryAngajamenteDATA_EMITERE: TDateTimeField
      FieldName = 'DATA_EMITERE'
    end
    object qryAngajamenteID_DEPARTAMENT: TIntegerField
      FieldName = 'ID_DEPARTAMENT'
    end
    object qryAngajamenteNUMAR: TStringField
      FieldName = 'NUMAR'
      Size = 50
    end
    object qryAngajamenteSCOPUL: TStringField
      FieldName = 'SCOPUL'
      Size = 255
    end
    object qryAngajamenteID_LST_REPARTITORI: TIntegerField
      FieldName = 'ID_LST_REPARTITORI'
    end
    object qryAngajamenteVALIDAT: TIntegerField
      FieldName = 'VALIDAT'
    end
    object qryAngajamenteTIME_IMPORT: TBlobField
      FieldName = 'TIME_IMPORT'
    end
    object qryAngajamenteCLASA_FUNCTIONALA: TStringField
      FieldName = 'CLASA_FUNCTIONALA'
      Size = 254
    end
    object qryAngajamenteTIP_ANGAJAMENT: TIntegerField
      FieldName = 'TIP_ANGAJAMENT'
    end
    object qryAngajamenteRECTIFICARE: TBooleanField
      FieldName = 'RECTIFICARE'
    end
    object qryAngajamenteID_CONTRACT: TIntegerField
      FieldName = 'ID_CONTRACT'
    end
    object qryAngajamenteID_ACT_ADITIONAL: TIntegerField
      FieldName = 'ID_ACT_ADITIONAL'
    end
    object qryAngajamenteESTE_INCHIS: TBooleanField
      FieldName = 'ESTE_INCHIS'
    end
    object qryAngajamenteID_PARINTE: TIntegerField
      FieldName = 'ID_PARINTE'
    end
    object qryAngajamenteSTARE: TIntegerField
      FieldName = 'STARE'
    end
    object qryAngajamenteCOD_FUNCTIONAL: TStringField
      FieldName = 'COD_FUNCTIONAL'
      Size = 100
    end
    object qryAngajamenteDATA_OPERARE: TDateTimeField
      FieldName = 'DATA_OPERARE'
    end
    object qryAngajamenteDATA: TDateTimeField
      FieldName = 'DATA'
    end
    object qryAngajamenteDATA_ANULARE: TDateTimeField
      FieldName = 'DATA_ANULARE'
    end
    object qryAngajamenteID_ANALITIC: TIntegerField
      FieldName = 'ID_ANALITIC'
    end
    object qryAngajamenteCOD_ECRAN: TStringField
      FieldName = 'COD_ECRAN'
      ReadOnly = True
      Size = 254
    end
    object qryAngajamenteDATA_STERGERE: TDateTimeField
      FieldName = 'DATA_STERGERE'
    end
    object qryAngajamenteUSER_STERGERE: TIntegerField
      FieldName = 'USER_STERGERE'
    end
    object qryAngajamenteCoduriEconomice: TStringField
      FieldName = 'CoduriEconomice'
      Size = 255
    end
    object qryAngajamenteProiect: TStringField
      FieldName = 'Proiect'
      Size = 255
    end
    object qryAngajamenteNR_CONTRACT: TStringField
      FieldName = 'NR_CONTRACT'
      Size = 255
    end
    object qryAngajamenteDATA_CONTRACT: TDateTimeField
      FieldName = 'DATA_CONTRACT'
    end
    object qryAngajamenteMAN_NR: TIntegerField
      FieldName = 'MAN_NR'
    end
    object qryAngajamentenr_proiect: TStringField
      FieldName = 'nr_proiect'
      Size = 50
    end
    object qryAngajamentevizat_trezorerie: TBooleanField
      FieldName = 'vizat_trezorerie'
    end
    object qryAngajamenteid_bg_versiune: TIntegerField
      FieldName = 'id_bg_versiune'
    end
    object qryAngajamentenecesita_viza_trezorerie: TBooleanField
      FieldName = 'necesita_viza_trezorerie'
    end
    object qryAngajamentedata_viza_trezorerie: TDateTimeField
      FieldName = 'data_viza_trezorerie'
    end
    object qryAngajamenteID_ANGAJAMENT_LEGAL: TIntegerField
      FieldName = 'ID_ANGAJAMENT_LEGAL'
    end
    object qryAngajamenteman_id_orig: TIntegerField
      FieldName = 'man_id_orig'
    end
    object qryAngajamenteman_id: TIntegerField
      FieldName = 'man_id'
    end
    object qryAngajamenteDATA_INTRODUCERE: TDateTimeField
      FieldName = 'DATA_INTRODUCERE'
    end
    object qryAngajamenteref_One_TipProgram: TIntegerField
      FieldName = 'ref_One_TipProgram'
    end
    object qryAngajamenteref_One_Contract: TIntegerField
      FieldName = 'ref_One_Contract'
    end
    object qryAngajamentesumaContract: TFloatField
      FieldName = 'sumaContract'
    end
    object qryAngajamenteman_den: TStringField
      FieldName = 'man_den'
      Size = 100
    end
    object qryAngajamentenr_dosar: TStringField
      FieldName = 'nr_dosar'
      Size = 64
    end
    object qryAngajamentedata_dosar: TDateTimeField
      FieldName = 'data_dosar'
    end
    object qryAngajamenterefDosar: TIntegerField
      FieldName = 'refDosar'
    end
    object qryAngajamenteSoldPrecedent: TBooleanField
      FieldName = 'SoldPrecedent'
    end
  end
  object dtAngDefalcare: TDataSource
    DataSet = qryDefalcare
    Left = 48
    Top = 240
  end
  object qryDefalcare: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT d.*'
      'FROM alop_angajamente a'
      
        'JOIN alop_angajamente_defalcare d ON d.id_alop_angajamente = a.i' +
        'd_alop_angajamente'
      'WHERE a.numar = :numar'
      '  AND d.id_pozitie_parinte IS NULL')
    Params = <
      item
        DataType = ftUnknown
        Name = 'numar'
        ParamType = ptUnknown
      end>
    Left = 136
    Top = 240
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'numar'
        ParamType = ptUnknown
      end>
    object qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
      ReadOnly = True
    end
    object qryDefalcareID_ALOP_ANGAJAMENTE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE'
    end
    object qryDefalcareID_UTILIZATORI: TIntegerField
      FieldName = 'ID_UTILIZATORI'
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
    object qryDefalcareID_VALUTA: TIntegerField
      FieldName = 'ID_VALUTA'
    end
    object qryDefalcareANGAJAT_VALUTA: TFloatField
      FieldName = 'ANGAJAT_VALUTA'
    end
    object qryDefalcareCURS_VALUTAR: TFloatField
      FieldName = 'CURS_VALUTAR'
    end
    object qryDefalcareANGAJAT: TFloatField
      FieldName = 'ANGAJAT'
    end
    object qryDefalcareRAMAS_DE_ANGAJAT: TFloatField
      FieldName = 'RAMAS_DE_ANGAJAT'
    end
    object qryDefalcareVALIDAT: TIntegerField
      FieldName = 'VALIDAT'
    end
    object qryDefalcareDESCRIERE: TStringField
      FieldName = 'DESCRIERE'
      Size = 255
    end
    object qryDefalcareCLASA_ECONOMICA: TStringField
      FieldName = 'CLASA_ECONOMICA'
      Size = 254
    end
    object qryDefalcareID_ANALITIC: TIntegerField
      FieldName = 'ID_ANALITIC'
    end
    object qryDefalcareCOD_ECRAN: TStringField
      FieldName = 'COD_ECRAN'
      Size = 255
    end
    object qryDefalcareID_OI_UNITATI: TIntegerField
      FieldName = 'ID_OI_UNITATI'
    end
    object qryDefalcareID_OI_PROIECTE: TIntegerField
      FieldName = 'ID_OI_PROIECTE'
    end
    object qryDefalcareeste_credit_angajament: TBooleanField
      FieldName = 'este_credit_angajament'
    end
    object qryDefalcareeste_procentual: TBooleanField
      FieldName = 'este_procentual'
    end
    object qryDefalcareTIMESTAMP: TBlobField
      FieldName = 'TIMESTAMP'
    end
    object qryDefalcaredata_curs: TDateTimeField
      FieldName = 'data_curs'
    end
    object qryDefalcareprocProiect: TFloatField
      FieldName = 'procProiect'
    end
    object qryDefalcareangProiect: TFloatField
      FieldName = 'angProiect'
    end
    object qryDefalcareaprobatDocument: TFloatField
      FieldName = 'aprobatDocument'
    end
    object qryDefalcareangajatDocument: TFloatField
      FieldName = 'angajatDocument'
    end
    object qryDefalcaredisponibilDocument: TFloatField
      FieldName = 'disponibilDocument'
    end
    object qryDefalcareramasDocument: TFloatField
      FieldName = 'ramasDocument'
    end
    object qryDefalcareID_LST_REPARTITORI: TIntegerField
      FieldName = 'ID_LST_REPARTITORI'
    end
    object qryDefalcareSoldRectificat: TBooleanField
      FieldName = 'SoldRectificat'
    end
    object qryDefalcareID_POZITIE_PARINTE: TIntegerField
      FieldName = 'ID_POZITIE_PARINTE'
    end
  end
  object dtNewAng: TDataSource
    DataSet = tblNewAng
    Left = 304
    Top = 280
  end
  object tblNewAng: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 368
    Top = 280
  end
  object qryDefalcareSubpoz: TZQuery
    Connection = frmData.dbContabilitate
    AfterPost = qryDefalcareSubpozAfterPost
    AfterDelete = qryDefalcareSubpozAfterDelete
    SQL.Strings = (
      'SELECT *'
      'FROM alop_angajamente_defalcare'
      'WHERE id_pozitie_parinte = :id_parinte')
    Params = <
      item
        DataType = ftUnknown
        Name = 'id_parinte'
        ParamType = ptUnknown
      end>
    Left = 768
    Top = 312
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'id_parinte'
        ParamType = ptUnknown
      end>
    object qryDefalcareSubpozID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE_DEFALCARE'
      ReadOnly = True
    end
    object qryDefalcareSubpozID_ALOP_ANGAJAMENTE: TIntegerField
      FieldName = 'ID_ALOP_ANGAJAMENTE'
    end
    object qryDefalcareSubpozID_UTILIZATORI: TIntegerField
      FieldName = 'ID_UTILIZATORI'
    end
    object qryDefalcareSubpozCOD_ECONOMIC: TStringField
      FieldName = 'COD_ECONOMIC'
      Size = 100
    end
    object qryDefalcareSubpozAPROBATE: TFloatField
      FieldName = 'APROBATE'
    end
    object qryDefalcareSubpozTOTAL_ANGAJATE: TFloatField
      FieldName = 'TOTAL_ANGAJATE'
    end
    object qryDefalcareSubpozDISPONIBIL: TFloatField
      FieldName = 'DISPONIBIL'
    end
    object qryDefalcareSubpozID_VALUTA: TIntegerField
      FieldName = 'ID_VALUTA'
    end
    object qryDefalcareSubpozANGAJAT_VALUTA: TFloatField
      FieldName = 'ANGAJAT_VALUTA'
    end
    object qryDefalcareSubpozCURS_VALUTAR: TFloatField
      FieldName = 'CURS_VALUTAR'
    end
    object qryDefalcareSubpozANGAJAT: TFloatField
      FieldName = 'ANGAJAT'
    end
    object qryDefalcareSubpozRAMAS_DE_ANGAJAT: TFloatField
      FieldName = 'RAMAS_DE_ANGAJAT'
    end
    object qryDefalcareSubpozVALIDAT: TIntegerField
      FieldName = 'VALIDAT'
    end
    object qryDefalcareSubpozDESCRIERE: TStringField
      FieldName = 'DESCRIERE'
      Size = 255
    end
    object qryDefalcareSubpozCLASA_ECONOMICA: TStringField
      FieldName = 'CLASA_ECONOMICA'
      Size = 254
    end
    object qryDefalcareSubpozID_ANALITIC: TIntegerField
      FieldName = 'ID_ANALITIC'
    end
    object qryDefalcareSubpozCOD_ECRAN: TStringField
      FieldName = 'COD_ECRAN'
      Size = 255
    end
    object qryDefalcareSubpozID_OI_UNITATI: TIntegerField
      FieldName = 'ID_OI_UNITATI'
    end
    object qryDefalcareSubpozID_OI_PROIECTE: TIntegerField
      FieldName = 'ID_OI_PROIECTE'
    end
    object qryDefalcareSubpozeste_credit_angajament: TBooleanField
      FieldName = 'este_credit_angajament'
    end
    object qryDefalcareSubpozeste_procentual: TBooleanField
      FieldName = 'este_procentual'
    end
    object qryDefalcareSubpozTIMESTAMP: TBlobField
      FieldName = 'TIMESTAMP'
    end
    object qryDefalcareSubpozdata_curs: TDateTimeField
      FieldName = 'data_curs'
    end
    object qryDefalcareSubpozprocProiect: TFloatField
      FieldName = 'procProiect'
    end
    object qryDefalcareSubpozangProiect: TFloatField
      FieldName = 'angProiect'
    end
    object qryDefalcareSubpozaprobatDocument: TFloatField
      FieldName = 'aprobatDocument'
    end
    object qryDefalcareSubpozangajatDocument: TFloatField
      FieldName = 'angajatDocument'
    end
    object qryDefalcareSubpozdisponibilDocument: TFloatField
      FieldName = 'disponibilDocument'
    end
    object qryDefalcareSubpozramasDocument: TFloatField
      FieldName = 'ramasDocument'
    end
    object qryDefalcareSubpozID_LST_REPARTITORI: TIntegerField
      FieldName = 'ID_LST_REPARTITORI'
    end
    object qryDefalcareSubpozSoldRectificat: TBooleanField
      FieldName = 'SoldRectificat'
    end
    object qryDefalcareSubpozID_POZITIE_PARINTE: TIntegerField
      FieldName = 'ID_POZITIE_PARINTE'
    end
  end
  object dtDefalcareSubpoz: TDataSource
    DataSet = qryDefalcareSubpoz
    Left = 664
    Top = 320
  end
  object ZUpdateSQLSubpoz: TUpdateSQL
    ModifySQL.Strings = (
      'UPDATE alop_angajamente_defalcare SET'
      '  ANGAJAT_VALUTA = :ANGAJAT_VALUTA,'
      '  RAMAS_DE_ANGAJAT = :RAMAS_DE_ANGAJAT'
      'WHERE'
      
        '  ID_ALOP_ANGAJAMENTE_DEFALCARE = :OLD_ID_ALOP_ANGAJAMENTE_DEFAL' +
        'CARE')
    InsertSQL.Strings = (
      'INSERT INTO alop_angajamente_defalcare ('
      
        '  ID_ALOP_ANGAJAMENTE, ID_UTILIZATORI, ANGAJAT_VALUTA, RAMAS_DE_' +
        'ANGAJAT, ID_POZITIE_PARINTE'
      ')'
      'VALUES ('
      
        '  :ID_ALOP_ANGAJAMENTE, :ID_UTILIZATORI,  :ANGAJAT_VALUTA, :RAMA' +
        'S_DE_ANGAJAT, :ID_POZITIE_PARINTE'
      ')')
    DeleteSQL.Strings = (
      'DELETE FROM alop_angajamente_defalcare'
      
        'WHERE ID_ALOP_ANGAJAMENTE_DEFALCARE = :OLD_ID_ALOP_ANGAJAMENTE_D' +
        'EFALCARE')
    Left = 744
    Top = 408
  end
  object qryExec: TZQuery
    Connection = frmData.dbContabilitate
    Params = <>
    Left = 920
    Top = 368
  end
end
