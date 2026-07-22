unit InflStocCategorie;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, cxLookAndFeelPainters, cxControls, cxContainer, cxEdit,
  cxGroupBox, StdCtrls, cxButtons, ExtCtrls, cxGraphics, 
  cxTL, cxInplaceContainer, cxTLData, cxDBTL, DB, dxmdaset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles;

type
  TfrmCategoriipeStoc = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    grpTipProd: TcxGroupBox;
    TreeCategorii: TcxDBTreeList;
    qryInflCategorii: TdxMemData;
    DTInflCategorii: TDataSource;
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
  private
    FIdGestTipStoc: Integer;
    FIdGestTipDocum: Integer;
    FSemnItems: Integer;
    FTipPredator: Integer;
    FOldSemn: Variant;
    FSemn: Variant;
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshScreen;
    property IdGestTipStoc : Integer read FIdGestTipStoc write FIdGestTipStoc;
    property IdGestTipDocum : Integer read FIdGestTipDocum write FIdGestTipDocum;
    property TipPredator : Integer read FTipPredator write FTipPredator;
    property SemnItems : Integer read FSemnItems write FSemnItems;
    property Semn : Variant read FSemn write FSemn;
    property OldSemn : Variant read FOldSemn write FOldSemn;
  end;


implementation



{$R *.dfm}

procedure TfrmCategoriipeStoc.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  btnOk.Click;
end;

procedure TfrmCategoriipeStoc.RefreshScreen;
begin
 // aqry
end;

end.
