unit IstoricNota;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, DateUnit, DB,  ZDataSet, StdCtrls, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxCalendar, cxTextEdit,
  cxCurrencyEdit, cxCheckBox, cxTLdxBarBuiltInMenu, cxInplaceContainer,
  cxDBTL, cxTLData, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfrmIstoricNota = class(TForm)
    qryNota: TZQuery;
    DataSource1: TDataSource;
    Button1: TButton;
    TreeIstoricNota: TcxDBTreeList;
    TreeIstoricNotaNR: TcxDBTreeListColumn;
    TreeIstoricNotaCOD: TcxDBTreeListColumn;
    TreeIstoricNotaPOZ: TcxDBTreeListColumn;
    TreeIstoricNotaJURNAL: TcxDBTreeListColumn;
    TreeIstoricNotaNRDOC: TcxDBTreeListColumn;
    TreeIstoricNotaDATA: TcxDBTreeListColumn;
    TreeIstoricNotaEXPLICATIE: TcxDBTreeListColumn;
    TreeIstoricNotaVALOARE: TcxDBTreeListColumn;
    TreeIstoricNotaCONT_DEBT: TcxDBTreeListColumn;
    TreeIstoricNotaREPARTITOR_DEBIT: TcxDBTreeListColumn;
    TreeIstoricNotaAC: TcxDBTreeListColumn;
    TreeIstoricNotaECL: TcxDBTreeListColumn;
    TreeIstoricNotaC_O: TcxDBTreeListColumn;
    TreeIstoricNotaBUGET: TcxDBTreeListColumn;
    TreeIstoricNotaCOMPUSA: TcxDBTreeListColumn;
    TreeIstoricNotaCONTD: TcxDBTreeListColumn;
    TreeIstoricNotaCONTC: TcxDBTreeListColumn;
    TreeIstoricNotaCONT_CRED: TcxDBTreeListColumn;
    TreeIstoricNotaID_INITIAL: TcxDBTreeListColumn;
    TreeIstoricNotaID_PARINTE: TcxDBTreeListColumn;
    TreeIstoricNotaDATA_OPERARE: TcxDBTreeListColumn;
    TreeIstoricNotaCODREP_OLD: TcxDBTreeListColumn;
    TreeIstoricNotaSTARE: TcxDBTreeListColumn;
    TreeIstoricNotaID_ANGAJAMENTE_DEFALCARE: TcxDBTreeListColumn;
    TreeIstoricNotaREPARTITOR_CREDIT: TcxDBTreeListColumn;
    TreeIstoricNotaMODUL: TcxDBTreeListColumn;
    TreeIstoricNotaCOD_ECONOMIC: TcxDBTreeListColumn;
    TreeIstoricNotaCOD_FUNCTIONAL: TcxDBTreeListColumn;
    TreeIstoricNotaNR_OP: TcxDBTreeListColumn;
    TreeIstoricNotaID_ORDIN_PLATA: TcxDBTreeListColumn;
    TreeIstoricNotaCODGEST_OLD: TcxDBTreeListColumn;
    TreeIstoricNotaOLD_CONT_C: TcxDBTreeListColumn;
    TreeIstoricNotaOLD_CONT_D: TcxDBTreeListColumn;
    TreeIstoricNotaTRANSFERAT_C: TcxDBTreeListColumn;
    TreeIstoricNotaTRANSFERAT_D: TcxDBTreeListColumn;
    TreeIstoricNotaID_UNIC_MODUL: TcxDBTreeListColumn;
    TreeIstoricNotaID_DOCUMENT_MODUL: TcxDBTreeListColumn;
    TreeIstoricNotaID_MATERIAL: TcxDBTreeListColumn;
    TreeIstoricNotaTIP_DOCUMENT: TcxDBTreeListColumn;
    TreeIstoricNotaNUMAR_DOCUMENT: TcxDBTreeListColumn;
    TreeIstoricNotaDATA_DOCUMENT: TcxDBTreeListColumn;
    TreeIstoricNotaNUME_MATERIAL: TcxDBTreeListColumn;
    TreeIstoricNotaANALITIC_MATERIAL: TcxDBTreeListColumn;
    TreeIstoricNotaistoric_nr: TcxDBTreeListColumn;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure Button1Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.dfm}

uses
  CommonDBVar;

procedure TfrmIstoricNota.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmIstoricNota.FormCreate(Sender: TObject);
begin
  StorageReadCxTree(TreeIstoricNota);
end;

procedure TfrmIstoricNota.FormDestroy(Sender: TObject);
begin
  StorageWriteCxTree(TreeIstoricNota);
end;

procedure TfrmIstoricNota.Button1Click(Sender: TObject);
begin
  Close;
end;

end.
