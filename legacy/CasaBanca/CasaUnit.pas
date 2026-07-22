unit CasaUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ComCtrls, RegistruUnit, VizualizareUnit, FiltruUnit, ExtCtrls, StdCtrls,
  Buttons, CommonCasa, ImgList, CommonDBVar, ContainerUnit,
  cxControls, cxPC, dxBar, cxGraphics, cxLookAndFeelPainters, cxLookAndFeels,
  dxBarBuiltInMenu;

Const
  StrSoldInitianl    = 'Sold initial la data %s : %s ';
  StrUserCurentLogat = '%s';
  StrInfo            = '%s';


type
  TfrmCasa = class(TForm)
    StatusImageList: TImageList;
    pnBottom: TPanel;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    btnApplyFilter: TSpeedButton;
    btnApplyDate: TSpeedButton;
    PageControl: TcxTabControl;
    pnClient: TPanel;
    lbSoldInitial: TLabel;
    btnMoveFiltered: TSpeedButton;
    btnMoveSelected: TSpeedButton;
    btnSaveNow: TSpeedButton;
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnApplyDateClick(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure FormDestroy(Sender: TObject);
    procedure btnApplyFilterClick(Sender: TObject);
    procedure PageControlChange(Sender: TObject);
    procedure btnMoveFilteredClick(Sender: TObject);
    procedure btnMoveSelectedClick(Sender: TObject);
    procedure btnSaveNowClick(Sender: TObject);
  private
    { Private declarations }
  protected
    function  CreazaForm(aTab : TTabSheet; aForm : TFormClass) : TForm; overload;
    function  CreazaForm(aForm : TFormClass) : TForm; overload;
    procedure SetCaptionOnForm (var Message : TMessage); message WM_SET_CAPTION;
  public
    { Public declarations }
    FrmRegistru : TFrmRegistru;
    FrmVisual   : TFrmListaCasa;
    FrmFiltru   : TFrmFiltru;
    OldTabIndex : Integer;

  end;

var
  frmCasa : TFrmCasa;


implementation

uses
  ZeosDBUtile, dxTL, ConcurentUsersUnit, cxDBTL, dxDBTL;

{$R *.DFM}

function TfrmCasa.CreazaForm(aTab: TTabSheet; aForm: TFormClass): TForm;
begin
  Result := aForm.Create(Self);
  with Result do begin
    Parent      := aTab;
    Align       := alClient;
    BorderStyle := bsNone;
    Visible     := True;
  end;
end;

procedure TfrmCasa.FormCreate(Sender: TObject);
begin
 if (Application.MainForm = nil) or
    (Application.MainForm.ClientHandle = 0) then
    FormStyle := fsNormal;

  frmCasaContainer  := TfrmCasaContainer.Create(Self);

  FrmRegistru       := TFrmRegistru(CreazaForm(TFrmRegistru));
  FrmVisual         := TFrmListaCasa(CreazaForm(TFrmListaCasa));
  FrmFiltru         := TFrmFiltru(CreazaForm(TFrmFiltru));

  FrmFiltru.DataSource  := FrmRegistru.DTRegistru;
  //FrmVisual.CasaHolder := (FrmRegistru.edCurentHouse);

  CasaHandle := frmRegistru.Handle;
  FiltruHandle := frmFiltru.Handle;
  ListaHandle := frmVisual.Handle;

  FrmRegistru.BringToFront;  
  OldTabIndex := 0;
  EtichetaHandle := Self.Handle;
//  StatusBar.Panels[0].Text := Format(StrUserCurentLogat, [NumeLoginComplet + '(' + NumeLogin+ ')']);
  PostMessage(Handle, WM_SET_CAPTION, 0, 0);
  WindowState := wsMaximized;
end;

procedure TfrmCasa.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmCasa.btnApplyDateClick(Sender: TObject);
begin
  if not Assigned(FrmRegistru) then Exit;
  FrmFiltru.SetDatasetFilter;
  FrmRegistru.StartInterval := FrmFiltru.StartDate;
  FrmRegistru.EndInterval   := FrmFiltru.EndDate;
  FrmRegistru.RefreshDataSet;

end;

procedure TfrmCasa.btnOkClick(Sender: TObject);
begin
  if Assigned(FrmRegistru) then FrmRegistru.SaveToDb;
  Close;
end;

procedure TfrmCasa.btnCancelClick(Sender: TObject);
begin
  if Assigned(FrmRegistru) then begin
    FrmRegistru.IsModified := False;
    FrmRegistru.DeleteShare;
  end;
  Close;
end;

procedure TfrmCasa.FormCloseQuery(Sender: TObject; var CanClose: Boolean);
begin
  CanClose := False;
  FrmRegistru.CheckForSave;
  CanClose := True;
end;

procedure TfrmCasa.FormDestroy(Sender: TObject);
begin
  ExitSingleUser;
end;

procedure TfrmCasa.btnApplyFilterClick(Sender: TObject);
begin
  if not Assigned(FrmRegistru) then Exit;
  FrmRegistru.Filter := FrmFiltru.Filter;

end;

function TfrmCasa.CreazaForm(aForm: TFormClass): TForm;
begin
  Result := aForm.Create(Self);
  with Result do begin
    Parent      := pnClient;
    Align       := alClient;
    BorderStyle := bsNone;
    Visible     := True;
  end;
end;

procedure TfrmCasa.PageControlChange(Sender: TObject);
begin

   OldTabIndex := PageControl.TabIndex;
end;



procedure TfrmCasa.btnMoveFilteredClick(Sender: TObject);
begin
  if FrmVisual.MemLista.Active then FrmVisual.MemLista.Active := False;
  FrmVisual.MemLista.Active := True;
  FrmVisual.MemLista.LoadFromDataSet(FrmRegistru.MemRegistru);

end;

procedure TfrmCasa.btnMoveSelectedClick(Sender: TObject);
var
  I:Integer;
begin
  if FrmVisual.MemLista.Active then FrmVisual.MemLista.Active := False;
  FrmVisual.MemLista.Active := True;
  FrmRegistru.GridRegistru.BeginUpdate;
  for I := 0 to FrmRegistru.GridRegistru.SelectedCount -1 do begin
    if FrmRegistru.MemRegistru.Locate('ID_LISTA', FrmRegistru.GridRegistru.SelectedNodes[I].Values[FrmRegistru.GridRegistruID_LISTA.Index], []) then
      DBCopyRecordWithEdit(FrmRegistru.MemRegistru, FrmVisual.MemLista);
  end;
  FrmRegistru.GridRegistru.EndUpdate;
    pnBottom.Visible := True;
pnBottom.BringToFront;
pnBottom.Repaint;
Application.ProcessMessages;

end;

procedure TfrmCasa.btnSaveNowClick(Sender: TObject);
begin
  FrmRegistru.SaveNow;
   pnBottom.Visible := True;
  pnBottom.Align := alNone;
  pnBottom.Align := alBottom;
  pnBottom.Repaint;
  Application.ProcessMessages;
end;


procedure TfrmCasa.SetCaptionOnForm(var Message: TMessage);
begin
   Caption := FrmRegistru.Caption;
end;

end.


