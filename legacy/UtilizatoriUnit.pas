unit UtilizatoriUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, Buttons, dxDBTLCl,
  dxGrClms, dxExEdtr, ChangePassUnit, Menus, cxLookAndFeelPainters,
  cxButtons, ExtCtrls, DegradePanel,
  cxGraphics,
  cxLookAndFeels, cxControls, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, cxNavigator, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog, Data.DB, cxDBData,
  cxMaskEdit, cxImageComboBox, cxCheckBox, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView,
  cxGrid, dxLayoutContainer, cxGridInplaceEditForm, dxScrollbarAnnotations;

type

  TfrmUtilizatori = class(TForm)
    btnAdd: TcxButton;
    btnDel: TcxButton;
    btnSetareParola: TcxButton;
    pnTop: TDegradePanel;
    BtnOk: TcxButton;
    btnModifica: TcxButton;
    viewUsers: TcxGridDBTableView;
    nivelUsers: TcxGridLevel;
    gridUsers: TcxGrid;
    viewUsersNUME: TcxGridDBColumn;
    viewUsersNUMEINTREG: TcxGridDBColumn;
    viewUsersDREPTURI: TcxGridDBColumn;
    viewUsersSTARE: TcxGridDBColumn;
    viewUsersID_FUNCTIUNI: TcxGridDBColumn;
    viewUsersID_LST_UNITATI: TcxGridDBColumn;
    procedure btnSetareParolaClick(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnModificaClick(Sender: TObject);
    procedure viewUsersFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
  private
    { Private declarations }

  public
    { Public declarations }
  end;


implementation

{$R *.DFM}

uses
  DateUnit, ZeosDBUtile, dxCompsUtile, CommonDBVar, ATSZDBUtils;

procedure TfrmUtilizatori.btnSetareParolaClick(Sender: TObject);
var
  lPassword: String;
begin
  if ChangePassword(FrmData.QryOperatori.FieldByName('PAROLA').AsString, FrmData.QryOperatori.FieldByName('NUMEINTREG').AsString, lPassword, nil, 'Contabilitate') then
    DBSetFieldValue(FrmData.QryOperatori, 'PAROLA', lPassword);
end;

procedure TfrmUtilizatori.btnAddClick(Sender: TObject);
begin
  viewUsers.DataController.Append;
end;

procedure TfrmUtilizatori.btnDelClick(Sender: TObject);
begin
  viewUsers.DataController.DeleteFocused;
end;

procedure TfrmUtilizatori.btnModificaClick(Sender: TObject);
begin
  viewUsers.DataController.Edit;
end;

procedure TfrmUtilizatori.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBPost(frmData.QryOperatori);
  Action := caFree;
end;

procedure TfrmUtilizatori.FormCreate(Sender: TObject);
begin
  FillImageCombo(viewUsersID_FUNCTIUNI.Properties, frmData.QryFunctiuni, 'ID_FUNCTIUNI', 'DENUMIRE');
  FillImageCombo(viewUsersID_LST_UNITATI.Properties, frmData.qryOIUnitati, 'ID_OI_UNITATI', 'DENUMIRE');
end;

procedure TfrmUtilizatori.viewUsersFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  btnDel.Enabled := Assigned(AFocusedRecord);
  btnModifica.Enabled := btnDel.Enabled;
  btnSetareParola.Enabled := btnDel.Enabled;
end;

procedure TfrmUtilizatori.BtnOkClick(Sender: TObject);
begin
  Close;
end;

end.
