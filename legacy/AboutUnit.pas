unit AboutUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, CommonDBVar, registry, jpeg, ExtCtrls, svnInfo, Menus,
  cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels, cxControls, cxContainer, cxEdit, cxLabel, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxImageComboBox;

type
  TfrmAbout = class(TForm)
    lbVersiune: TLabel;
    lbUpdate: TLabel;
    Image1: TImage;
    lbsvnRevision: TLabel;
    lbsvnDate: TLabel;
    lbsvnCompilat: TLabel;
    lbsvnURL: TLabel;
    btnUpdate: TcxButton;
    btnOk: TcxButton;
    cxLabel1: TcxLabel;
    cxLabel2: TcxLabel;
    edUpdate: TcxImageComboBox;
    procedure btnOkClick(Sender: TObject);
    procedure edUpdateChange(Sender: TObject);
    procedure btnUpdateClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshSelection;
  end;

procedure ShowAboutForm;

implementation

uses
  ZeosDBUtile,
  DateUnit;

{$R *.DFM}

procedure ShowAboutForm;
var
  lAbout : TfrmAbout;
begin
  lAbout := TfrmAbout.Create(nil);
  try
    lAbout.ShowModal;
  finally
    lAbout.Free;
  end;
end;

procedure TfrmAbout.btnOkClick(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TfrmAbout.RefreshSelection;
Var I: Integer;
begin
  edUpdate.Clear;
  edUpdate.Properties.Items.Clear;
  frmdata.RefreshUpdateLocations(frmdata.dbContabilitate);
  for I:= 0 to frmdata.FUpdateLocations.Count-1 do begin
     with edUpdate.Properties.Items.Add do begin
        Value := frmdata.FUpdateLocations[I];
        Description := frmdata.FUpdateDescriptions[I];
        Tag := Integer(Pointer(frmData.FUpdateLocations.Objects[I]));
     end;
  end;
  if frmdata.FUpdateLocations.Count > 0 then
    edUpdate.EditValue := frmdata.FUpdateLocations[0];
  edUpdate.Visible := (frmdata.FUpdateLocations.Count>0);
  btnUpdate.Visible := (frmdata.FUpdateLocations.Count>0);
  lbUpdate.Visible := (frmdata.FUpdateLocations.Count>0);
end;

procedure TfrmAbout.edUpdateChange(Sender: TObject);
begin
  btnUpdate.Enabled := (Trim(edUpdate.Text) <> '');
end;

procedure TfrmAbout.btnUpdateClick(Sender: TObject);
var UpdateLocation : TUpdateLocation;
    Reg : TRegistry;
    OldDate : String;
    lItem : TcxImageComboBoxItem;
begin
  lItem := cxFindItemByComboValue(edUpdate.Properties.Items, edUpdate.EditValue);
  if lItem = nil then Exit;
  UpdateLocation :=TUpdateLocation(lItem.Tag);
  Reg := TRegistry.Create;
  Reg.RootKey := HKEY_CURRENT_USER;
  OldDate := '';
  try
    if Reg.OpenKey(frmdata.AutoUpdate.LastURLEntry.Key+'\'+frmdata.AutoUpdate.LastURLEntry.Section,False) then begin
      if Reg.ValueExists(frmdata.AutoUpdate.LastURLEntry.Section) then
        OldDate := Reg.ReadString(frmdata.AutoUpdate.LastURLEntry.Section);
        Reg.DeleteValue(frmdata.AutoUpdate.LastURLEntry.Section);
    end;
  except
  end;
  frmData.UpdExecuteClasic := False;
  frmData.IsUpdateError := False;
  frmdata.DoUpdateFromLocation(UpdateLocation, True);

  if frmData.IsUpdateError  then
    if (OldDate <> '') then
     if Reg.OpenKey(frmdata.AutoUpdate.LastURLEntry.Key+'\'+frmdata.AutoUpdate.LastURLEntry.Section,False) then
        Reg.WriteString(frmdata.AutoUpdate.LastURLEntry.Section, OldDate);

  if frmData.IsRestarting then
      bIsCanceling := True;
  Reg.Free;
  frmData.UpdExecuteClasic := True;
end;

procedure TfrmAbout.FormShow(Sender: TObject);
begin
  lbVersiune.Caption    := Format(lbVersiune.Hint, [ExeVersion]);
  lbsvnRevision.Caption := Format(lbsvnRevision.Hint, [svnRevision]);
  lbsvnDate.Caption     := Format(lbsvnDate.Hint, [svnDate]);
  lbsvnCompilat.Caption := Format(lbsvnCompilat.Hint, [svnMixed]);
  lbsvnURL.Caption      := Format(lbsvnURL.Hint, [svnURL]);

  lbVersiune.Visible    := DelphiRunning;
  lbsvnRevision.Visible := lbVersiune.Visible;
  lbsvnDate.Visible     := lbVersiune.Visible;
  lbsvnCompilat.Visible := lbVersiune.Visible;
  lbsvnURL.Visible      := lbVersiune.Visible;
  RefreshSelection;
end;

procedure TfrmAbout.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

end.
