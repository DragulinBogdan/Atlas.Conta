object frmSearchErrors: TfrmSearchErrors
  Left = 447
  Top = 430
  BorderIcons = []
  Caption = 'Localizare Erori'
  ClientHeight = 167
  ClientWidth = 469
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object TLabel
    Left = 16
    Top = 16
    Width = 3
    Height = 13
  end
  object ErrorPageControl: TcxTabControl
    Left = 0
    Top = 0
    Width = 469
    Height = 23
    Align = alTop
    ParentShowHint = False
    ShowHint = False
    TabOrder = 0
    Properties.CustomButtons.Buttons = <>
    Properties.HotTrack = True
    Properties.RaggedRight = True
    Properties.Style = 5
    Properties.TabIndex = 0
    Properties.Tabs.Strings = (
      'Setari      '
      'Validari          '
      'Transferuri       '
      'Nechilibrate         '
      'Duplicate         ')
    Properties.TabWidth = 100
    LookAndFeel.Kind = lfFlat
    LookAndFeel.NativeStyle = False
    OnChange = ErrorPageControlChange
    OnDrawTabEx = ErrorPageControlDrawTabEx
    ClientRectBottom = 24
    ClientRectRight = 469
    ClientRectTop = 24
  end
  object pnClient: TPanel
    Left = 0
    Top = 23
    Width = 469
    Height = 144
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    object pnList: TPanel
      Left = 0
      Top = 0
      Width = 469
      Height = 144
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 1
      object GridErrors: TdxDBGrid
        Left = 0
        Top = 0
        Width = 469
        Height = 144
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        KeyField = 'COD'
        SummaryGroups = <
          item
            DefaultGroup = True
            SummaryItems = <
              item
                ColumnName = 'GridErrorsCOD_CB'
                SummaryField = 'COD_CB'
                SummaryType = cstCount
              end>
            Name = 'GridErrorsSummaryGroup2'
          end>
        SummarySeparator = ', '
        Align = alClient
        TabOrder = 0
        OnDblClick = GridErrorsDblClick
        OnKeyDown = GridErrorsKeyDown
        DataSource = DTError
        Filter.FilterStatus = fsNone
        Filter.Criteria = {00000000}
        LookAndFeel = lfUltraFlat
        OptionsBehavior = [edgoAutoCopySelectedToClipboard, edgoAutoSearch, edgoAutoSort, edgoDragScroll, edgoTabThrough, edgoVertThrough]
        OptionsDB = [edgoCancelOnExit, edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoHeaderPanelHeight, edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
        ShowRowFooter = True
        object GridErrorsCOD: TdxDBGridMaskColumn
          Caption = 'Id'
          Visible = False
          Width = 49
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD'
        end
        object GridErrorsTIPDOC: TdxDBGridMaskColumn
          Caption = 'TipDoc'
          Width = 83
          BandIndex = 0
          RowIndex = 0
          FieldName = 'TIPDOC'
        end
        object GridErrorsNRDOC: TdxDBGridMaskColumn
          Caption = 'Nr.'
          Width = 76
          BandIndex = 0
          RowIndex = 0
          FieldName = 'NRDOC'
        end
        object GridErrorsCOD_CB: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Casa/Banca'
          MinWidth = 16
          Width = 82
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_CB'
          ShowDescription = True
        end
        object GridErrorsDATA: TdxDBGridDateColumn
          Caption = 'Data'
          Width = 59
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DATA'
        end
        object GridErrorsTIP_EROARE: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Eroare'
          MinWidth = 16
          Width = 118
          BandIndex = 0
          RowIndex = 0
          FieldName = 'TIP_EROARE'
          Descriptions.Strings = (
            'Inregistrari Nevalidate'
            'Transferuri Neincheiate'
            'Pozitii Nechilibrate')
          Images = ErrorList
          ImageIndexes.Strings = (
            '0'
            '1'
            '2')
          ShowDescription = True
          Values.Strings = (
            '1'
            '2'
            '3')
        end
      end
    end
    object pnSetari: TPanel
      Left = 0
      Top = 0
      Width = 469
      Height = 144
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 0
      DesignSize = (
        469
        144)
      object Label5: TLabel
        Left = 16
        Top = 16
        Width = 107
        Height = 13
        Caption = 'Perioada Cautare :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label6: TLabel
        Left = 26
        Top = 36
        Width = 62
        Height = 13
        Caption = 'Data Inceput'
      end
      object Label7: TLabel
        Left = 218
        Top = 36
        Width = 55
        Height = 13
        Caption = 'Data Sfarsit'
      end
      object Label8: TLabel
        Left = 16
        Top = 68
        Width = 114
        Height = 13
        Caption = 'Cautare pe o Casa :'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edStartDate: TdxDateEdit
        Left = 91
        Top = 32
        Width = 121
        Style.BorderStyle = xbsSingle
        TabOrder = 0
        Date = -700000.000000000000000000
        DateValidation = True
        UseEditMask = True
        StoredValues = 4
      end
      object edEndDate: TdxDateEdit
        Left = 275
        Top = 32
        Width = 121
        Style.BorderStyle = xbsSingle
        TabOrder = 1
        Date = -700000.000000000000000000
        DateValidation = True
        UseEditMask = True
        StoredValues = 4
      end
      object edCasa: TdxImageEdit
        Left = 163
        Top = 80
        Width = 300
        Enabled = False
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = []
        ParentFont = False
        Style.BorderStyle = xbsSingle
        Style.HotTrack = True
        TabOrder = 3
        Anchors = [akLeft, akTop, akRight]
      end
      object btnCauta: TBitBtn
        Left = 391
        Top = 112
        Width = 75
        Height = 25
        Anchors = [akRight, akBottom]
        Caption = '&Cauta'
        TabOrder = 4
        OnClick = btnCautaClick
      end
      object chkHouse: TdxfCheckBox
        Left = 16
        Top = 83
        Width = 147
        Height = 18
        Checked = False
        GroupIndex = 0
        Caption = 'o Casa/Banca Specificata'
        TabOrder = 2
        OnClick = chkHouseClick
      end
    end
  end
  object DTError: TDataSource
    DataSet = QryErrors
    Left = 56
    Top = 23
  end
  object QryErrors: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_LISTA_ERORI :DATA_IN, :DATA_OUT, :COD_CB')
    Params = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_IN'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_OUT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 88
    Top = 24
    ParamData = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_IN'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_OUT'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ErrorList: TImageList
    Height = 12
    Width = 12
    Left = 120
    Top = 26
    Bitmap = {
      494C01010300050004000C000C00FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000300000000C00000001002000000000000009
      0000000000000000000000000000000000000000000000000000000000000000
      0000808080008080800080808000808080008080800000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00808080008080800080808000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000008000
      0000FF000000FF000000FF000000800000008080800080808000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF000000FF000000FF0080808000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000080000000FF000000FF00
      0000FF000000FF000000FF000000FF000000FF00000080000000808080000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000080808000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FF000000FF000000FF00
      0000FF000000FFFFFF00FF000000FF000000FF000000FF000000808080008080
      8000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF000000FF000000FF008080800080808000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000080000000FF000000FF000000FF00
      0000FF000000FFFFFF00FF000000FF000000FF000000FF000000800000008080
      8000808080000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF000000FF000000FF000000FF000000000080808000FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FF000000FF000000FF000000FF00
      0000FF000000FFFFFF00FF000000FF000000FF000000FF000000FF0000008080
      8000808080008080800000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00000000000000FF000000FF000000000080808000FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FF000000FF000000FF000000FF00
      0000FF000000FFFFFF00FF000000FF000000FF000000FF000000FF0000008080
      800080808000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000C0C0
      C000C0C0C000C0C0C000C0C0C000C0C0C000FFFFFF00FFFFFF00FFFFFF008080
      800080808000FFFFFF00FFFFFF000000FF000000FF0080808000FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FF000000FF000000FF000000FF00
      0000C0C0C000FFFFFF00C0C0C000FF000000FF000000FF000000FF0000008080
      800080808000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000C0C0C000C0C0
      C000C0C0C000C0C0C000C0C0C00000000000FFFFFF00FFFFFF00FFFFFF000000
      000080808000FFFFFF00FFFFFF000000FF000000FF0080808000FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000080000000FF000000FF000000FF00
      0000FFFFFF00FFFFFF00FFFFFF00FF000000FF000000FF000000800000000000
      000000000000000000000000000000000000000000000000000000000000C0C0
      C000C0C0C000C0C0C0000000000000000000FFFFFF00FFFFFF00FFFFFF000000
      FF000000000080808000000000000000FF0000000000FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FF000000FF000000FF00
      0000C0C0C000FFFFFF00C0C0C000FF000000FF000000FF000000000000000000
      000000000000000000000000000000000000000000000000000000000000C0C0
      C000C0C0C000000000000000000000000000FFFFFF00FFFFFF00FFFFFF000000
      00000000FF000000FF000000FF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000080000000FF000000FF00
      0000FF000000FF000000FF000000FF000000FF00000080000000000000000000
      000000000000000000000000000000000000000000000000000000000000C0C0
      C00000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000008000
      0000FF000000FF000000FF000000800000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      28000000300000000C0000000100010000000000600000000000000000000000
      000000000000000000000000FFFFFF00F07FFF0000000000E03FFF0000000000
      801FE70000000000800FE3000000000000000300000000000000000000000000
      00000000000000000000010000000000001FE30000000000803FE70000000000
      803FEF0000000000E0FFFF000000000000000000000000000000000000000000
      000000000000}
  end
end
