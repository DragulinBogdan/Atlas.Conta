unit MijloaceTransportUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ExtCtrls, dxCntner, dxEditor, dxExEdtr, dxEdLib,
  dxTL, dxDBCtrl, dxDBGrid, dxDBTLCl, dxGrClms, DB, cxLookAndFeelPainters,
  cxButtons, ActnList, Menus, DegradePanel,
  cxGraphics, cxLookAndFeels;

type
  TfrmMijTransport = class(TForm)
    Panel1: TPanel;
    Panel2: TPanel;
    Label1: TLabel;
    edRepartitor: TdxImageEdit;
    gridMijloaceTransport: TdxDBGrid;
    gridMijloaceTransportID_REPARTITORI_TRANSPORT: TdxDBGridMaskColumn;
    gridMijloaceTransportCOD: TdxDBGridMaskColumn;
    gridMijloaceTransportID_REPARTITORI: TdxDBGridMaskColumn;
    gridMijloaceTransportNUMAR_AUTO: TdxDBGridMaskColumn;
    gridMijloaceTransportDEFAULT: TdxDBGridCheckColumn;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    gridMijloaceTransportTIP_MASINA: TdxDBGridMRUColumn;
    ActionList: TActionList;
    Cmd_AddAuto: TAction;
    Cmd_DelAuto: TAction;
    btnAddDelegat: TcxButton;
    btnDelDelegat: TcxButton;
    pnTop: TDegradePanel;
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Cmd_AddAutoExecute(Sender: TObject);
    procedure Cmd_DelAutoExecute(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormDestroy(Sender: TObject);
    procedure edRepartitorChange(Sender: TObject);
  private
    FIdRepartitor: Integer;
    { Private declarations }
    procedure PopulateImage(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc: String);
    procedure SetMijlocTransportFilter(AFilter : String);
    procedure SetIdRepartitor(const Value: Integer);
    procedure QryMijlTransportNewRecord(DataSet: TDataSet);
    procedure PopulateMRUTipMasina;
  public
    { Public declarations }
    procedure RefreshDataSet;
    property IdRepartitor : Integer read FIdRepartitor write SetIdRepartitor;
  end;

procedure ShowMijlocTransport(const AIDRepartitor: Integer = -1);

implementation

uses
  DateUnit, ZDataSet;

{$R *.dfm}

procedure ShowMijlocTransport(const AIDRepartitor: Integer = -1);
var
  lMijloc: TfrmMijTransport;
begin
  lMijloc := TfrmMijTransport.Create(nil);
  try
    if AIDRepartitor <> -1 then lMijloc.IdRepartitor := AIDRepartitor;
    lMijloc.ShowModal;
  finally
    lMijloc.Free;
  end;
end;

procedure TfrmMijTransport.BtnOkClick(Sender: TObject);
begin
  if FrmData.QryMijTransport.State in [dsEdit, dsInsert] then
    FrmData.QryMijTransport.Post; 
  ModalResult := mrOk;
end;

procedure TfrmMijTransport.BtnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmMijTransport.PopulateImage(aDataSet: TDataSet; aValues,
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

procedure TfrmMijTransport.SetMijlocTransportFilter(AFilter: String);
var NewFiltered : Boolean;
begin
  with frmData.QryMijTransport do begin
    NewFiltered := Filter <> AFilter;
    if NewFiltered then Filter := AFilter;
    Filtered := Filter <> '';
  end;
end;

procedure TfrmMijTransport.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  PopulateImage(frmData.QryRepartitori,edRepartitor.Values, edRepartitor.Descriptions,'ID_REPARTITORI','NUME');
  edRepartitor.Text := FrmData.QryRepartitori.FieldByName('ID_REPARTITORI').AsString;
  SetMijlocTransportFilter('ID_REPARTITORI='+edRepartitor.Text);
  PopulateMRUTipMasina;
  FrmData.QryMijTransport.OnNewRecord := QryMijlTransportNewRecord;  
end;

procedure TfrmMijTransport.RefreshDataSet;
var
  OldFilter : String;
begin
  OldFilter := frmData.QryMijTransport.Filter;
  if frmData.QryMijTransport.Active then frmData.QryMijTransport.Close;
  frmData.QryMijTransport.Open;
  if OldFilter <> '' then begin
    frmData.QryMijTransport.Filter := OldFilter;
    frmData.QryMijTransport.Filtered := True;
  end;
end;


procedure TfrmMijTransport.SetIdRepartitor(const Value: Integer);
begin
  FIdRepartitor := Value;
  edRepartitor.Text := IntToStr(Value);
end;

procedure TfrmMijTransport.QryMijlTransportNewRecord(DataSet: TDataSet);
begin
   DataSet.FieldByName('ID_REPARTITORI').AsInteger := StrToInt(edRepartitor.Text);
end;

procedure TfrmMijTransport.Cmd_AddAutoExecute(Sender: TObject);
begin
  if (edRepartitor.Text = '') or (StrToInt(edRepartitor.Text) < 0) then begin
    MessageDlg('Nu aveti selectat nici un repartitor ! Selectati un repartitor inainte de a adauga un delegat.', mtError, [mbOK], 0);
    Exit;
  end;
  with frmData.QryMijTransport do begin
    Append;
    FieldByName('DEFAULT').AsBoolean := False;
    Post;
    Edit;
  end;
end;



procedure TfrmMijTransport.Cmd_DelAutoExecute(Sender: TObject);
var
  aNode : TdxTreeListNode;
  lNumeRepartitor, lNumeDelegat : String;
  lId  : Integer;
  aQry : TZReadOnlyQuery;
begin
  aNode := gridMijloaceTransport.FocusedNode;
  if aNode = nil then begin
    MessageDlg('Nu ati selectat nici un delegat pentru stergere !', mtError, [mbOK], 0);
    Exit;
  end;


  //ne pozitionam in dataset pe lId
  lId := -1;
  if aNode.Strings[gridMijloaceTransportID_REPARTITORI_TRANSPORT.Index]<> '' then begin
    lId := aNode.Values[gridMijloaceTransportID_REPARTITORI_TRANSPORT.Index];
    aQry := GetTmpADOQuery;
    with aQry do
      try
        SQL.Add('EXEC SP_GEST_VERIFICA_AUTO '+ IntToStr(lId));
        Open;
        if Fields[0].AsInteger = 1 then begin
           MessageDlg('Acest autovehicul este legat de un document emis. Aceasta inregistrare nu se poate stege !', mtConfirmation, [mbOK], 0);
           Exit;
        end;
      finally
        Free;
      end;
  end;
  if lId <> -1 then begin
    //numele auto
    if edRepartitor.Values.IndexOf(edRepartitor.Text) > 0 then
      lNumeRepartitor := edRepartitor.Descriptions.Strings[edRepartitor.Values.IndexOf(edRepartitor.Text)]
    else
      lNumeRepartitor := '';
    lNumeDelegat := aNode.Strings[gridMijloaceTransportNUMAR_AUTO.Index];
    //confirmarea stergerii
    if (MessageDlg(Format('Doriti stergerea autovehiculului %s asociat repartitorului %s ? ',[lNumeDelegat, lNumeRepartitor]), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
      if FrmData.QryMijTransport.Locate('ID_REPARTITORI_TRANSPORT', lId, []) then FrmData.QryMijTransport.Delete;
  end
  else
    //a fost sters deja ?
    RefreshDataSet;
end;

procedure TfrmMijTransport.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmMijTransport.FormDestroy(Sender: TObject);
begin
  FrmData.QryMijTransport.OnNewRecord := nil;
end;

procedure TfrmMijTransport.PopulateMRUTipMasina;
var
  aQry : TZReadOnlyQuery;
begin
  aQry := GetTmpADOQuery;
  with aQry do
    try
      SQL.Add('EXEC SP_GEST_MRULIST_AUTO');
      Open;
      gridMijloaceTransportTIP_MASINA.Items.Clear;
      First;
      if not IsEmpty then
        while not eof do begin
          gridMijloaceTransportTIP_MASINA.Items.Add(Fields[0].AsString);
          Next;
        end;
    finally
      Free;
    end;
end;

procedure TfrmMijTransport.edRepartitorChange(Sender: TObject);
begin
  SetMijlocTransportFilter('ID_REPARTITORI='+edRepartitor.Text);
end;

end.
