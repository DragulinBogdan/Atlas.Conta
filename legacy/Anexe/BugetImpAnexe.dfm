object frmImportAnexeXLS: TfrmImportAnexeXLS
  Left = 157
  Top = 43
  Caption = 'Import Anexe Raportare'
  ClientHeight = 626
  ClientWidth = 899
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  WindowState = wsMaximized
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnTools: TPanel
    Left = 0
    Top = 0
    Width = 899
    Height = 230
    Align = alTop
    BevelOuter = bvLowered
    TabOrder = 0
    DesignSize = (
      899
      230)
    object Label2: TLabel
      Left = 3
      Top = 72
      Width = 99
      Height = 13
      Caption = 'Fisier de import : '
      FocusControl = edFileName
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label4: TLabel
      Left = 11
      Top = 135
      Width = 59
      Height = 13
      Caption = 'Camp tabela'
    end
    object Label6: TLabel
      Left = 152
      Top = 150
      Width = 6
      Height = 13
      Caption = '='
    end
    object Label5: TLabel
      Left = 170
      Top = 135
      Width = 68
      Height = 13
      Caption = 'Coloana Excel'
    end
    object Label3: TLabel
      Left = 11
      Top = 95
      Width = 88
      Height = 13
      Caption = 'Camp Indentificare'
    end
    object Label7: TLabel
      Left = 152
      Top = 112
      Width = 6
      Height = 13
      Caption = '='
    end
    object Label8: TLabel
      Left = 170
      Top = 95
      Width = 68
      Height = 13
      Caption = 'Coloana Excel'
    end
    object Label11: TLabel
      Left = 598
      Top = 92
      Width = 102
      Height = 13
      Caption = 'Mod Introducere :'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object edFileName: TEdit
      Left = 102
      Top = 69
      Width = 412
      Height = 21
      BevelInner = bvNone
      BevelKind = bkFlat
      BevelOuter = bvNone
      BorderStyle = bsNone
      TabOrder = 0
      OnChange = edFileNameChange
    end
    object BtnAuto: TcxButton
      Left = 176
      Top = 173
      Width = 79
      Height = 43
      Anchors = [akTop, akRight]
      Caption = 'Identifica'
      Enabled = False
      LookAndFeel.SkinName = ''
      TabOrder = 1
      Visible = False
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object BtnGenerare: TcxButton
      Left = 10
      Top = 189
      Width = 155
      Height = 25
      Caption = 'Genereaza Inregistrari'
      Enabled = False
      TabOrder = 2
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = BtnGenerareClick
    end
    object Progress: TcxProgressBar
      Left = 615
      Top = 64
      AutoSize = False
      ParentColor = False
      Properties.BarBevelOuter = cxbvRaised
      Properties.BarStyle = cxbsGradient
      Properties.BeginColor = 3265321
      Properties.BorderWidth = 1
      Properties.EndColor = 11399085
      Properties.OverloadValue = 100.000000000000000000
      Properties.PeakValue = 100.000000000000000000
      Properties.ShowTextStyle = cxtsPosition
      Properties.SolidTextColor = True
      Style.BorderColor = clInfoBk
      Style.BorderStyle = ebsUltraFlat
      Style.Color = clWhite
      Style.Edges = [bLeft, bTop, bRight, bBottom]
      Style.Shadow = False
      Style.TextStyle = [fsBold]
      Style.TransparentBorder = True
      StyleHot.TextStyle = []
      TabOrder = 3
      Height = 19
      Width = 254
    end
    object edCamp: TcxImageComboBox
      Left = 23
      Top = 149
      Properties.Items = <>
      TabOrder = 4
      Width = 120
    end
    object edColoana: TcxImageComboBox
      Left = 172
      Top = 149
      Properties.Items = <>
      TabOrder = 5
      Width = 120
    end
    object edListaAsocieri: TcxListBox
      Left = 316
      Top = 100
      Width = 90
      Height = 33
      ItemHeight = 13
      TabOrder = 6
      Visible = False
    end
    object edCampIdentificare: TcxImageComboBox
      Left = 23
      Top = 107
      Properties.Items = <>
      TabOrder = 7
      Width = 120
    end
    object edColoanaIdentificare: TcxImageComboBox
      Left = 172
      Top = 108
      Properties.Items = <>
      TabOrder = 8
      Width = 120
    end
    object edNrZerouri: TcxImageComboBox
      Left = 703
      Top = 87
      EditValue = '1'
      Properties.Items = <
        item
          Description = 'Leu'
          ImageIndex = 0
          Value = '1'
        end
        item
          Description = 'Zeci de lei'
          ImageIndex = 1
          Value = '10'
        end
        item
          Description = 'Sute de lei'
          ImageIndex = 2
          Value = '100'
        end
        item
          Description = 'Mii lei'
          ImageIndex = 3
          Value = '1000'
        end
        item
          Description = 'Zeci de mii'
          ImageIndex = 4
          Value = '10000'
        end
        item
          Description = 'Sute de mii'
          ImageIndex = 5
          Value = '100000'
        end
        item
          Description = 'Milioane'
          ImageIndex = 6
          Value = '1000000'
        end>
      Properties.OnChange = edNrZerouriPropertiesChange
      TabOrder = 9
      Width = 97
    end
    object edZerouri: TcxSpinEdit
      Left = 803
      Top = 87
      TabOrder = 10
      Value = 1
      Width = 68
    end
    object btnAdauga: TcxButton
      Left = 310
      Top = 143
      Width = 23
      Height = 22
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360800000000000036000000280000002000000010000000010020000000
        000000000000C40E0000C40E0000000000000000000000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        800000808000008080000080800000808000FFFFFFFFFFFFFFFF008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF000000FF00808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFF7F7F7FFF00808000FFFFFFFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF0000FFFF000000FF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFFFFFFFFFF7F7F7FFF7F7F7FFF0080
        8000FFFFFFFFFFFFFFFF00808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF0000FFFF0000FFFF0000FFFF0000
        00FF000000FF0080800000808000008080000080800000808000FFFFFFFFFFFF
        FFFFFFFFFFFFFFFFFFFFFFFFFFFF7F7F7FFFFFFFFFFF00808000008080007F7F
        7FFF7F7F7FFF00808000FFFFFFFFFFFFFFFF00808000000000FF000000FF0000
        00FF000000FF000000FF000000FF000000FF0000FFFF0000FFFF0000FFFF0000
        FFFF0000FFFF000000FF000000FF00808000008080007F7F7FFF7F7F7FFF7F7F
        7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFF0080800000808000008080000080
        8000008080007F7F7FFF7F7F7FFF00808000FFFFFFFF000000FF0000FFFF0000
        FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
        FFFF0000FFFF0000FFFF0000FFFF000000FF000000FF7F7F7FFFFFFFFFFFFFFF
        FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00808000008080000080
        80000080800000808000FFFFFFFF7F7F7FFF7F7F7FFF000000FF000000FF0000
        00FF000000FF000000FF000000FF000000FF0000FFFF0000FFFF0000FFFF0000
        FFFF0000FFFF000000FF000000FF00808000008080007F7F7FFF7F7F7FFF7F7F
        7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFFFFFFFFFF00808000008080000080
        8000FFFFFFFF7F7F7FFF7F7F7FFF008080000080800000808000008080000080
        8000008080000080800000808000000000FF0000FFFF0000FFFF0000FFFF0000
        00FF000000FF0080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFFFFFFFFFF00808000FFFFFFFF7F7F
        7FFF7F7F7FFF0080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF0000FFFF000000FF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFFFFFFFFFF7F7F7FFF7F7F7FFF0080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF000000FF00808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFF7F7F7FFF00808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080000080800000808000}
      OptionsImage.NumGlyphs = 2
      TabOrder = 11
      OnClick = btnAdaugaClick
    end
    object btnSterge: TcxButton
      Left = 310
      Top = 172
      Width = 23
      Height = 22
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360800000000000036000000280000002000000010000000010020000000
        000000000000C40E0000C40E0000000000000000000000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000FFFFFFFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080000080800000808000000000FF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        800000808000008080000080800000808000FFFFFFFF7F7F7FFF7F7F7FFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF000000FF0000FFFF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000FFFFFFFF7F7F7FFF7F7F7FFF008080007F7F7FFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        800000808000000000FF000000FF0000FFFF0000FFFF0000FFFF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        8000FFFFFFFF7F7F7FFF7F7F7FFF0080800000808000008080007F7F7FFFFFFF
        FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00808000008080000000
        00FF000000FF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF000000FF0000
        00FF000000FF000000FF000000FF000000FF000000FF00808000FFFFFFFF7F7F
        7FFF7F7F7FFF00808000008080000080800000808000008080007F7F7FFF7F7F
        7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFF000000FF000000FF0000
        FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
        FFFF0000FFFF0000FFFF0000FFFF0000FFFF000000FF7F7F7FFF7F7F7FFF0080
        8000FFFFFFFFFFFFFFFF0080800000808000008080000080800000808000FFFF
        FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF7F7F7FFF00808000008080000000
        00FF000000FF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF000000FF0000
        00FF000000FF000000FF000000FF000000FF000000FF00808000008080007F7F
        7FFF7F7F7FFF00808000FFFFFFFFFFFFFFFF00808000008080007F7F7FFF7F7F
        7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFF7F7F7FFF00808000008080000080
        800000808000000000FF000000FF0000FFFF0000FFFF0000FFFF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080007F7F7FFF7F7F7FFF00808000FFFFFFFFFFFFFFFF7F7F7FFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000000000FF000000FF0000FFFF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080007F7F7FFF7F7F7FFF008080007F7F7FFFFFFF
        FFFF008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080000080800000808000000000FF000000FF0080
        8000008080000080800000808000008080000080800000808000008080000080
        800000808000008080000080800000808000008080007F7F7FFF7F7F7FFF0080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        8000008080000080800000808000008080000080800000808000008080000080
        80000080800000808000008080000080800000808000}
      OptionsImage.NumGlyphs = 2
      TabOrder = 12
      OnClick = btnStergeClick
    end
    object edListaCaption: TcxListBox
      Left = 337
      Top = 110
      Width = 532
      Height = 113
      ItemHeight = 13
      TabOrder = 13
    end
    object pnParam: TPanel
      Left = 1
      Top = 1
      Width = 897
      Height = 60
      Align = alTop
      BevelOuter = bvNone
      Color = 15788262
      TabOrder = 14
      object Label9: TLabel
        Left = 6
        Top = 10
        Width = 104
        Height = 13
        Caption = 'Selectie Unitate : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label10: TLabel
        Left = 426
        Top = 10
        Width = 104
        Height = 13
        Caption = 'Perioada fiscala : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label1: TLabel
        Left = 4
        Top = 37
        Width = 98
        Height = 13
        Caption = 'Selectie Anexa : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edtUnitate: TcxPopupEdit
        Tag = -1
        Left = 108
        Top = 5
        Properties.PopupAutoSize = False
        Properties.PopupControl = cxTreeUnitati
        Properties.PopupSysPanelStyle = True
        Properties.OnCloseQuery = edtUnitatePropertiesCloseQuery
        Properties.OnInitPopup = edtUnitatePropertiesInitPopup
        Properties.OnPopup = edtUnitatePropertiesPopup
        TabOrder = 0
        Width = 303
      end
      object edtPerioada: TcxImageComboBox
        Tag = -1
        Left = 531
        Top = 5
        Properties.Items = <>
        Properties.OnChange = edtPerioadaPropertiesChange
        TabOrder = 1
        Width = 303
      end
      object edtAnexa: TcxImageComboBox
        Tag = -1
        Left = 108
        Top = 32
        Properties.Items = <>
        Properties.OnChange = edtAnexaPropertiesChange
        TabOrder = 2
        Width = 432
      end
      object btnEditParams: TcxButton
        Left = 657
        Top = 30
        Width = 100
        Height = 24
        Caption = 'Parametrii'
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7F7F7FF949494FF7B7373FF84847BFF84847BFFBDB5
          B5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00E7E7E7FF6B6363FF848C84FF949494FF636363FF9494
          94FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00EFEFEFFF8C847BFFEFDEDEFFFFFFFF008C8473FF9494
          94FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0084847BFF63636BFF7B8494FF423939FFA59C
          9CFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00FFFFFF0073D6EFFF00A5D6FF00ADDEFF00A5D6FF9CC6
          C6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7EFE7FF39DEFFFF10DEFFFF29D6FFFF10DEFFFF6BB5
          CEFFFFE7D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFF7EFFF84D6E7FF39D6FFFF42C6FFFF42C6FFFF42DEFFFF39CE
          EFFFBDB5ADFFFFF7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFF7F7FFADDEE7FF4ADEFFFF63D6FFFF73DEFFFF84DEFFFF6BD6FFFF63E7
          FFFF5ABDD6FFD6BDB5FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00DEEFEFFF7BDEF7FF7BD6FFFF8CD6FFFFADDEFFFFADE7FFFF94D6FFFF94DE
          FFFF73E7FFFF84B5C6FFF7E7DEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00BDE7F7FF94DEFFFF9CDEFFFFC6E7FFFFD6EFFFFFD6EFFFFFC6E7FFFFA5DE
          FFFFB5F7FFFF84D6E7FFCED6D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00B5E7FFFF94DEFFFFBDE7FFFFE7EFFFFFE7EFFFFFDEEFFFFFE7EFFFFFC6E7
          FFFFBDE7FFFFA5E7F7FFBDD6D6FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00A5DEFFFF94D6FFFFD6E7FFFFDEEFFFFFE7EFFFFFDEEFFFFFE7EFFFFFCEE7
          FFFFADDEFFFFB5E7EFFFDEDEDEFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00ADDEF7FF73D6FFFFDEF7FFFFD6F7FFFFC6EFFFFFCEEFFFFFD6EFFFFFCEEF
          FFFF8CDEFFFFADDEEFFFFFEFE7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00EFF7F7FF6BCEFFFF7BDEFFFFE7FFFFFFEFFFFFFFCEF7FFFFC6EFFFFF94DE
          FFFF5ACEFFFFCEE7EFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00D6EFFFFF5ACEFFFF73DEFFFFB5EFFFFF94E7FFFF63DEFFFF4AC6
          F7FFB5DEEFFFFFFFF7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
          FF00FFFFFF00FFFFFF00F7FFFFFFA5E7FFFF84DEFFFF84DEFFFF94DEF7FFDEEF
          F7FFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00}
        TabOrder = 3
        Visible = False
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        OnClick = btnEditParamsClick
      end
      object edIdParam: TcxDBTextEdit
        Left = 773
        Top = 33
        DataBinding.DataField = 'idCDAnexeIntrodus'
        DataBinding.DataSource = DTHead
        ParentFont = False
        Properties.ReadOnly = True
        Style.Color = clSilver
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        TabOrder = 4
        Width = 121
      end
    end
    object btnOpenFile: TcxButton
      Left = 520
      Top = 69
      Width = 21
      Height = 21
      Caption = '...'
      TabOrder = 15
      OnClick = btnOpenFileClick
    end
  end
  object cxTreeUnitati: TcxDBTreeList
    Left = 484
    Top = 408
    Width = 305
    Height = 184
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTOIUnitati
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_OI_UNITATI'
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeUnitatiDESCRIERE
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 1
    Visible = False
    OnDblClick = cxTreeUnitatiDblClick
    OnKeyDown = cxTreeUnitatiKeyDown
    object cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI_TIPURI'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.GlyphAlignHorz = taCenter
      Caption.Text = 'Institutie/Unitate'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATATEA_URMARITA'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'NUME_ORDONANTATOR'
      Width = 100
      Position.ColIndex = 10
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_UTILIZATORI'
      Width = 100
      Position.ColIndex = 13
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiSTARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'STARE'
      Width = 100
      Position.ColIndex = 12
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATEA_CENTRALIZATOARE'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA'
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_COD'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_CONT'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object Excel: TdxSpreadSheet
    Left = 0
    Top = 230
    Width = 899
    Height = 396
    Align = alClient
    OnActiveSheetChanged = ExcelActiveSheetChanged
    OnProgress = ExcelProgress
    ExplicitTop = 229
    Data = {
      D701000044585353763242460B00000042465320000000000000000001000101
      010100000000000001004246532000000000424653200200000001000000200B
      00000007000000430061006C0069006200720069000000000000002000000020
      00000000200000000020000000002000000000200007000000470045004E0045
      00520041004C0000000000000200000000000000000101000000200B00000007
      000000430061006C006900620072006900000000000000200000002000000000
      200000000020000000002000000000200007000000470045004E004500520041
      004C000000000000020000000000000000014246532001000000424653201700
      0000540064007800530070007200650061006400530068006500650074005400
      610062006C006500560069006500770006000000530068006500650074003100
      01FFFFFFFFFFFFFFFF6400000002000000020000000200000055000000140000
      0002000000020000000002000000000000010000000000010100004246532055
      0000000000000042465320000000004246532014000000000000004246532000
      0000000000000000000000010000000000000000000000000000000000000042
      465320000000000000000000000000424653200000000000000000}
  end
  object qryHead: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from CDAnexeIntrodus')
    Params = <>
    Left = 398
    Top = 24
  end
  object DTHead: TDataSource
    DataSet = qryHead
    Left = 347
    Top = 23
  end
  object openFile: TOpenDialog
    DefaultExt = '*.xls'
    Filter = 'Fisiere EXCEL|*.xls|All files (*.*)|*.*'
    Title = 'Alegeti Fisierul Excel'
    Left = 280
    Top = 32
  end
end
