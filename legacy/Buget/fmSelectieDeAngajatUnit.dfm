object fmSelectieDeAngajat: TfmSelectieDeAngajat
  Left = 316
  Top = 229
  AutoScroll = False
  Caption = 'Selectie Pozitii de Angajat'
  ClientHeight = 386
  ClientWidth = 640
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object pnBottom: TPanel
    Left = 0
    Top = 344
    Width = 640
    Height = 42
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
  end
  object pageTipDeAngajat: TcxPageControl
    Left = 0
    Top = 0
    Width = 640
    Height = 344
    ActivePage = tabProiecte
    Align = alClient
    TabOrder = 1
    ClientRectBottom = 344
    ClientRectRight = 640
    ClientRectTop = 24
    object tabProiecte: TcxTabSheet
      Caption = 'Proiecte'
      ImageIndex = 0
      object treeProiecte: TcxDBTreeList
        Left = 0
        Top = 0
        Width = 640
        Height = 320
        Align = alClient
        Bands = <>
        RootValue = -1
        TabOrder = 0
      end
    end
    object tabCE: TcxTabSheet
      Caption = 'Clasificatie Economica'
      ImageIndex = 1
      object treeCE: TcxDBTreeList
        Left = 0
        Top = 0
        Width = 640
        Height = 320
        Align = alClient
        Bands = <>
        RootValue = -1
        TabOrder = 0
      end
    end
  end
end
