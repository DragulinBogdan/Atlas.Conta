unit fmGrupaProiecteUnit;

interface

uses Forms, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, Menus,
  StdCtrls, cxButtons, Classes, Controls, ExtCtrls, cxControls, cxStyles,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, DB, cxDBData,
  cxSplitter, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  DegradePanel, cxContainer, cxTextEdit, cxMaskEdit, ZAbstractRODataset,
  ZDataset, ZAbstractDataset, cxTL, cxTLdxBarBuiltInMenu,
  cxInplaceContainer, cxTLData, cxDBTL, cxCheckBox, dxScrollbarAnnotations,
  cxNavigator, dxDateRanges;

type
  TfmGrupaProiecte = class(TForm)
    BtnOk: TcxButton;
    pnContent: TPanel;
    pnBottom: TPanel;
    spliterV: TcxSplitter;
    pnGrupaProiecte: TPanel;
    viewGrupa: TcxGridDBTableView;
    nivelGrupa: TcxGridLevel;
    gridGrupa: TcxGrid;
    qryListaProiecteGrupa: TZQuery;
    qryGrupe: TZQuery;
    dtGrupe: TDataSource;
    dtListaProiecteGrupa: TDataSource;
    viewGrupaid_oi_grupe: TcxGridDBColumn;
    viewGrupadenumire: TcxGridDBColumn;
    treeProiecteGrup: TcxDBTreeList;
    treeProiecteGrupid_oi_proiecte: TcxDBTreeListColumn;
    treeProiecteGrupid_parinte: TcxDBTreeListColumn;
    treeProiecteGrupsel: TcxDBTreeListColumn;
    treeProiecteGrupdenumire: TcxDBTreeListColumn;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure qryListaProiecteGrupaAfterOpen(DataSet: TDataSet);
  private
    { Private declarations }
    procedure DoSetProiectToGroup(Sender: TField);
  public
  end;


implementation

uses
  ZeosDBUtile, CommonDBVar, dateUnit, Math;

{$R *.dfm}

procedure TfmGrupaProiecte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfmGrupaProiecte.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfmGrupaProiecte.FormCreate(Sender: TObject);
begin
  OpenDataSets(Self);
end;

procedure TfmGrupaProiecte.qryListaProiecteGrupaAfterOpen(
  DataSet: TDataSet);
begin
  qryListaProiecteGrupa.FieldByName('Sel').ReadOnly := False;
  qryListaProiecteGrupa.FieldByName('Sel').OnChange := DoSetProiectToGroup;
end;

procedure TfmGrupaProiecte.DoSetProiectToGroup(Sender: TField);
begin
  DBExecSqlFmt('exec [spSetProiectToGroup] %d, %d, %s, %s, %s', [IdLogin, IdUtilizator, ValueToStr(QryGrupe['id_oi_grupe']), ValueToStr(qryListaProiecteGrupa['id_oi_proiecte']), ValueToStr(Sender.AsBoolean)]);
end;

end.
