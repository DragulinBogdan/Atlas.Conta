unit OI_UnitatiTipuri;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, cxGraphics,
  cxDataStorage, cxEdit, DB, cxDBData, cxTextEdit,
  cxGridLevel, cxGridCustomTableView, cxGridTableView, cxGridDBTableView,
  cxClasses, cxControls, cxGridCustomView, cxGrid, cxContainer, cxDBEdit,
  StdCtrls, cxButtons, ExtCtrls, DBCtrls, cxCheckBox, Menus, 
  DegradePanel,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmOIUnitatiTipuri = class(TForm)
    pnOptions: TPanel;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    pnBottom: TPanel;
    cxButton1: TcxButton;
    cxOIUnitatiTipuri: TcxGrid;
    cxOIUnitatiTipuriDBTableView1: TcxGridDBTableView;
    cxOIUnitatiTipuriLevel1: TcxGridLevel;
    Panel1: TPanel;
    Label1: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    cxDBCheckBox2: TcxDBCheckBox;
    cxDBCheckBox3: TcxDBCheckBox;
    chkEsteProiect: TcxDBCheckBox;
    cxOIUnitatiTipuriDBTableView1ID_OI_UNITATI_TIPURI: TcxGridDBColumn;
    cxOIUnitatiTipuriDBTableView1DENUMIRE1: TcxGridDBColumn;
    cxOIUnitatiTipuriDBTableView1ARE_CONTABILITATE: TcxGridDBColumn;
    cxOIUnitatiTipuriDBTableView1ARE_CONT: TcxGridDBColumn;
    cxOIUnitatiTipuriDBTableView1ESTE_PROIECT: TcxGridDBColumn;
    pnTop: TDegradePanel;
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
  end;


implementation

uses
  DateUnit, ZeosDBUtile;

{$R *.dfm}

{ TfrmOIUnitatiTipuri }

procedure TfrmOIUnitatiTipuri.RefreshDataSet;
begin
  DBRefresh(FrmData.qryOIUnitatiTipuri);
end;

procedure TfrmOIUnitatiTipuri.btnAddClick(Sender: TObject);
begin
  FrmData.qryOIUnitatiTipuri.Append;
  FrmData.qryOIUnitatiTipuri.FieldByName('DENUMIRE').AsString := '<Tip Nou>';
  FrmData.qryOIUnitatiTipuri.Post;
  FrmData.qryOIUnitatiTipuri.Edit;
end;

procedure TfrmOIUnitatiTipuri.btnDelClick(Sender: TObject);
var
   lDenTip : String;
begin
  lDenTip := FrmData.qryOIUnitatiTipuri.FieldByName('DENUMIRE').AsString;
  if (MessageDlg(Format('Doriti stergerea tipului de unitate  : %s', [lDenTip]), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;
  FrmData.qryOIUnitatiTipuri.Delete;
end;

procedure TfrmOIUnitatiTipuri.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmOIUnitatiTipuri.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
end;

end.
