unit UnitInchiderePer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls, DB, ZDataSet, 
  DBCtrls, cxCheckBox, cxDBEdit, cxControls, cxContainer, cxEdit,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxCalendar, 
  cxLabel, cxDBLabel, Menus, DegradePanel, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxLookAndFeels, ZSqlUpdate, cxStyles, cxCustomData, cxFilter,
  cxData, cxDataStorage, cxDBData, cxGridLevel, cxClasses,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmInchiderePerioada = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    DTPerioadeFiscale: TDataSource;
    QryPerioadeFiscale: TZQuery;
    pnTop: TDegradePanel;
    GridInchidere: TcxGridDBTableView;
    GridInchidereLevel1: TcxGridLevel;
    cxGridInchidere: TcxGrid;
    pnContext: TPanel;
    Bevel: TBevel;
    Label1: TLabel;
    Label2: TLabel;
    lbDataInchidere: TLabel;
    cxDBDateEdit1: TcxDBDateEdit;
    cxDBDateEdit2: TcxDBDateEdit;
    chkInchidere: TcxDBCheckBox;
    edDataInchidere: TcxDBDateEdit;
    cxDBTextEdit1: TcxDBTextEdit;
    lbID: TcxDBLabel;
    GridInchidereAnFiscal: TcxGridDBColumn;
    GridInchidereDataStart: TcxGridDBColumn;
    GridInchidereDataStop: TcxGridDBColumn;
    GridInchidereInchisa: TcxGridDBColumn;
    GridInchidereDataInchidere: TcxGridDBColumn;
    GridInchideretrim: TcxGridDBColumn;
    GridInchidereUtilizatorInchidere: TcxGridDBColumn;
    GridInchidereID: TcxGridDBColumn;
    GridInchidereluna: TcxGridDBColumn;
    GridInchidereblocata: TcxGridDBColumn;
    GridInchidereDataBlocare: TcxGridDBColumn;
    GridInchidereUtilizatorBlocare: TcxGridDBColumn;
    lbUtilizatorInchidere: TcxDBLabel;
    chkBlocare: TcxDBCheckBox;
    lbDataBlocare: TLabel;
    edDataBlocare: TcxDBDateEdit;
    lbUtilizatorBlocare: TcxDBLabel;
    procedure FormCreate(Sender: TObject);
    procedure chkInchidereClick(Sender: TObject);
    procedure chkBlocarePropertiesChange(Sender: TObject);
    procedure chkInchiderePropertiesChange(Sender: TObject);
  private
    { Private declarations }
    FIsInLoading : Boolean;
    procedure InchisaChange(Sender : TField);
    procedure BlocataChange(Sender : TField);
  public
    { Public declarations }
    procedure OpenDataSet;
  end;


procedure IntretinerePerioadeFiscale;

var
  frmInchiderePerioada: TfrmInchiderePerioada;

implementation

uses
  Math, ZeosDBUtile, DateUnit, CommonDBVar;

{$R *.dfm}

procedure IntretinerePerioadeFiscale;
begin
  frmInchiderePerioada := TfrmInchiderePerioada.Create(nil);
  with frmInchiderePerioada do
    try
      ShowModal;
    finally
      Free;
    end;
end;

procedure TfrmInchiderePerioada.FormCreate(Sender: TObject);
begin
  OpenDataSet;
end;

procedure TfrmInchiderePerioada.OpenDataSet;
var
   lId : Integer;
begin
  lId := -1;
  if (QryPerioadeFiscale.Active) and not (QryPerioadeFiscale.IsEmpty) then
    lId := QryPerioadeFiscale.FieldByName('ID').AsInteger;
  QryPerioadeFiscale.Close;    
  QryPerioadeFiscale.DisableControls;
  FIsInLoading := True;
  DBRefresh(QryPerioadeFiscale);
  QryPerioadeFiscale.FieldByName('inchisa').OnChange := InchisaChange;
  QryPerioadeFiscale.FieldByName('inchisa').ReadOnly := False;
  QryPerioadeFiscale.FieldByName('blocata').OnChange := BlocataChange;
  QryPerioadeFiscale.FieldByName('blocata').ReadOnly := False;
  FIsInLoading := False;
  QryPerioadeFiscale.EnableControls;
  if lId <> -1 then
    QryPerioadeFiscale.Locate('ID', lId, []);
end;



procedure TfrmInchiderePerioada.chkInchidereClick(Sender: TObject);
begin
  if not FIsInLoading then begin
    DBExecSQLFmt('exec [spInchiderePerioadeFiscale] %s, %d', [ValueToStr(lbID.EditValue), IfThen(chkInchidere.Checked, 1, 0)]);
    OpenDataSet;
  end;
end;


procedure TfrmInchiderePerioada.chkBlocarePropertiesChange(
  Sender: TObject);
begin
  if FIsInLoading then Exit;
  chkInchidere.Enabled := not(chkBlocare.Checked);
  edDataInchidere.Enabled :=  chkInchidere.Enabled;
  lbUtilizatorBlocare.Enabled :=  chkInchidere.Enabled;
  lbDataInchidere.Enabled := chkInchidere.Enabled;
end;

procedure TfrmInchiderePerioada.chkInchiderePropertiesChange(
  Sender: TObject);
begin
  if FIsInLoading then Exit;
  chkBlocare.Visible := chkInchidere.Checked;
  edDataBlocare.Visible := chkBlocare.Visible;
  lbUtilizatorBlocare.Visible := chkBlocare.Visible;
  lbDataBlocare.Visible := chkBlocare.Visible;
end;

procedure TfrmInchiderePerioada.InchisaChange(Sender: TField);
var
  lIdPerioada : Integer;
begin
  if not FIsInLoading then begin
    DBExecSQLFmt('exec [spInchiderePerioadeFiscale] %s, %d, %d', [ValueToStr(Sender.DataSet['ID']), IfThen(Sender.AsBoolean, 1, 0), IdUtilizator]);
    OpenDataSet;
  end;
end;

procedure TfrmInchiderePerioada.BlocataChange(Sender: TField);
var
  lIdPerioada : Integer;
begin
  lIdPerioada := Sender.DataSet.FieldByName('ID').AsInteger;
  if not Sender.AsBoolean then begin
    if (not Sender.DataSet.FieldByName('ID_Utilizatori_Blocare').AsInteger = IdUtilizator) then begin
      ShowEroare('Nu puteti debloca perioada. Perioada este blocata de utilizatorul ' + Sender.DataSet.FieldByName('UtilizatorBlocare').AsString + '. Va rugam contactati-l pentru a debloca perioada !');
    end else begin
      DBExecSQLFmt('exec spBlocarePerioadeFiscale %d, 0, %d', [lIdPerioada, IdUtilizator]);
    end;
  end else begin
    DBExecSQLFmt('exec spBlocarePerioadeFiscale %d, 1, %d', [lIdPerioada, IdUtilizator]);
  end;
  OpenDataSet;
end;

end.
