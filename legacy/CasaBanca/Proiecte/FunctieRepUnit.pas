unit FunctieRepUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, HeadPanel, StdCtrls, dxExEdtr, DB, dxmdaset,
  dxCntner, dxTL, dxDBCtrl, dxDBGrid, dxDBTL, dxDBTLCl, dxGrClms, Menus,
  cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmFunctieRep = class(TForm)
    HeadPanel1: THeadPanel;
    GridUtilizatori: TdxDBTreeList;
    TblUtilizatori: TdxMemData;
    DTUtilizatori: TDataSource;
    GridUtilizatoriADRESA: TdxDBGridMaskColumn;
    GridUtilizatoriNUME: TdxDBGridMaskColumn;
    GridUtilizatoriSEL: TdxDBGridCheckColumn;
    GridUtilizatoriID_REPARTITORI: TdxDBTreeListMaskColumn;
    GridUtilizatoriID_PARINTE: TdxDBTreeListMaskColumn;
    BtnCancel: TcxButton;
    BtnOk: TcxButton;
    procedure GridUtilizatoriDblClick(Sender: TObject);
    procedure GridUtilizatoriKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    FIdFunctie: Integer;
    procedure SetIdFunctie(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    procedure WriteToDb;
    property IdFunctie : Integer read FIdFunctie write SetIdFunctie;
  end;

  function ModificaUtilizatori(aIdFunctie: Integer): Boolean;

implementation

{$R *.dfm}

uses DateUnit, ZDataSet, CommonDBVar;

function ModificaUtilizatori(aIdFunctie: Integer): Boolean;
begin
  with TfrmFunctieRep.Create(Application) do
    try
      IdFunctie := aIdFunctie;
      Result := ShowModal = mrOk;
      if Result then WriteToDb;
    finally
       Free;
    end;
end;

{ TfrmAlegeUtilizator }

procedure TfrmFunctieRep.SetIdFunctie(const Value: Integer);
var
  aQry: TZReadOnlyQuery;
begin
  FIdFunctie := Value;
  aQry := GetTmpADOQuery;
  with aQry do
    try
      ParamCheck := False;
      SQL.Add('SELECT CASE WHEN EXISTS(SELECT TOP 1 1 FROM REPARTITORI_ORGANIGRAMA AA WHERE AA.ID_REPARTITORI = A.ID_REPARTITORI AND AA.ID_ORGANIGRAMA = '+IntToStr(Value)+') THEN 1 ELSE 0 END AS SEL, ');
      SQL.Add(' A.NUME, A.ADRESA, A.ID_REPARTITORI, A.ID_PARINTE FROM REPARTITORI A');
      SQL.Add(' WHERE A.GESTINT = 1');
      Open;
      TblUtilizatori.LoadFromDataSet(aQry);
    finally
      Free;
    end;
end;

procedure TfrmFunctieRep.WriteToDb;
begin
  with GetTmpADOQuery do
    try
       Sql.Add('DELETE FROM REPARTITORI_ORGANIGRAMA WHERE ID_ORGANIGRAMA = '+IntToStr(FIdFunctie));
       ExecSQL;
       SQL.Clear;
       SQL.Add('INSERT INTO REPARTITORI_ORGANIGRAMA(ID_ORGANIGRAMA, ID_REPARTITORI, ID_UTILIZATOR)');
       SQL.Add('VALUES('+IntToStr(FIdFunctie)+', :ID_REPARTITORI, '+IntToStr(CommonDBVar.IdLogin)+' )');
       DataSource := DTUtilizatori;
       with TblUtilizatori do begin
         First;
         while not Eof do begin
           if TblUtilizatori.FieldByName('SEL').AsInteger = 1 then ExecSql;
           Next;
         end;
       end;
    finally
       Free;
    end;
end;

procedure TfrmFunctieRep.GridUtilizatoriDblClick(Sender: TObject);
begin
  GridUtilizatori.BeginUpdate;
  TblUtilizatori.Edit;
  if TblUtilizatori.FieldByName('SEL').AsInteger = 0 then
     TblUtilizatori.FieldByName('SEL').AsInteger := 1
  else TblUtilizatori.FieldByName('SEL').AsInteger := 0;
  TblUtilizatori.Post;
  GridUtilizatori.EndUpdate;
  GridUtilizatori.Invalidate;
end;

procedure TfrmFunctieRep.GridUtilizatoriKeyUp(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key= VK_SPACE then GridUtilizatoriDblClick(nil);
end;

end.
