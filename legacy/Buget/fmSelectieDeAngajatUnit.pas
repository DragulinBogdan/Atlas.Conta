unit fmSelectieDeAngajatUnit;

interface

uses
  Forms, Classes, Controls, ExtCtrls, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxCustomData, cxStyles, cxTL,
  cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL, DB,
  ZAbstractRODataset, ZDataset, cxPC;

type
  TfmSelectieDeAngajat = class(TForm)
    pnBottom: TPanel;
    pageTipDeAngajat: TcxPageControl;
    tabProiecte: TcxTabSheet;
    tabCE: TcxTabSheet;
    treeProiecte: TcxDBTreeList;
    treeCE: TcxDBTreeList;
  public
  end;
  
implementation

{$R *.DFM}


end.