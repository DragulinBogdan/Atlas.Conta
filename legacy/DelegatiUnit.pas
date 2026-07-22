unit DelegatiUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ExtCtrls, dxCntner, dxEditor, dxExEdtr, dxEdLib,
  dxTL, dxDBCtrl, dxDBGrid, dxDBTLCl, dxGrClms, DB,
  cxLookAndFeelPainters,
  cxButtons, ActnList, Menus, DegradePanel,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmDelegati = class(TForm)
    Panel1: TPanel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    Panel2: TPanel;
    Label1: TLabel;
    edRepartitor: TdxImageEdit;
    gridDelegatii: TdxDBGrid;
    gridDelegatiiID_REPARTITORI_DELEGATI: TdxDBGridMaskColumn;
    gridDelegatiiID_REPARTITORI: TdxDBGridMaskColumn;
    gridDelegatiiCOD: TdxDBGridMaskColumn;
    gridDelegatiiNUME_DELEGAT: TdxDBGridMaskColumn;
    gridDelegatiiPRENUME_DELEGAT: TdxDBGridMaskColumn;
    gridDelegatiiNUME_COMPLET: TdxDBGridMaskColumn;
    gridDelegatiiCNP_DELEGAT: TdxDBGridMaskColumn;
    gridDelegatiiSERIE_BI: TdxDBGridMaskColumn;
    gridDelegatiiNR_BI: TdxDBGridMaskColumn;
    gridDelegatiiDATA_BI: TdxDBGridDateColumn;
    gridDelegatiiDEFAULT: TdxDBGridCheckColumn;
    gridDelegatiiELIBERAT_BI: TdxDBGridMaskColumn;
    gridDelegatiiTIP_BI: TdxDBGridImageColumn;
    btnAddDelegat: TcxButton;
    btnDelDelegat: TcxButton;
    ActionList: TActionList;
    Cmd_AddDelegat: TAction;
    Cmd_DelDelegat: TAction;
    pnTop: TDegradePanel;
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure edRepartitorChange(Sender: TObject);
    procedure Cmd_AddDelegatExecute(Sender: TObject);
    procedure Cmd_DelDelegatExecute(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormDestroy(Sender: TObject);
  private
    FIdRepartitor: Integer;
    procedure PopulateImage(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc: String);
    procedure SetDelegatiFilter(AFilter : String);
    procedure SetIdRepartitor(const Value: Integer);
    procedure QryDelegatiNewRecord(DataSet: TDataSet);
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshDataSet;
    property IdRepartitor : Integer read FIdRepartitor write SetIdRepartitor;
  end;

procedure ShowDelegatii(const AIDRepartitor: Integer = -1);

implementation

uses DateUnit, ZDataSet;

{$R *.dfm}

procedure ShowDelegatii(const AIDRepartitor: Integer = -1);
var
  lDelegatii: TfrmDelegati;
begin
  lDelegatii := TfrmDelegati.Create(nil);
  try
    if AIDRepartitor <> -1 then lDelegatii.IdRepartitor := AIDRepartitor;
    lDelegatii.ShowModal;
  finally
    lDelegatii.Free;
  end;
end;

procedure TfrmDelegati.BtnOkClick(Sender: TObject);
begin
  if FrmData.QryDelegati.State in [dsEdit, dsInsert] then
    FrmData.QryDelegati.Post; 
  ModalResult := mrOk;
end;

procedure TfrmDelegati.BtnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmDelegati.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  PopulateImage(frmData.QryRepartitori, edRepartitor.Values, edRepartitor.Descriptions,'ID_REPARTITORI','NUME');
  edRepartitor.Text := FrmData.QryRepartitori.FieldByName('ID_REPARTITORI').AsString;
  SetDelegatiFilter('ID_REPARTITORI='+edRepartitor.Text);
  FrmData.QryDelegati.OnNewRecord := QryDelegatiNewRecord;
end;

procedure TfrmDelegati.PopulateImage(aDataSet: TDataSet; aValues,
  aDescs: TStrings; aValue, aDesc: String);
var OldPoz : TBookmark;
    lValField, lDescField : TField;  
begin
  aValues.Clear;
  aDescs.Clear;
  lValField := aDataSet.FindField(aValue);
  lDescField := aDataSet.FindField(aDesc);

  if not Assigned(lValField) then Exit;
  if not Assigned(lDescField) then lDescField := lValField;
  with aDataSet do begin
    OldPoz := GetBookmark;
    DisableControls;
    try
       First;
       while not Eof do begin
         aValues.Add(lValField.AsString);
         aDescs.Add(lDescField.AsString);
         Next;
       end;
    finally
       GotoBookmark(OldPoz);
       FreeBookmark(OldPoz);
       EnableControls;
    end;
  end;
end;

procedure TfrmDelegati.SetDelegatiFilter(AFilter: String);
var  NewFiltered : Boolean;
begin
  with frmData.QryDelegati do begin
    NewFiltered := Filter <> AFilter;
    if NewFiltered then Filter := AFilter;
    Filtered := Filter <> '';
  end;
end;

procedure TfrmDelegati.SetIdRepartitor(const Value: Integer);
begin
  FIdRepartitor := Value;
  edRepartitor.Text := IntToStr(Value);
end;

procedure TfrmDelegati.edRepartitorChange(Sender: TObject);
begin
  SetDelegatiFilter('ID_REPARTITORI='+edRepartitor.Text);
end;

procedure TfrmDelegati.QryDelegatiNewRecord(DataSet: TDataSet);
begin
   DataSet.FieldByName('ID_REPARTITORI').AsInteger := StrToInt(edRepartitor.Text);
end;

procedure TfrmDelegati.Cmd_AddDelegatExecute(Sender: TObject);
begin
  if (edRepartitor.Text = '') or (StrToInt(edRepartitor.Text) < 0) then begin
    MessageDlg('Nu aveti selectat nici un repartitor ! Selectati un repartitor inainte de a adauga un delegat.', mtError, [mbOK], 0);
    Exit;
  end;
  with frmData.QryDelegati do begin
    Append;
    FieldByName('DEFAULT').AsBoolean := False;
    Post;
    Edit;
  end;
end;

procedure TfrmDelegati.Cmd_DelDelegatExecute(Sender: TObject);
var
  aNode : TdxTreeListNode;
  lNumeRepartitor, lNumeDelegat : String;
  lId  : Integer;
  aQry : TZReadOnlyQuery;
begin
  aNode := gridDelegatii.FocusedNode;
  if aNode = nil then begin
    MessageDlg('Nu ati selectat nici un delegat pentru stergere !', mtError, [mbOK], 0);
    Exit;
  end;


  //ne pozitionam in dataset pe lId
  lId := -1;
  if aNode.Strings[gridDelegatiiID_REPARTITORI_DELEGATI.Index]<> '' then begin
    lId := aNode.Values[gridDelegatiiID_REPARTITORI_DELEGATI.Index];
    aQry := GetTmpADOQuery;
    with aQry do
      try
        SQL.Add('EXEC SP_GEST_VERIFICA_DEL '+ IntToStr(lId));
        Open;
        if Fields[0].AsInteger = 1 then begin
           MessageDlg('Acest delegat este legat de un document emis. Aceasta inregistrare nu se poate stege !', mtConfirmation, [mbOK], 0);
           Exit;
        end;
      finally
        Free;
      end;
  end;
  if lId <> -1 then begin
    //numele repartitorului
    if edRepartitor.Values.IndexOf(edRepartitor.Text) > 0 then
      lNumeRepartitor := edRepartitor.Descriptions.Strings[edRepartitor.Values.IndexOf(edRepartitor.Text)]
    else
      lNumeRepartitor := '';
    lNumeDelegat := aNode.Strings[gridDelegatiiNUME_DELEGAT.Index];
    //confirmarea stergerii
    if (MessageDlg(Format('Doriti stergerea delegatului %s asociat repartitorului %s ? ',[lNumeDelegat, lNumeRepartitor]), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
      if FrmData.QryDelegati.Locate('ID_REPARTITORI_DELEGATI', lId, []) then FrmData.QryDelegati.Delete;
  end
  else
    //a fost sters deja ?
    RefreshDataSet;
end;

procedure TfrmDelegati.RefreshDataSet;
var
  OldFilter : String;
begin
  OldFilter := frmData.QryDelegati.Filter;
  if frmData.QryDelegati.Active then frmData.QryDelegati.Close;
  frmData.QryDelegati.Open;
  if OldFilter <> '' then begin
    frmData.QryDelegati.Filter := OldFilter;
    frmData.QryDelegati.Filtered := True;
  end;
end;

procedure TfrmDelegati.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmDelegati.FormDestroy(Sender: TObject);
begin
  FrmData.QryDelegati.OnNewRecord := nil;
end;

end.
