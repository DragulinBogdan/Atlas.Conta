unit OI_ProiecteTipuri;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxControls,
  cxContainer, cxEdit, cxTextEdit, cxDBEdit, DB, ZDataSet, 
  cxGraphics, cxDataStorage, cxDBData,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, dxExEdtr, dxCntner,
  dxTL, dxDBCtrl, cxCheckBox, dxDBTLCl, Menus, 
  cxTL, cxInplaceContainer, cxTLData, cxDBTL,
  cxMaskEdit, DegradePanel,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles, dxScrollbarAnnotations;

type
  TfrmOITipuriProiecte = class(TForm)
    pnOptions: TPanel;
    Panel1: TPanel;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    pnBottom: TPanel;
    cxButton1: TcxButton;
    Label1: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    TreeOITipProiecte: TcxDBTreeList;
    edtSeAfiseaza: TcxDBCheckBox;
    DTOIProiecteTipuri: TDataSource;
    qryOIProiecteTipuri: TZQuery;
    TreeOITipProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    TreeOITipProiecteDENUMIRE: TcxDBTreeListColumn;
    TreeOITipProiecteID_PARINTE: TcxDBTreeListColumn;
    TreeOITipProiecteSTARE: TcxDBTreeListColumn;
    pnTop: TDegradePanel;
    procedure btnCloseClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure TreeOITipProiecteCustomDrawDataCell(
      Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
      AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
  end;

procedure ShowTipuriProiecte;

implementation

uses
  DateUnit, ZeosDBUtile;

{$R *.dfm}

procedure ShowTipuriProiecte;
var
  lTipuriProiecte: TfrmOITipuriProiecte;
begin
  lTipuriProiecte := TfrmOITipuriProiecte.Create(nil);
  try
    lTipuriProiecte.ShowModal;
  finally
    lTipuriProiecte.Free;
  end;
end;

procedure TfrmOITipuriProiecte.btnCloseClick(Sender: TObject);
begin
  if  fsmodal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmOITipuriProiecte.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  //DTOIProiecteTipuri
end;

procedure TfrmOITipuriProiecte.RefreshDataSet;
begin
  DBRefresh(qryOIProiecteTipuri);
end;

procedure TfrmOITipuriProiecte.btnAddClick(Sender: TObject);
begin
  qryOIProiecteTipuri.Append;
  qryOIProiecteTipuri.FieldByName('DENUMIRE').AsString := '<Tip Nou>';
  qryOIProiecteTipuri.Post;
  qryOIProiecteTipuri.Edit;
end;

procedure TfrmOITipuriProiecte.btnDelClick(Sender: TObject);
var
   lDenTip : String;
begin
  lDenTip := qryOIProiecteTipuri.FieldByName('DENUMIRE').AsString;
  if (MessageDlg(Format('Doriti stergerea tipului de proiect  : %s', [lDenTip]), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;
  qryOIProiecteTipuri.Delete;
end;

procedure TfrmOITipuriProiecte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if qryOIProiecteTipuri.State in [dsEdit, dsInsert] then
     qryOIProiecteTipuri.Post;
  //facem refresh pe data module la cele cu stare 1
  DBRefresh(frmData.qryOIProiecteTipuri);
  //inchidem forma
  
    Action := caFree;
end;

procedure TfrmOITipuriProiecte.TreeOITipProiecteCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
 //
  if not (Trim(AViewInfo.Node.Texts[TreeOITipProiecteSTARE.ItemIndex]) = '')
     and (AViewInfo.Node.Values[TreeOITipProiecteSTARE.ItemIndex] = False) then
   begin
     ACanvas.Font.Color := clRed;
     ACanvas.Font.Style := ACanvas.Font.Style + [fsStrikeOut];
   end
   else begin
     ACanvas.Font.Color := clBlack;
     ACanvas.Font.Style := ACanvas.Font.Style - [fsStrikeOut];
   end;
   if TreeOITipProiecte.FocusedNode = AViewInfo.Node then begin
     ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
     ACanvas.Font.Color := clGreen;
   end;
end;

end.
