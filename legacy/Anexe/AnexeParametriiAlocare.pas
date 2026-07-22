unit AnexeParametriiAlocare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxControls,
  cxContainer, cxEdit, cxTextEdit, cxDBEdit, DB, ZDataSet, 
  cxGraphics, cxDataStorage, cxDBData,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxClasses, cxGridLevel, cxGrid, dxExEdtr, dxCntner,
  dxTL, dxDBCtrl, cxCheckBox, dxDBTLCl, Menus, 
  cxTL, cxDBTL,
  cxMaskEdit, DegradePanel, cxImageComboBox, cxDropDownEdit, cxSpinEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmAsocParam = class(TForm)
    pnOptions: TPanel;
    Panel1: TPanel;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    pnBottom: TPanel;
    cxButton1: TcxButton;
    Label1: TLabel;
    pnTop: TDegradePanel;
    Grid: TcxGridDBTableView;
    GridL: TcxGridLevel;
    cxGrid: TcxGrid;
    Label2: TLabel;
    DTParam: TDataSource;
    qryParam: TZQuery;
    GridID_ANEXE_BILANT_PARAMETRII: TcxGridDBColumn;
    GridID_ANEXE_PARAMETRII: TcxGridDBColumn;
    GridID_ANEXE_BILANT: TcxGridDBColumn;
    GridORDINE: TcxGridDBColumn;
    edParam: TcxDBImageComboBox;
    cxDBTextEdit1: TcxDBSpinEdit;
    procedure btnCloseClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    FIdAnexeBilant: Integer;
    procedure SetIdAnexeBilant(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
    property IdAnexeBilant : Integer read FIdAnexeBilant write SetIdAnexeBilant;
  end;


implementation

uses
  dxCompsUtile, DateUnit, ZeosDBUtile;

{$R *.dfm}

procedure TfrmAsocParam.btnCloseClick(Sender: TObject);
begin
  if  fsmodal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmAsocParam.FormCreate(Sender: TObject);
begin
  FillImageCombo(edParam.Properties, 'SELECT ID_ANEXE_PARAMETRII, coalesce(DESCRIPTION, CAPTION, PARAMNAME) FROM ANEXE_PARAMETRII', 0, 1);
  GridID_ANEXE_PARAMETRII.Properties.Assign(edParam.Properties);
  RefreshDataSet;
end;

procedure TfrmAsocParam.RefreshDataSet;
begin
  DBRefresh(qryParam);
end;

procedure TfrmAsocParam.btnAddClick(Sender: TObject);
begin
  qryParam.Append;
  qryParam.FieldByName('ID_ANEXE_BILANT').AsInteger := FIdAnexeBilant;
  qryParam.Post;
  qryParam.Edit;
end;

procedure TfrmAsocParam.btnDelClick(Sender: TObject);
var
  lDenParam : String;
begin
  lDenParam :=  edParam.EditText;
  if (MessageDlg(Format('Doriti stergerea parametrului  : %s', [lDenParam]), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;
  qryParam.Delete;
end;

procedure TfrmAsocParam.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if qryParam.State in [dsEdit, dsInsert] then
     qryParam.Post;
  //inchidem forma
  
    Action := caFree;
end;

procedure TfrmAsocParam.SetIdAnexeBilant(const Value: Integer);
begin
  FIdAnexeBilant := Value;
  qryParam.Params.ParamByName('ID_ANEXE_BILANT').Value := FIdAnexeBilant;
  DBRefresh(qryParam);
end;

end.
