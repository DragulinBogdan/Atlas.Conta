object FormAlop: TFormAlop
  Left = 0
  Top = 0
  Caption = 'Alop'
  ClientHeight = 647
  ClientWidth = 1049
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object pnDocument: TPanel
    Left = 0
    Top = 0
    Width = 1049
    Height = 175
    Align = alTop
    TabOrder = 0
    DesignSize = (
      1049
      175)
    object lbDocument: TLabel
      Left = 450
      Top = 3
      Width = 76
      Height = 13
      Anchors = [akTop]
      Caption = 'Nr. Angajament'
      ExplicitLeft = 434
    end
    object LbDataNota: TLabel
      Left = 684
      Top = 3
      Width = 51
      Height = 13
      Anchors = [akTop]
      Caption = 'Din Data : '
      ExplicitLeft = 660
    end
    object LbPredator: TLabel
      Left = 9
      Top = 3
      Width = 124
      Height = 13
      Caption = 'Departament Angajament'
    end
    object LbPrimitor: TLabel
      Left = 994
      Top = 3
      Width = 47
      Height = 13
      Anchors = [akTop, akRight]
      Caption = 'Beneficiar'
      ExplicitLeft = 959
    end
    object Label1: TLabel
      Left = 9
      Top = 90
      Width = 119
      Height = 13
      Caption = 'Clasificatie Functionala : '
    end
    object Label2: TLabel
      Left = 9
      Top = 65
      Width = 85
      Height = 13
      Caption = 'Tip Angajament : '
    end
    object lbContract: TLabel
      Left = 416
      Top = 43
      Width = 72
      Height = 13
      Caption = 'Detalii contract'
    end
    object Label3: TLabel
      Left = 10
      Top = 42
      Width = 74
      Height = 13
      Caption = 'Detalii rectificat'
    end
    object LbScopul: TLabel
      Left = 700
      Top = 3
      Width = 47
      Height = 13
      Anchors = [akTop]
      Caption = 'Nr proiect'
      Visible = False
      ExplicitLeft = 676
    end
    object Label4: TLabel
      Left = 9
      Top = 114
      Width = 33
      Height = 13
      Caption = 'Scop : '
    end
    object lbLegal: TLabel
      Left = 306
      Top = 66
      Width = 41
      Height = 13
      Caption = 'in baza :'
    end
    object lbNumar: TLabel
      Left = 772
      Top = 41
      Width = 67
      Height = 13
      Anchors = [akTop, akRight]
      Caption = 'Detalii Dosar :'
      ExplicitLeft = 737
    end
    object edtDetaliiContract: TcxPopupEdit
      Left = 493
      Top = 38
      Hint = 'Detalile depre contractul de investitii'
      Anchors = [akLeft, akTop, akRight]
      Properties.HideSelection = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupMinHeight = 350
      Properties.PopupSysPanelStyle = True
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 5
      Width = 273
    end
    object edPredator: TcxPopupEdit
      Left = 9
      Top = 15
      Hint = 
        'Departamentul/Biroul care reprezinta partea tehnica a angajament' +
        'ului'
      AutoSize = False
      ParentFont = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupSysPanelStyle = True
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 0
      Height = 21
      Width = 192
    end
    object edPrimitor: TcxPopupEdit
      Left = 787
      Top = 15
      Hint = 'Furnizorul/entitatea externa cu care se incheie angajamentul'
      Anchors = [akTop]
      AutoSize = False
      ParentFont = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupSysPanelStyle = True
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 4
      Height = 21
      Width = 251
    end
    object edDataDoc: TcxDateEdit
      Left = 674
      Top = 16
      Hint = 'Data Angajamentului'
      Anchors = [akTop]
      Properties.DateOnError = deNull
      Properties.ImmediatePost = True
      Properties.InputKind = ikMask
      Properties.SaveTime = False
      Properties.ShowTime = False
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 2
      Width = 89
    end
    object edTipAngajament: TcxImageComboBox
      Left = 128
      Top = 63
      Hint = 'Tipul de angajament'
      Properties.Items = <>
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 3
      Width = 169
    end
    object edNumarDoc: TcxButtonEdit
      Left = 446
      Top = 16
      Hint = 'Numarul de angajament'
      Anchors = [akTop]
      Properties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 1
      Width = 89
    end
    object edRectificat: TcxButtonEdit
      Left = 128
      Top = 38
      Properties.Buttons = <
        item
          Default = True
          Hint = 'Adaugare rectificat'
          Kind = bkEllipsis
        end
        item
          Caption = 'X'
          Hint = 'Stergere rectificat'
          Kind = bkText
        end>
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 6
      Width = 284
    end
    object cxFunctionalBar: TcxPopupEdit
      Left = 128
      Top = 86
      Anchors = [akLeft, akTop, akRight]
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 450
      Properties.PopupMinHeight = 450
      Properties.PopupSysPanelStyle = True
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 7
      Width = 915
    end
    object edNrProiect: TcxButtonEdit
      Left = 686
      Top = 14
      Hint = 'Numar proiect de angajament'
      Anchors = [akTop]
      Properties.Buttons = <
        item
          Default = True
          Kind = bkEllipsis
        end>
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 8
      Visible = False
      Width = 89
    end
    object edtScop: TcxTextEdit
      Left = 128
      Top = 109
      Hint = 'Descriere Angajament'
      Anchors = [akLeft, akTop, akRight]
      AutoSize = False
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 9
      Height = 21
      Width = 915
    end
    object edLegal: TcxButtonEdit
      Left = 352
      Top = 62
      Anchors = [akLeft, akTop, akRight]
      Properties.Buttons = <
        item
          Default = True
          Hint = 'Adaugare rectificat'
          Kind = bkEllipsis
        end
        item
          Caption = 'X'
          Hint = 'Stergere rectificat'
          Kind = bkText
        end>
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 10
      Width = 691
    end
    object btnAdaugaPozitii: TcxButton
      Left = 8
      Top = 136
      Width = 140
      Height = 30
      Anchors = [akLeft, akBottom]
      LookAndFeel.Kind = lfOffice11
      TabOrder = 11
    end
    object cxButton2: TcxButton
      Left = 153
      Top = 136
      Width = 140
      Height = 30
      Anchors = [akLeft, akBottom]
      LookAndFeel.Kind = lfOffice11
      TabOrder = 12
    end
    object edSumaProiect: TcxCurrencyEdit
      Left = 397
      Top = 139
      Properties.Alignment.Horz = taRightJustify
      Properties.DisplayFormat = ',0.00;-,0.00'
      Properties.UseDisplayFormatWhenEditing = True
      Properties.UseLeftAlignmentOnEditing = False
      Properties.UseThousandSeparator = True
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 13
      Visible = False
      Width = 121
    end
    object lbSumaProiect: TcxLabel
      Left = 304
      Top = 141
      Caption = 'Suma Pe Proiect'
      Transparent = True
      Visible = False
    end
    object chkRectificareSold: TcxCheckBox
      Left = 899
      Top = 142
      Anchors = [akTop, akRight]
      Caption = 'Rectificare Sold Initial'
      Style.TransparentBorder = False
      TabOrder = 15
      Visible = False
    end
    object edtDetaliiDosar: TcxPopupEdit
      Left = 843
      Top = 38
      Hint = 'Detalii despre dosarul asociat angajamentului'
      Anchors = [akTop, akRight]
      Properties.HideSelection = False
      Properties.PopupAutoSize = False
      Properties.PopupHeight = 400
      Properties.PopupMinHeight = 350
      Properties.PopupMinWidth = 500
      Properties.PopupSysPanelStyle = True
      Properties.PopupWidth = 500
      Style.Color = clCream
      Style.LookAndFeel.Kind = lfOffice11
      StyleDisabled.Color = clWindow
      StyleDisabled.LookAndFeel.Kind = lfOffice11
      StyleDisabled.TextColor = clBlack
      StyleFocused.LookAndFeel.Kind = lfOffice11
      StyleHot.LookAndFeel.Kind = lfOffice11
      TabOrder = 16
      Width = 198
    end
  end
end
