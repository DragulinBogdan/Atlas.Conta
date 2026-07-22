unit frmContaTranspunereAnUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, DB,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  ExtCtrls, cxContainer, StdCtrls, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, cxSplitter, ZAbstractRODataset, ZAbstractDataset,
  ZDataset, Menus, ActnList, cxButtons, dxLayoutControl, ZSqlUpdate,
  cxDBLookupComboBox, dxLayoutcxEditAdapters, cxLookupEdit, cxDBLookupEdit, dxLayoutContainer,
  cxNavigator;

type
  TfrmContaTranspunereAn = class(TForm)
    gridTranspunere: TcxGridDBTableView;
    cxGridTranspunereLevel1: TcxGridLevel;
    cxGridTranspunere: TcxGrid;
    pnTop: TPanel;
    edModalitateSelectie: TcxImageComboBox;
    Label1: TLabel;
    DSTranspunere: TDataSource;
    qryTranspunere: TZQuery;
    Split: TcxSplitter;
    gridTranspunereid_cplan_transpunere: TcxGridDBColumn;
    gridTranspunerecont: TcxGridDBColumn;
    gridTranspunerecont_vechi: TcxGridDBColumn;
    pnBottom: TPanel;
    lcBasic: TdxLayoutControl;
    dxLayoutGroup1: TdxLayoutGroup;
    Panel1: TPanel;
    cxButton1: TcxButton;
    cxButton2: TcxButton;
    cxButton3: TcxButton;
    ActiuniSI: TActionList;
    actAdauga: TAction;
    actSterge: TAction;
    actModifica: TAction;
    updTranspunere: TZUpdateSQL;
    dsPlanVechi: TDataSource;
    qryPlanVechi: TZQuery;
    qryTranspunereid_cplan_transpunere: TFloatField;
    qryTranspunerecont: TStringField;
    qryTranspunerecont_vechi: TStringField;
    qryTranspunerean_fiscal: TIntegerField;
    edContNou: TcxDBLookupComboBox;
    lcBasicItem3: TdxLayoutItem;
    edContVechi: TcxDBLookupComboBox;
    lcBasicItem1: TdxLayoutItem;
    dsPlanNou: TDataSource;
    qryPlanNou: TZQuery;
    procedure FormShow(Sender: TObject);
    procedure edModalitateSelectiePropertiesEditValueChanged(
      Sender: TObject);
    procedure qryTranspunereAfterOpen(DataSet: TDataSet);
    procedure FormCreate(Sender: TObject);
    procedure actAdaugaExecute(Sender: TObject);
    procedure actStergeExecute(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure actModificaExecute(Sender: TObject);
    procedure gridTranspunereFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
  private
    FReadOnly: Boolean;
    procedure SetReadOnly(const Value: Boolean);
    { Private declarations }
  public
    { Public declarations }
    procedure OpenDataSet;
    property ReadOnly : Boolean read FReadOnly write SetReadOnly;
  end;


implementation

uses
  DateUnit, ZeosDBUtile, Math;

{$R *.dfm}

{ TfrmContaTranspunereAn }

procedure TfrmContaTranspunereAn.OpenDataSet;
begin
  qryPlanVechi.ParamByName('modalitate').AsInteger := edModalitateSelectie.EditValue;
  DBRefresh([qryPlanVechi, qryPlanNou, qryTranspunere]);
end;

procedure TfrmContaTranspunereAn.FormShow(Sender: TObject);
begin
  OpenDataSet;
end;

procedure TfrmContaTranspunereAn.edModalitateSelectiePropertiesEditValueChanged(
  Sender: TObject);
begin
  OpenDataSet;
end;

procedure TfrmContaTranspunereAn.qryTranspunereAfterOpen(
  DataSet: TDataSet);
begin
  DataSet.FieldByName('cont').ReadOnly := False;
  DataSet.FieldByName('cont_vechi').ReadOnly := False;  
end;

procedure TfrmContaTranspunereAn.FormCreate(Sender: TObject);
begin
  ReadOnly := True;
end;

procedure TfrmContaTranspunereAn.SetReadOnly(const Value: Boolean);
begin
  FReadOnly := Value;
  lcBasic.Enabled := not FReadOnly;
  edContNou.Enabled := not FReadOnly;
  edContVechi.Enabled := not FReadOnly;
end;

procedure TfrmContaTranspunereAn.actAdaugaExecute(Sender: TObject);
begin
  qryTranspunere.Append;
  ReadOnly := False;
end;

procedure TfrmContaTranspunereAn.actStergeExecute(Sender: TObject);
begin
  if MessageDlg('Doriti stergerea transpunerii selectate ? ', mtConfirmation, [mbYes, mbNo],0 ) <> mrYes then
    Abort;
  qryTranspunere.Delete;
end;

procedure TfrmContaTranspunereAn.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBPost(qryTranspunere);
  Action := caFree;
end;

procedure TfrmContaTranspunereAn.actModificaExecute(Sender: TObject);
begin
  ReadOnly := False;
end;

procedure TfrmContaTranspunereAn.gridTranspunereFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  ReadOnly := True;
end;

end.
