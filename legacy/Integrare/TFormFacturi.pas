unit TFormFacturi;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms, Dialogs,
  DB, ZAbstractRODataset, ZAbstractDataset, ZDataset, Grids, DBGrids, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, cxNavigator, dxDateRanges, dxScrollbarAnnotations,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, ZeosDBUtile, StdCtrls,
  IdHTTP, IdMultipartFormData, IdSSLOpenSSL, Vcl.Menus, cxButtons;

type
  TForm2 = class(TForm)
    ZQuery1: TZQuery;
    DataSource1: TDataSource;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1Level1: TcxGridLevel;
    cxGrid1: TcxGrid;
    btnValidareXMLClick: TButton;
    OpenDialogXML: TOpenDialog;
    btnTransformaInPDF: TcxButton;
    SaveDialogPDF: TSaveDialog;
    SaveDialog1: TSaveDialog;
    btnGenereazaXML: TButton;
    procedure FormCreate(Sender: TObject);
    procedure btnValidareXMLClickClick(Sender: TObject);
    procedure btnTransformaInPDFClick(Sender: TObject);
   // procedure FormClick(Sender: TObject);
  private
    procedure TrimiteXMLLaANAF(const FilePath, Standard: string);
  public
  end;

var
  Form2: TForm2;

implementation

uses DateUnit,System.IOUtils;

{$R *.dfm}



procedure TForm2.FormCreate(Sender: TObject);
begin
  WindowState := wsMaximized;

  ZQuery1.Connection := frmData.dbContabilitate;

  if ZQuery1.Active then
    ZQuery1.Close;

  ZQuery1.SQL.Text :=
    'SELECT d.id_gest_docum, d.nr_docum AS [Număr Factură], ' +
    'd.data_docum AS [Dată], f.NUME AS [Client], d.totaldoc AS [Total RON] ' +
    'FROM gest_docum d ' +
    'JOIN gest_defa_docum c ON c.id_gest_defa_docum = d.id_gest_defa_docum ' +
    'JOIN gest_tip_docum t ON t.id_gest_tip_docum = c.id_gest_tip_docum ' +
    'JOIN repartitori f ON f.id_repartitori = d.id_primitor ' +
    'WHERE d.stare = 1 AND t.cod_docum = ''FCT'' ' +
    'ORDER BY d.data_docum DESC';

  ZQuery1.Open;

  DataSource1.DataSet := ZQuery1;
  cxGrid1DBTableView1.DataController.DataSource := DataSource1;

  if cxGrid1DBTableView1.ColumnCount = 0 then
    cxGrid1DBTableView1.DataController.CreateAllItems(True);
end;



procedure TForm2.btnValidareXMLClickClick(Sender: TObject);
var
  standard: string;
  btnResult: TModalResult;
begin
  btnResult := MessageDlg(
    'Alege standardul pentru validare:' + sLineBreak +
    'Alegeti Yes pentru FACT1' + sLineBreak +
    'Alegeti No pentru FCN',
    mtConfirmation,
    [mbYes, mbNo, mbCancel],
    0
  );

  case btnResult of
    mrYes: standard := 'FACT1';
    mrNo:  standard := 'FCN';
  else
    Exit; // cancel
  end;

  if OpenDialogXML.Execute then
    TrimiteXMLLaANAF(OpenDialogXML.FileName, standard);
end;


//validare xml
procedure TForm2.TrimiteXMLLaANAF(const FilePath, Standard: string);
var
  http: TIdHTTP;
  ssl: TIdSSLIOHandlerSocketOpenSSL;
  fs: TFileStream;
  response: string;
begin
  http := TIdHTTP.Create(nil);
  ssl := TIdSSLIOHandlerSocketOpenSSL.Create(nil);
  fs := TFileStream.Create(FilePath, fmOpenRead or fmShareDenyWrite);
  try
    http.IOHandler := ssl;
    http.Request.ContentType := 'text/plain';
    http.Request.CharSet := 'utf-8';

    response := http.Post(
      'https://webservicesp.anaf.ro/prod/FCTEL/rest/validare/' + Standard,
      fs
    );

    ShowMessage('Răspuns ANAF:' + sLineBreak + response);
  except
    on E: Exception do
      ShowMessage('Eroare la validare: ' + E.Message);
  end;

  fs.Free;
  ssl.Free;
  http.Free;
end;
procedure TForm2.btnTransformaInPDFClick(Sender: TObject);
var
  standard: string;
  http: TIdHTTP;
  ssl: TIdSSLIOHandlerSocketOpenSSL;
  xmlStream, pdfStream: TMemoryStream;
  filePath: string;
  btn: Integer;
begin
  btn := MessageDlg(
    'Alege standardul pentru transformare în PDF:' + sLineBreak +
    'Yes = FACT1' + sLineBreak +
    'No = FCN',
    mtConfirmation, [mbYes, mbNo, mbCancel], 0);

  case btn of
    mrYes: standard := 'FACT1';
    mrNo:  standard := 'FCN';
  else
    Exit;
  end;
  ShowMessage('Alege calea catre fisierul xml');
  if not OpenDialogXML.Execute then Exit;
  filePath := OpenDialogXML.FileName;
   ShowMessage('Alege calea pentru salvarea fisierului pdf');
  if not SaveDialogPDF.Execute then Exit;

  http := TIdHTTP.Create(nil);
  ssl := TIdSSLIOHandlerSocketOpenSSL.Create(nil);
  xmlStream := TMemoryStream.Create;
  pdfStream := TMemoryStream.Create;
  try
    xmlStream.LoadFromFile(filePath);

    http.IOHandler := ssl;


    http.Request.ContentType := 'text/plain';
    http.Request.CharSet := 'utf-8';
    http.Request.Accept := 'application/pdf';
    http.Request.UserAgent := 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)';

    http.Post(
      Format('https://webservicesp.anaf.ro/prod/FCTEL/rest/transformare/%s/DA', [standard]),
      xmlStream,
      pdfStream
    );

    pdfStream.SaveToFile(SaveDialogPDF.FileName);
    ShowMessage('PDF salvat cu succes la: ' + SaveDialogPDF.FileName);
  except
    on E: Exception do
      ShowMessage('Eroare la transformare: ' + E.Message);
  end;

  xmlStream.Free;
  pdfStream.Free;
  ssl.Free;
  http.Free;
end;





end.

