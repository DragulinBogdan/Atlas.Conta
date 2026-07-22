unit JurnaleUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  cxLookAndFeels, cxCustomData, cxStyles, dxScrollbarAnnotations, cxTL,
  cxImageComboBox, cxTLdxBarBuiltInMenu, cxInplaceContainer, cxGroupBox,
  cxSplitter, cxFilter, cxData, cxDataStorage, cxNavigator, dxDateRanges,
  cxDBData, cxMaskEdit, cxSpinEdit, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView, cxGrid,
  ZAbstractRODataset, ZAbstractDataset, ZDataset, cxGraphics,
  cxLookAndFeelPainters, Vcl.Menus, cxControls, cxContainer, cxEdit, Data.DB,
  cxCheckBox, Vcl.ExtCtrls, cxDBEdit, cxTextEdit, Vcl.StdCtrls, cxButtons,
  cxDBTL, cxTLData;

type
  TfrmJurnale = class(TForm)
    BtnOk: TcxButton;
    edCodJurnal: TcxDBTextEdit;
    edDenumireJurnal: TcxDBTextEdit;
    edNumarNota: TcxDBTextEdit;
    chkJurnalInchidere: TcxDBCheckBox;
    chkJurnalConsolidare: TcxDBCheckBox;
    lbCod: TLabel;
    lbNumarNota: TLabel;
    lbDenumire: TLabel;
    grClient: TcxGroupBox;
    pnBottom: TPanel;
    treeUsers: TcxDBTreeList;
    grJurnal: TcxGroupBox;
    splitterV: TcxSplitter;
    grDetalii: TcxGroupBox;
    viewJurnal: TcxGridDBTableView;
    nivelJurnal: TcxGridLevel;
    gridJurnal: TcxGrid;
    viewJurnalJURNAL: TcxGridDBColumn;
    viewJurnalDENUMIRE: TcxGridDBColumn;
    viewJurnalNUMAR_NOTA: TcxGridDBColumn;
    viewJurnalINCHIDERE: TcxGridDBColumn;
    dtJurnale: TDataSource;
    qryJurnale: TZQuery;
    dtUserJurnale: TDataSource;
    qryUserJurnale: TZQuery;
    treeUsersID_UTILIZATORI: TcxDBTreeListColumn;
    treeUserssel: TcxDBTreeListColumn;
    treeUsersNUMEINTREG: TcxDBTreeListColumn;
    pnUsers: TPanel;
    cxButton1: TcxButton;
    cxButton3: TcxButton;
    procedure BtnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure viewJurnalFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure treeUsersselPropertiesEditValueChanged(Sender: TObject);
    procedure cxButton1Click(Sender: TObject);
    procedure chkJurnalInchidereClick(Sender: TObject);
    procedure cxButton3Click(Sender: TObject);




  private
    FJurnal: String;
    FModified: Boolean;
    procedure SetJurnal(const Value: String);


    { Private declarations }
  public
    property Modified: Boolean read FModified write FModified;
    property Jurnal : String read FJurnal write SetJurnal;


    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, DateUnit;

procedure TfrmJurnale.SetJurnal(const Value: String);
begin
  qryJurnale.Locate('JURNAL', Value, []);
end;

procedure TfrmJurnale.treeUsersselPropertiesEditValueChanged(Sender: TObject);
var
  lJurnal,
  lIdUtilizator : Variant;
begin
  if not Assigned(treeUsers.FocusedNode) then Exit;
  lJurnal := qryUserJurnale.Params[0].Value;
  lIdUtilizator := TcxDBTreeListNode(treeUsers.FocusedNode).KeyValue;
  if TcxCheckBox(Sender).Checked then
    DBExecSQLFmt('insert into JURNAL_UTILIZATORI (JURNAL, ID_UTILIZATORI) values (%s, %s)',
        [ValueToStr(lJurnal), ValueToStr(lIdUtilizator)])
  else
    DBExecSQLFmt('delete from JURNAL_UTILIZATORI where JURNAL = %s and ID_UTILIZATORI = %s',
        [ValueToStr(lJurnal), ValueToStr(lIdUtilizator)])
end;

procedure TfrmJurnale.viewJurnalFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  if (AFocusedRecord <> nil) and (AFocusedRecord.IsData) then begin
    qryUserJurnale.Params[0].Value := AFocusedRecord.Values[viewJurnalJURNAL.Index];
    DBRefresh(qryUserJurnale);
  end;
end;

procedure TfrmJurnale.BtnOkClick(Sender: TObject);
begin
  if qryJurnale.State in [dsEdit, dsInsert] then
    qryJurnale.Post;

  if qryJurnale.UpdatesPending then
  begin
    qryJurnale.ApplyUpdates;
    qryJurnale.CommitUpdates;
  end;

  ModalResult := mrOk;
  Close;
end;



procedure TfrmJurnale.chkJurnalInchidereClick(Sender: TObject);
begin
    if qryJurnale.State = dsBrowse then
    qryJurnale.Edit;
end;



procedure TfrmJurnale.cxButton1Click(Sender: TObject);
begin
if not qryJurnale.Active then
    Exit;

  if not qryJurnale.IsEmpty then
    qryJurnale.Delete;
end;


procedure TfrmJurnale.cxButton3Click(Sender: TObject);
begin
       if not qryJurnale.Active then
    Exit;

  qryJurnale.Append;
end;

procedure TfrmJurnale.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmJurnale.FormCreate(Sender: TObject);
begin
  OpenDataSets(Self);

  chkJurnalInchidere.DataBinding.DataField := 'INCHIDERE';

  chkJurnalInchidere.Properties.ValueChecked := 1;
  chkJurnalInchidere.Properties.ValueUnchecked := 0;
  chkJurnalInchidere.Properties.AllowGrayed := False; // Fără stare gri
end;


end.
