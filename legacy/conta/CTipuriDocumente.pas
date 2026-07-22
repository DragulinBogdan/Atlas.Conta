unit CTipuriDocumente;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxControls,
  cxContainer, cxEdit, cxTextEdit, cxDBEdit, DB, 
  cxGraphics, cxDataStorage, cxDBData,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxClasses, cxGridLevel, cxGrid, dxExEdtr, dxCntner,
  dxTL, dxDBCtrl, cxCheckBox, dxDBTLCl, Menus, 
  cxTL, cxDBTL,
  cxMaskEdit, DegradePanel,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmCTipuriDocumente = class(TForm)
    pnOptions: TPanel;
    Panel1: TPanel;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    pnBottom: TPanel;
    cxButton1: TcxButton;
    Label1: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    pnTop: TDegradePanel;
    GridTipDoc: TcxGridDBTableView;
    GridTipDocL: TcxGridLevel;
    cxGridTipDoc: TcxGrid;
    GridTipDocID_CTIP_DOCUMENTE: TcxGridDBColumn;
    GridTipDocCOD_DOCUMENT: TcxGridDBColumn;
    GridTipDocDESCRIERE: TcxGridDBColumn;
    Label2: TLabel;
    cxDBTextEdit2: TcxDBTextEdit;
    btnImport: TcxButton;
    procedure btnCloseClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnImportClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
  end;

var
  frmCTipuriDocumente: TfrmCTipuriDocumente;

implementation

uses
  ZeosDBUtile, DateUnit, CommonDBVar;

{$R *.dfm}

procedure TfrmCTipuriDocumente.btnCloseClick(Sender: TObject);
begin
  if  fsmodal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmCTipuriDocumente.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  
  //DTOIProiecteTipuri
end;

procedure TfrmCTipuriDocumente.RefreshDataSet;
begin
  DBRefresh(frmdata.QryCTipDoc);
end;

procedure TfrmCTipuriDocumente.btnAddClick(Sender: TObject);
begin
  frmdata.QryCTipDoc.Append;
  frmdata.QryCTipDoc.FieldByName('DESCRIERE').AsString := '<Tip Nou>';
  frmdata.QryCTipDoc.Post;
  frmdata.QryCTipDoc.Edit;
end;

procedure TfrmCTipuriDocumente.btnDelClick(Sender: TObject);
var
   lDenTip : String;
begin
  lDenTip := frmdata.QryCTipDoc.FieldByName('COD_DOCUMENT').AsString;
  if (MessageDlg(Format('Doriti stergerea tipului de proiect  : %s', [lDenTip]), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;
  frmdata.QryCTipDoc.Delete;
end;

procedure TfrmCTipuriDocumente.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if frmdata.QryCTipDoc.State in [dsEdit, dsInsert] then
     frmdata.QryCTipDoc.Post;
  //facem refresh pe data module la cele cu stare 1
  DBRefresh(frmdata.QryCTipDoc);
  //inchidem forma
  
    Action := caFree;
end;

procedure TfrmCTipuriDocumente.btnImportClick(Sender: TObject);
begin
  //
  DBExecSQL('exec spCImportTipDocumente');
  DBRefresh(frmdata.QryCTipDoc);
end;

end.
