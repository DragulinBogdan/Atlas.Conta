unit NoteEronateUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, StdCtrls, dxDBGrid, dxDBTLCl, dxGrClms, dxTL,
  dxDBCtrl, dxCntner, Db, ZDataSet, dxExEdtr, DegradePanel, Menus,
  cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels, cxControls, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, cxNavigator, dxDateRanges, dxScrollbarAnnotations,
  cxDBData, cxImageComboBox, cxMaskEdit, cxCalendar, cxCurrencyEdit, cxClasses,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGridLevel,
  cxGridCustomView, cxGrid;

type
  TfrmNoteEronate = class(TForm)
    QryNoteErr: TZReadOnlyQuery;
    DTNoteEronate: TDataSource;
    pnTop: TDegradePanel;
    BtnOk: TcxButton;
    viewNoteEronate: TcxGridDBTableView;
    nivelNoteEronate: TcxGridLevel;
    gridNoteEronate: TcxGrid;
    viewNoteEronateTIP_EROARE: TcxGridDBColumn;
    viewNoteEronateCOD: TcxGridDBColumn;
    viewNoteEronateJURNAL: TcxGridDBColumn;
    viewNoteEronateNRDOC: TcxGridDBColumn;
    viewNoteEronateDATA: TcxGridDBColumn;
    viewNoteEronateECL: TcxGridDBColumn;
    viewNoteEronateCONTD: TcxGridDBColumn;
    viewNoteEronateREPARTITOR_DEBIT: TcxGridDBColumn;
    viewNoteEronateVALOARE: TcxGridDBColumn;
    viewNoteEronateCONTC: TcxGridDBColumn;
    viewNoteEronateREPARTITOR_CREDIT: TcxGridDBColumn;
    viewNoteEronateEXPLICATIE: TcxGridDBColumn;
    viewNoteEronateMODUL: TcxGridDBColumn;
    viewNoteEronateC_O: TcxGridDBColumn;
    viewNoteEronateID_PARINTE: TcxGridDBColumn;
    viewNoteEronateDATA_OPERARE: TcxGridDBColumn;
    viewNoteEronateCOD_ECONOMIC: TcxGridDBColumn;
    viewNoteEronateCOD_FUNCTIONAL: TcxGridDBColumn;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile;

procedure TfrmNoteEronate.FormCreate(Sender: TObject);
begin
  FillImageCombo(viewNoteEronateJURNAL.Properties, 'select jurnal, denumire from cjurnale order by denumire', 0, 1);
  FillImageCombo(viewNoteEronateC_O.Properties, 'select id_utilizatori, numeintreg from utilizatori order by numeintreg', 0, 1);
  OpenDataSets(Self);
end;

end.
