unit AsocUtilizUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, HeadPanel, StdCtrls, dxExEdtr, DB, dxmdaset,
  dxCntner, dxTL, dxDBCtrl, dxDBGrid, ComCtrls, dxDBTLCl, dxGrClms, dxDBTL,
  Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmAsocUtilizatori = class(TForm)
    HeadPanel1: THeadPanel;
    TblUtilizatori: TdxMemData;
    DTUtilizatori: TDataSource;
    PageControl: TPageControl;
    pageUtiliz: TTabSheet;
    GridUtilizatori: TdxDBTreeList;
    GridUtilizatoriSEL: TdxDBGridCheckColumn;
    GridUtilizatoriNUME: TdxDBGridMaskColumn;
    GridUtilizatoriADRESA: TdxDBGridMaskColumn;
    GridUtilizatoriID_REPARTITORI: TdxDBTreeListMaskColumn;
    GridUtilizatoriID_PARINTE: TdxDBTreeListMaskColumn;
    pageGrup: TTabSheet;
    BtnCancel: TcxButton;
    BtnOk: TcxButton;
    procedure GridUtilizatoriDblClick(Sender: TObject);
    procedure GridUtilizatoriKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    FIdFunctie: Integer;
    FCodCasa: Integer;
    FListaUser: String;
    procedure SetIdFunctie(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    procedure WriteToDb;
    property IdFunctie : Integer read FIdFunctie write SetIdFunctie;
    property CodCasa : Integer read FCodCasa write FCodCasa;
    property ListaUser : String read FListaUser write FListaUser;
  end;

  function ModificaUtilizatori(aCodCasa: Integer; aIdFunctie: Integer): String;

implementation

{$R *.dfm}

uses DateUnit, ZDataSet, CommonDBVar;

function ModificaUtilizatori(aCodCasa : Integer; aIdFunctie: Integer): String;
begin
  with TfrmAsocUtilizatori.Create(Application) do
    try
      CodCasa := aCodCasa;
      IdFunctie := aIdFunctie;
      Result := '';
      if (ShowModal = mrOk) then begin
         WriteToDb;
         Result := ListaUser;
      end;
    finally
       Free;
    end;
end;

{ TfrmAlegeUtilizator }

procedure TfrmAsocUtilizatori.SetIdFunctie(const Value: Integer);
var
  aQry: TZReadOnlyQuery;
begin
  FIdFunctie := Value;
  aQry := GetTmpADOQuery;
  with aQry do
    try
      ParamCheck := False;
      SQL.Add('SELECT CASE WHEN EXISTS ( ');
      SQL.Add(' SELECT TOP 1 1 FROM UTILIZATORI_CASIERIE AA WHERE AA.ID_UTILIZATORI = A.ID_UTILIZATORI AND AA.FUNCTIE = '+IntToStr(Value)+' AND COD_CB ='+IntToStr(FCodCasa));
      SQL.Add(') THEN 1 ELSE 0 END AS SEL, ');
      SQL.Add(' A.NUME, A.NUMEINTREG, A.ID_UTILIZATORI FROM UTILIZATORI A');
      Open;
      TblUtilizatori.LoadFromDataSet(aQry);
    finally
      Free;
    end;
end;

procedure TfrmAsocUtilizatori.WriteToDb;
begin
  with GetTmpADOQuery do
    try
       Sql.Add('DELETE FROM UTILIZATORI_CASIERIE WHERE FUNCTIE = '+IntToStr(FIdFunctie)+' AND COD_CB ='+IntToStr(FCodCasa));
       ExecSQL;
       SQL.Clear;
       SQL.Add('INSERT INTO UTILIZATORI_CASIERIE(ID_UTILIZATORI, COD_CB, FUNCTIE)');
       SQL.Add('VALUES( :ID_UTILIZATORI, '+ IntToStr(FCodCasa) + ',' +IntToStr(FIdFunctie)+' )');
       DataSource := DTUtilizatori;
       with TblUtilizatori do begin
         First;
         while not Eof do begin
           if TblUtilizatori.FieldByName('SEL').AsInteger = 1 then begin
              ExecSql;
              if Trim(ListaUser) = '' then ListaUser := TblUtilizatori.FieldByName('ID_UTILIZATORI').AsString
              else ListaUser := ListaUser + ',' + TblUtilizatori.FieldByName('ID_UTILIZATORI').AsString;
           end;
           Next;
         end;
       end;
    finally
       Free;
    end;
end;

procedure TfrmAsocUtilizatori.GridUtilizatoriDblClick(Sender: TObject);
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

procedure TfrmAsocUtilizatori.GridUtilizatoriKeyUp(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key= VK_SPACE then GridUtilizatoriDblClick(nil);
end;

end.
