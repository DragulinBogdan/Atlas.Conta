object frmContProp: TfrmContProp
  Left = 289
  Top = 166
  BorderStyle = bsDialog
  Caption = 'Cont'
  ClientHeight = 274
  ClientWidth = 433
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  DesignSize = (
    433
    274)
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 16
    Top = 64
    Width = 55
    Height = 13
    Caption = 'Simbol cont'
  end
  object Label2: TLabel
    Left = 16
    Top = 136
    Width = 91
    Height = 13
    Caption = 'Sold Initial Debitor :'
  end
  object Label3: TLabel
    Left = 220
    Top = 136
    Width = 93
    Height = 13
    Caption = 'Sold Initial Creditor :'
  end
  object Label8: TLabel
    Left = 16
    Top = 192
    Width = 49
    Height = 13
    Caption = 'Tip Cont : '
  end
  object Label9: TLabel
    Left = 220
    Top = 192
    Width = 48
    Height = 13
    Caption = 'Sumator : '
  end
  object Label10: TLabel
    Left = 16
    Top = 219
    Width = 58
    Height = 13
    Caption = 'Mod Tratare'
  end
  object Label11: TLabel
    Left = 220
    Top = 219
    Width = 46
    Height = 13
    Caption = 'Defalcare'
  end
  object Label12: TLabel
    Left = 16
    Top = 85
    Width = 47
    Height = 13
    Caption = 'Explicatii :'
  end
  object Label13: TLabel
    Left = 220
    Top = 64
    Width = 83
    Height = 13
    Caption = 'Data Sold Initial : '
  end
  object Label4: TLabel
    Left = 144
    Top = 163
    Width = 153
    Height = 13
    Caption = 'Sold total rezultat din repartitori : '
  end
  object edSimbol: TcxDBMaskEdit
    Left = 80
    Top = 60
    DataBinding.DataField = 'CONT'
    DataBinding.DataSource = frmData.DTPlanCont
    TabOrder = 2
    Width = 81
  end
  object edDataSold: TcxDBDateEdit
    Left = 304
    Top = 60
    DataBinding.DataField = 'DATA'
    DataBinding.DataSource = frmData.DTPlanCont
    TabOrder = 3
    Width = 121
  end
  object edRomana: TcxDBMemo
    Left = 80
    Top = 85
    DataBinding.DataField = 'ROMANA'
    DataBinding.DataSource = frmData.DTPlanCont
    TabOrder = 4
    Height = 41
    Width = 345
  end
  object edSID: TcxDBCurrencyEdit
    Left = 112
    Top = 132
    DataBinding.DataField = 'SID'
    DataBinding.DataSource = frmData.DTPlanCont
    TabOrder = 5
    Width = 97
  end
  object edSIC: TcxDBCurrencyEdit
    Left = 320
    Top = 132
    DataBinding.DataField = 'SIC'
    DataBinding.DataSource = frmData.DTPlanCont
    TabOrder = 6
    Width = 97
  end
  object edTipCont: TcxDBImageComboBox
    Left = 88
    Top = 188
    DataBinding.DataField = 'FCTCONT'
    DataBinding.DataSource = frmData.DTPlanCont
    Properties.Items = <
      item
        Description = 'Debitor'
        ImageIndex = 0
        Value = 'D'
      end
      item
        Description = 'Creditor'
        Value = 'C'
      end
      item
        Description = 'Bifunctional'
        Value = 'B'
      end>
    TabOrder = 7
    Width = 121
  end
  object edSumator: TcxDBImageComboBox
    Left = 272
    Top = 188
    DataBinding.DataField = 'SUMATOR'
    DataBinding.DataSource = frmData.DTPlanCont
    Properties.Items = <
      item
        Description = 'Sumator'
        ImageIndex = 0
        Value = True
      end
      item
        Description = 'Analitic'
        Value = False
      end>
    TabOrder = 8
    Width = 121
  end
  object edTip: TcxDBImageComboBox
    Left = 88
    Top = 215
    DataBinding.DataField = 'TIP'
    DataBinding.DataSource = frmData.DTPlanCont
    Properties.Items = <>
    TabOrder = 9
    Width = 121
  end
  object edBalanta: TcxDBImageComboBox
    Left = 272
    Top = 215
    DataBinding.DataField = 'BALANTA'
    DataBinding.DataSource = frmData.DTPlanCont
    Properties.Items = <>
    TabOrder = 10
    Width = 121
  end
  object ChkAplicaRecursiv: TcxCheckBox
    Left = 16
    Top = 247
    Caption = 'Aplica modificarile recursiv'
    State = cbsChecked
    TabOrder = 11
    Width = 121
  end
  object edTotalRepartitori: TcxCurrencyEdit
    Left = 299
    Top = 160
    Properties.ReadOnly = True
    TabOrder = 12
    Width = 121
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 433
    Height = 49
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'Modificare Cont : '
    Color = clWhite
    Ctl3D = False
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clBlack
    Font.Height = -24
    Font.Name = 'Arial'
    Font.Style = []
    ParentCtl3D = False
    ParentFont = False
    TabOrder = 13
  end
  object BtnOk: TcxButton
    Left = 270
    Top = 246
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    TabOrder = 0
    OnClick = BtnOkClick
  end
  object BtnCancel: TcxButton
    Left = 350
    Top = 246
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
    TabOrder = 1
    OnClick = BtnCancelClick
  end
end
