unit frmIntretinereJudete;

interface

uses
  System.SysUtils, System.Classes, Vcl.Forms, Data.DB,
  ZAbstractRODataset, ZAbstractDataset, ZDataset,
  cxGrid, cxGridDBTableView, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  Vcl.StdCtrls, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, cxStyles,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, cxNavigator,
  dxDateRanges, dxScrollbarAnnotations, cxDBData, Vcl.Controls, Dialogs, cxButtons;

type
  TfrmIntretinereJudete = class(TForm)
    ZQuery1: TZQuery;
    DataSource1: TDataSource;
    cxGrid1: TcxGrid;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1Level1: TcxGridLevel;
    cxGrid1DBTableView1Column1: TcxGridDBColumn;
    cxGrid1DBTableView1Column2: TcxGridDBColumn;
    procedure FormCreate(Sender: TObject);
    procedure FormShow(Sender: TObject);
  private
    btnClose: TcxButton;
    procedure btnCloseClick(Sender: TObject);
  end;

implementation

{$R *.dfm}

uses
  DateUnit;

procedure TfrmIntretinereJudete.FormCreate(Sender: TObject);
begin

  Self.BorderStyle := bsNone;
  Self.WindowState := wsMaximized;

  ShowMessage('Se inițializează frmIntretinereJudete...');
end;

procedure TfrmIntretinereJudete.FormShow(Sender: TObject);
begin
  try
     Self.BorderStyle := bsNone;
    Self.Left := 0;
    Self.Top := 50;
    Self.Width := Screen.WorkAreaWidth;
    Self.Height := Screen.WorkAreaHeight;


    btnClose := TcxButton.Create(Self);
    btnClose.Parent := Self;
    btnClose.Caption := 'X';
    btnClose.Font.Size := 8;
    btnClose.Width := 20;
    btnClose.Height := 20;
    btnClose.Top := 5;
    btnClose.Left := Self.Width - btnClose.Width - 10;
    btnClose.Anchors := [akTop, akRight];
    btnClose.OnClick := btnCloseClick;


    cxGrid1.Align := alClient;


    ZQuery1.Close;
    ZQuery1.SQL.Text := 'SELECT id_judete, denumire, simb_auto FROM dbo.JUDETE';
    ZQuery1.Connection := frmData.dbContabilitate;
    DataSource1.DataSet := ZQuery1;
    ZQuery1.Open;

    if ZQuery1.IsEmpty then
    begin
      ShowMessage('Interogarea a returnat 0 înregistrări!');
      Exit;
    end;

    cxGrid1DBTableView1.DataController.DataSource := DataSource1;
    cxGrid1DBTableView1.DataController.Refresh;
    cxGrid1DBTableView1.ClearItems;
    cxGrid1DBTableView1.DataController.CreateAllItems;
  except
    on E: Exception do
      ShowMessage('Eroare la încărcarea datelor: ' + E.Message);
  end;
end;


procedure TfrmIntretinereJudete.btnCloseClick(Sender: TObject);
begin
  Self.Close;
end;

procedure ShowIntretinereJudete;
var
  frm: TfrmIntretinereJudete;
begin
  frm := TfrmIntretinereJudete.Create(nil);
  try
    frm.ShowModal;
  finally
    frm.Free;
  end;
end;



end.

