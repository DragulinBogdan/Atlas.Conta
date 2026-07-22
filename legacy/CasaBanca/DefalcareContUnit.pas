unit DefalcareContUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, dxmdaset, dxCntner, dxTL, dxDBCtrl, dxDBGrid, ExtCtrls,
  HeadPanel, dxDBTLCl, dxGrClms, dxGrClEx;

type
  TFrmDefalcareCont = class(TForm)
    HeadPanel1: THeadPanel;
    pnClient: TPanel;
    GridDefalcare: TdxDBGrid;
    DTDefalcCont: TDataSource;
    MemDefalcCont: TdxMemData;
    MemDefalcContBREG_COD: TIntegerField;
    MemDefalcContCONT_CSP: TStringField;
    MemDefalcContVALOARE: TCurrencyField;
    MemDefalcContEXPLICATIE: TStringField;
    MemDefalcContC_O: TIntegerField;
    MemDefalcContID_DEFALC: TAutoIncField;
    GridDefalcareRecId: TdxDBGridColumn;
    GridDefalcareID_DEFALC: TdxDBGridMaskColumn;
    GridDefalcareBREG_COD: TdxDBGridMaskColumn;
    GridDefalcareCONT_CSP: TdxDBGridPopupColumn;
    GridDefalcareVALOARE: TdxDBGridCurrencyColumn;
    GridDefalcareEXPLICATIE: TdxDBGridMaskColumn;
    GridDefalcareC_O: TdxDBGridMaskColumn;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FrmDefalcareCont: TFrmDefalcareCont;

implementation

{$R *.DFM}

end.
