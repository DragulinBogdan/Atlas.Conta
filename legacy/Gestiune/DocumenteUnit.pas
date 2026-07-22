unit DocumenteUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Db, ImgList, cxControls, cxContainer, cxEdit, cxCheckBox, cxDBEdit, 
  cxCustomData, cxGraphics, cxFilter, 
  cxDataStorage, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxMaskEdit, cxImageComboBox, cxMemo, Menus, cxLookAndFeelPainters,
  cxButtons,
  cxLookAndFeels, cxStyles, cxData, cxNavigator, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfrmDocumente = class(TForm)
    Imagini: TImageList;
    chkActive: TcxCheckBox;
    cxGridDocumenteL: TcxGridLevel;
    cxGridDocumente: TcxGrid;
    GridDocumente: TcxGridDBTableView;
    GridDocumenteCOD_DOCUM: TcxGridDBColumn;
    GridDocumenteDEN_DOCUM: TcxGridDBColumn;
    GridDocumentePOZITIE: TcxGridDBColumn;
    GridDocumenteTIP_PREDATOR: TcxGridDBColumn;
    GridDocumenteTIP_PRIMITOR: TcxGridDBColumn;
    GridDocumenteDESC_DOCUM: TcxGridDBColumn;
    GridDocumenteSUPORTA_FILIALA: TcxGridDBColumn;
    GridDocumenteSTARE: TcxGridDBColumn;
    GridDocumenteID: TcxGridDBColumn;
    BtnAdd: TcxButton;
    BtnModifica: TcxButton;
    BtnSterge: TcxButton;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    btnSave: TcxButton;
    edtEsteActiv: TcxDBCheckBox;
    procedure GridDocumenteLDblClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnModificaClick(Sender: TObject);
    procedure BtnStergeClick(Sender: TObject);
    procedure BtnAddClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure chkActiveClick(Sender: TObject);
    procedure GridDocumenteDblClick(Sender: TObject);
    procedure btnSaveClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnCancelClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile,
  FormulareUnit, ATSZDBUtils, {ModificareDocUnit,} NewModificareDocUnit, DateUnit,
  CommonDBVar;


procedure TfrmDocumente.GridDocumenteLDblClick(Sender: TObject);
begin
  BtnModifica.Click;
end;

procedure TfrmDocumente.FormCreate(Sender: TObject);
begin
  DBRefresh([frmData.QryDocumente, frmData.QryDefaDoc]);
  GridDocumente.ApplyBestFit(nil);
  chkActiveClick(nil);
end;

procedure TfrmDocumente.BtnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
  if not (fsModal in FormState) then
    Close;
end;

procedure TfrmDocumente.BtnModificaClick(Sender: TObject);
var
  lRecord: TcxCustomGridRecord;
begin
  lRecord := GridDocumente.Controller.FocusedRecord;
  if Assigned(lRecord) and (lRecord.IsData) and
    frmData.QryDocumente.Locate('ID_GEST_TIP_DOCUM', lRecord.Values[GridDocumenteID.Index], []) then begin
    with TfrmModificDocument.Create(nil) do
      try
         LoadRemainings;
         if (ShowModal = mrOk) then SaveRemainings
         else if FrmData.QryDocumente.State in [dsEdit, dsInsert] then FrmData.QryDocumente.Cancel;
      finally
         Free;
      end;
  end;
end;

procedure TfrmDocumente.BtnStergeClick(Sender: TObject);
var lNode: TcxCustomGridRecord;
begin
 lNode :=  GridDocumente.Controller.FocusedRecord;
  if (Assigned(lNode) and lNode.IsData) then
    if frmData.QryDocumente.Locate('ID_GEST_TIP_DOCUM', GetInteger(lNode, GridDocumenteID.Index), []) then
    begin
      lNode :=  GridDocumente.Controller.FocusedRecord;
     if (MessageDlg('Doriti stergerea documentului '+Trim(lNode.DisplayTexts[GridDocumenteCOD_DOCUM.Index])+' : '+
                 Trim(lNode.DisplayTexts[GridDocumenteDEN_DOCUM.Index])+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes)
     then
      DBExecSQLFmt('exec spGestDeleteTipDocum %d', [GetInteger(lNode, GridDocumenteID.Index)]);
      DBRefresh([frmData.QryDocumente, frmData.QryDefaDoc]);
    end;
end;

procedure TfrmDocumente.BtnAddClick(Sender: TObject);
var
  lfrmDocument: TfrmModificDocument;
begin
  lfrmDocument := TfrmModificDocument.Create(Self);
  try
    frmData.QryDocumente.Append;
    frmData.QryDocumente.FieldByName('ID_GEST_TIP_DOCUM').AsInteger := GetNextId('GEST_TIP_DOCUM');
    frmData.QryDocumente.FieldByName('STARE').AsBoolean := True;
    lfrmDocument.LoadRemainings;
    if lfrmDocument.ShowModal = mrOk then
      lfrmDocument.SaveRemainings
    else
      frmData.QryDocumente.Cancel;
  finally
    lfrmDocument.Free;
  end;
end;

procedure TfrmDocumente.BtnOkClick(Sender: TObject);
begin
  btnSave.Click;
  ModalResult := mrOk;
  if not (fsModal in FormState) then
    Close;
end;

procedure TfrmDocumente.chkActiveClick(Sender: TObject);
begin
  if not GridDocumente.DataController.Filter.Active then
    GridDocumente.DataController.Filter.Active := True; 
  GridDocumente.DataController.Filter.Root.Clear;
  if chkActive.Checked then begin
    GridDocumente.DataController.Filter.Root.AddItem(GridDocumenteSTARE, foEqual,  'True', 'Pozitiile active');
  end;
end;

procedure TfrmDocumente.GridDocumenteDblClick(Sender: TObject);
begin
  BtnModifica.Click;
end;

procedure TfrmDocumente.btnSaveClick(Sender: TObject);
begin
  DBPost([frmData.QryTipStock, frmData.QryDefaStock, frmData.QryDocumente, frmData.QryDefaDoc]);
end;

procedure TfrmDocumente.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

end.

