unit TipRepartitori;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxControls,
  cxContainer, cxEdit, cxTextEdit, DB, ZDataSet, 
  cxGraphics, cxDataStorage, cxDBData,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxClasses, cxGridLevel, cxGrid, Menus, 
  DegradePanel,  cxMaskEdit,
  cxButtonEdit,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxDBEdit,
  cxNavigator, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog;

type
  TfrmTipuriRepartitori = class(TForm)
    pnOptions: TPanel;
    Panel1: TPanel;
    pnBottom: TPanel;
    cxButton1: TcxButton;
    Label1: TLabel;
    cxGTipRep: TcxGrid;
    cxGTipRepLevel1: TcxGridLevel;
    GridTipRep: TcxGridDBTableView;
    GridTipRepID_REPARTITORI_TIPURI: TcxGridDBColumn;
    GridTipRepDENUMIRE: TcxGridDBColumn;
    pnTop: TDegradePanel;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    edDenumire: TcxDBButtonEdit;
    GridTipRepCONT: TcxGridDBColumn;
    Label2: TLabel;
    edCont: TcxDBButtonEdit;
    procedure btnCloseClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure cxDBTextEdit1PropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edContPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
  end;

procedure ShowTipuriRepartitori;

implementation

uses
  ZeosDBUtile, DateUnit, CommonDBVar, PlanConturiUnit;

{$R *.dfm}

procedure ShowTipuriRepartitori;
var
  lTipuri : TfrmTipuriRepartitori;
begin
  lTipuri := TfrmTipuriRepartitori.Create(nil);
  try
    lTipuri.ShowModal;
  finally
    lTipuri.Free;
  end;
end;

procedure TfrmTipuriRepartitori.btnCloseClick(Sender: TObject);
begin
  if  fsmodal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmTipuriRepartitori.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmTipuriRepartitori.RefreshDataSet;
begin
  DBRefresh(FrmData.QryRepTipuri);
end;

procedure TfrmTipuriRepartitori.btnAddClick(Sender: TObject);
begin
  FrmData.QryRepTipuri.Append;
  FrmData.QryRepTipuri.FieldByName('DENUMIRE').AsString := '<Tip Nou>';
  FrmData.QryRepTipuri.Post;
  FrmData.QryRepTipuri.Edit;
end;

procedure TfrmTipuriRepartitori.btnDelClick(Sender: TObject);
var
  lDenTip: String;
begin
  lDenTip := FrmData.QryRepTipuri.FieldByName('DENUMIRE').AsString;
  if DBRecordExists('REPARTITORI_CLASIFICATI', 'ID_REPARTITORI_TIPURI', FrmData.QryRepTipuri['ID_REPARTITORI_TIPURI']) then begin
    MessageDlg(Format('Tipul de repatitor curent %s este folosit in cadrul programului !', [lDenTip]), mtError, [mbOK], 0);
    if not IsAdmin then Exit;
  end;

  if (MessageDlg(Format('Doriti stergerea tipului de repartitor  : %s',
                        [lDenTip]), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;
  FrmData.QryRepTipuri.Delete;
end;

procedure TfrmTipuriRepartitori.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmTipuriRepartitori.cxDBTextEdit1PropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
  DoCheckPostDataSet(FrmData.QryRepTipuri);
end;

procedure TfrmTipuriRepartitori.edContPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lCont : string;
begin
  lCont := FrmData.QryRepTipuri.fieldbyName('Cont').AsString;
  if SelectareContPlan(lCont) then begin
    DBSetFieldValue(FrmData.QryRepTipuri, 'Cont', lCont);
    DoCheckPostDataSet(FrmData.QryRepTipuri);
  end;
end;

end.
