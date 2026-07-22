unit ValidareCBUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxStyles, cxCustomData, cxGraphics, cxFilter, cxData,
  cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, Buttons, ExtCtrls, cxPC, ZDataSet,
  cxGridCardView, cxGridDBCardView, dxSkinsDefaultPainters;

type
  TfrmCBValidare = class(TForm)
    PageControl: TcxTabControl;
    pnClient: TPanel;
    pnBottom: TPanel;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1Level1: TcxGridLevel;
    cxGrid1: TcxGrid;
    DTVisualizare: TDataSource;
    QryVizualizare: TZQuery;
    pnExplicatie: TPanel;
    cxGrid1DBTableView1COD_CB: TcxGridDBColumn;
    cxGrid1DBTableView1DENUMIRE: TcxGridDBColumn;
    cxGrid1DBTableView1CRSP_LEI: TcxGridDBColumn;
    cxGrid1DBTableView1DENV: TcxGridDBColumn;
    cxGrid1DBTableView1C_O: TcxGridDBColumn;
    cxGrid1DBTableView1SOLDINI_D: TcxGridDBColumn;
    cxGrid1DBTableView1SOLDINI_C: TcxGridDBColumn;
    cxGrid1DBTableView1DATA_SOLD: TcxGridDBColumn;
    cxGrid1DBTableView1CASIER: TcxGridDBColumn;
    cxGrid1DBTableView1DEFALCATOR: TcxGridDBColumn;
    cxGrid1DBTableView1ADMIN: TcxGridDBColumn;
    cxGrid1DBTableView1IS_BANCA: TcxGridDBColumn;
    cxGrid1DBTableView1IS_AVANS: TcxGridDBColumn;
    cxGrid1DBTableView1IS_TEMPOR: TcxGridDBColumn;
    cxGrid1DBTableView1ID_REPARTITORI: TcxGridDBColumn;
    cxGrid1Level2: TcxGridLevel;
    cxGrid1DBTableView2: TcxGridDBTableView;
    DataSource1: TDataSource;
    QryLevel2: TZQuery;
    cxGrid1DBTableView2COD_CB: TcxGridDBColumn;
    cxGrid1DBTableView2COD: TcxGridDBColumn;
    cxGrid1DBTableView2CODGEST: TcxGridDBColumn;
    cxGrid1DBTableView2DATA: TcxGridDBColumn;
    cxGrid1DBTableView2TIPDOC: TcxGridDBColumn;
    cxGrid1DBTableView2NRDOC: TcxGridDBColumn;
    cxGrid1DBTableView2POZ: TcxGridDBColumn;
    cxGrid1DBTableView2EXPLICATIE: TcxGridDBColumn;
    cxGrid1DBTableView2INCASARI: TcxGridDBColumn;
    cxGrid1DBTableView2PLATI: TcxGridDBColumn;
    cxGrid1DBTableView2SOLD: TcxGridDBColumn;
    cxGrid1DBTableView2CONT_CSP: TcxGridDBColumn;
    cxGrid1DBTableView2VAL_CRSP: TcxGridDBColumn;
    cxGrid1DBTableView2ACHITAT: TcxGridDBColumn;
    cxGrid1DBTableView2DATAEM: TcxGridDBColumn;
    cxGrid1DBTableView2C_O: TcxGridDBColumn;
    cxGrid1DBTableView2NR_LIST: TcxGridDBColumn;
    cxGrid1DBTableView2MEXPLIC: TcxGridDBColumn;
    cxGrid1DBTableView2CURS_SCHIM: TcxGridDBColumn;
    cxGrid1DBTableView2SOLD_INITIAL: TcxGridDBColumn;
    cxGrid1DBTableView2COD_ARHIVA: TcxGridDBColumn;
    cxGrid1DBTableView2ECL: TcxGridDBColumn;
    cxGrid1DBTableView2VALIDATA: TcxGridDBColumn;
    cxGrid1DBTableView2TRANSFER: TcxGridDBColumn;
    cxGrid1DBTableView2COD_CBT: TcxGridDBColumn;
    cxGrid1DBTableView2COD_TRANSFER: TcxGridDBColumn;
    cxGrid1DBTableView2NR_DECONT: TcxGridDBColumn;
    cxGrid1DBTableView2DATA_DECONT: TcxGridDBColumn;
    cxGrid1DBTableView2PARENT_COD: TcxGridDBColumn;
    cxGrid1DBTableView2V_O: TcxGridDBColumn;
    cxGrid1DBTableView2VALIDATION_HASH: TcxGridDBColumn;
    cxGrid1Level3: TcxGridLevel;
    DataSource2: TDataSource;
    ADOQuery1: TZQuery;
    cxGrid1DBCardView1: TcxGridDBCardView;
    cxGrid1DBCardView1COD_CB: TcxGridDBCardViewRow;
    cxGrid1DBCardView1DENUMIRE: TcxGridDBCardViewRow;
    cxGrid1DBCardView1CRSP_LEI: TcxGridDBCardViewRow;
    cxGrid1DBCardView1DENV: TcxGridDBCardViewRow;
    cxGrid1DBCardView1C_O: TcxGridDBCardViewRow;
    cxGrid1DBCardView1SOLDINI_D: TcxGridDBCardViewRow;
    cxGrid1DBCardView1SOLDINI_C: TcxGridDBCardViewRow;
    cxGrid1DBCardView1DATA_SOLD: TcxGridDBCardViewRow;
    cxGrid1DBCardView1CASIER: TcxGridDBCardViewRow;
    cxGrid1DBCardView1DEFALCATOR: TcxGridDBCardViewRow;
    cxGrid1DBCardView1ADMIN: TcxGridDBCardViewRow;
    cxGrid1DBCardView1IS_BANCA: TcxGridDBCardViewRow;
    cxGrid1DBCardView1IS_AVANS: TcxGridDBCardViewRow;
    cxGrid1DBCardView1IS_TEMPOR: TcxGridDBCardViewRow;
    cxGrid1DBCardView1ID_REPARTITORI: TcxGridDBCardViewRow;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmCBValidare: TfrmCBValidare;

implementation

uses DateUnit;

{$R *.dfm}

end.
