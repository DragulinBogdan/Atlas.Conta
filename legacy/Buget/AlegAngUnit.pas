unit AlegAngUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, dxExEdtr, DB, ZDataSet, dxCntner, dxTL, dxDBCtrl, dxDBGrid,
  ComCtrls, ExtCtrls, HeadPanel,  cxLookAndFeelPainters, StdCtrls, cxButtons, Menus,
  dxDBTLCl, dxGrClms,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmBugetAlegAng = class(TForm)
    pnTop: THeadPanel;
    pnbottom: TPanel;
    tabGrid: TTabControl;
    GridAngajamente: TdxDBGrid;
    DTAngajamente: TDataSource;
    QryAngajament: TZQuery;
    GridAngajamenteID_UTILIZATORI: TdxDBGridImageColumn;
    GridAngajamenteDATA_EMITERE: TdxDBGridDateColumn;
    GridAngajamenteID_DEPARTAMENT: TdxDBGridImageColumn;
    GridAngajamenteNUMAR: TdxDBGridMaskColumn;
    GridAngajamenteSCOPUL: TdxDBGridColumn;
    GridAngajamenteID_LST_REPARTITORI: TdxDBGridMaskColumn;
    GridAngajamenteID_ANGAJAMENT: TdxDBGridMaskColumn;
    GridAngajamenteCLASA_FUNCTIONALA: TdxDBGridColumn;
    GridAngajamenteTIP_ANGAJAMENT: TdxDBGridImageColumn;
    GridAngajamenteNUME_ID_DEPARTAMENT: TdxDBGridColumn;
    GridAngajamenteNUME_ID_LST_REPARTITORI: TdxDBGridColumn;
    btnOk: TcxButton;
    procedure FormCreate(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure GridAngajamenteDblClick(Sender: TObject);
    procedure GridAngajamenteKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    FAngajament: Integer;
    { Private declarations }
  public
    { Public declarations }
    property Angajament : Integer read FAngajament write FAngajament;
  end;

implementation

uses DateUnit;

{$R *.dfm}

procedure TfrmBugetAlegAng.FormCreate(Sender: TObject);
begin
  SetTipAngajament(GridAngajamenteTIP_ANGAJAMENT.Descriptions, True);
  SetTipAngajament(GridAngajamenteTIP_ANGAJAMENT.Values, False);
  if QryAngajament.Active then QryAngajament.Active := False;
  QryAngajament.Active := True;
end;

procedure TfrmBugetAlegAng.btnOkClick(Sender: TObject);
var
  lNode : TdxTreeListNode;
begin
  lNode := GridAngajamente.FocusedNode;
  if lNode = nil then Exit;
  Angajament :=  lNode.Values[GridAngajamenteID_ANGAJAMENT.Index];
  ModalResult := mrOk;
end;

procedure TfrmBugetAlegAng.GridAngajamenteDblClick(Sender: TObject);
begin
  btnOk.Click;
end;

procedure TfrmBugetAlegAng.GridAngajamenteKeyUp(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then
     btnOk.Click;
end;

end.
