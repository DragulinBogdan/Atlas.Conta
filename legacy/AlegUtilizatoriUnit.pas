unit AlegUtilizatoriUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, StdCtrls, dxExEdtr, DB, dxmdaset,
  dxCntner, dxTL, dxDBCtrl, dxDBGrid, dxDBTLCl, dxGrClms,
  DegradePanel, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics, unitMemTableEx,
  cxLookAndFeels, cxControls, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxDBData, cxCheckBox,
  cxMaskEdit, cxClasses, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxGridCustomView, cxGrid, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmAlegeUtilizator = class(TForm)
    DTUtilizatori: TDataSource;
    BtnCancel: TcxButton;
    BtnOk: TcxButton;
    viewUtilizatori: TcxGridDBTableView;
    nivelUtilizatori: TcxGridLevel;
    gridUtilizatori: TcxGrid;
    viewUtilizatoriNUME: TcxGridDBColumn;
    viewUtilizatoriNUMEINTREG: TcxGridDBColumn;
    procedure BtnOkClick(Sender: TObject);
    procedure viewUtilizatoriSelectionChanged(Sender: TcxCustomGridTableView);
  private
    TblUtilizatori: TAtsMemData;
    FIdFunctie: Variant;
    procedure SetIdFunctie(const Value: Variant);
    { Private declarations }
  public
    procedure WriteRow(ARowIndex: Integer; ARowInfo: TcxRowInfo);
    property IdFunctie : Variant read FIdFunctie write SetIdFunctie;
  public
    { Public declarations }
    constructor Create(AOwner: TComponent); override;
  end;

  function ModificaUtilizatori(aIdFunctie: Variant): Boolean;

implementation

{$R *.dfm}

uses
  ZeosDBUtile;

function ModificaUtilizatori(aIdFunctie: Variant): Boolean;
var
  lUserSelFrm : TfrmAlegeUtilizator;
begin
  lUserSelFrm := TfrmAlegeUtilizator.Create(Application);
  try
    lUserSelFrm.IdFunctie := aIdFunctie;
    Result := lUserSelFrm.ShowModal = mrOk;
  finally
    lUserSelFrm.Free;
  end;
end;

{ TfrmAlegeUtilizator }

procedure TfrmAlegeUtilizator.SetIdFunctie(const Value: Variant);
begin
  FIdFunctie := Value;
  DBCopyDataSetFmt(TblUtilizatori, 'SELECT ID_UTILIZATORI, NUME, NUMEINTREG FROM UTILIZATORI', [ValueToStr(FIdFunctie)]);
end;

procedure TfrmAlegeUtilizator.viewUtilizatoriSelectionChanged(
  Sender: TcxCustomGridTableView);
begin
//
end;

procedure TfrmAlegeUtilizator.WriteRow(ARowIndex: Integer;
  ARowInfo: TcxRowInfo);
begin
  DBExecSQLFmt('exec [spUtilizatoriSetFunctiune] %s, %s',
     [
      ValueToStr(FIdFunctie),
      ValueToStr(viewUtilizatori.DataController.GetRecordId(ARowInfo.RecordIndex))
     ]);
end;

procedure TfrmAlegeUtilizator.BtnOkClick(Sender: TObject);
begin
  DBStartTransaction;
  try
    DBExecSQLFmt('UPDATE UTILIZATORI SET ID_FUNCTIUNI = NULL WHERE ID_FUNCTIUNI = %s', [ValueToStr(FIdFunctie)]);
    viewUtilizatori.DataController.ForEachRow(True, WriteRow);
    DBCommit;
    ModalResult := mrOk;
  except
    on E: Exception do begin
      DBRollBack();
      raise;
    end;
  end;
end;

constructor TfrmAlegeUtilizator.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  TblUtilizatori := TAtsMemData.Create(Self);
  DTUtilizatori.DataSet := TblUtilizatori;
end;

end.
