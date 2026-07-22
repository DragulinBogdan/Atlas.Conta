unit DefalcareDecontareUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, StdCtrls, Buttons, dxmdaset, dxCntner, dxTL, dxDBCtrl,
  dxDBGrid, dxEditor, dxExEdtr, dxEdLib, DBSumLst, dxDBTLCl, dxGrClms,
  ExtCtrls, AtsDBEvaluator;

type
  TfrmDefalcareDecontare = class(TForm)
    DTDefalcareDecontare: TDataSource;
    BtnCancel: TBitBtn;
    BtnOk: TBitBtn;
    DBDefalcareDecont: TdxMemData;
    GridDefalcare: TdxDBGrid;
    Label2: TLabel;
    Label3: TLabel;
    edInainte: TdxCurrencyEdit;
    edTotal: TdxCurrencyEdit;
    GridDefalcareTIPMAT: TdxDBGridMaskColumn;
    GridDefalcareDENMAT: TdxDBGridMaskColumn;
    GridDefalcareCANTITATE: TdxDBGridCurrencyColumn;
    GridDefalcarePRET_UNITAR: TdxDBGridCurrencyColumn;
    GridDefalcarePRET_LIVRARE_TVA: TdxDBGridCurrencyColumn;
    GridDefalcareCURENT: TdxDBGridCurrencyColumn;
    Panel1: TPanel;
    Label1: TLabel;
    LbInfoDocum: TLabel;
    GridDefalcareTOTAL: TdxDBGridCurrencyColumn;
    procedure BtnOkClick(Sender: TObject);
    procedure ATSEvaluatorChangeField(SrcDataSet: TDataSet;
      SrcField: TField; DestDataSet: TDataSet; DestField: TField;
      const FieldName: String; Value: Variant; Modified: Boolean);
  private
    { Private declarations }
    FATSEvaluator: TATSEvaluator;
    FIsNotWrited: Boolean;
    FIdDecont,
    FIdCasa,
    FIdDocum: Integer;
    procedure ValidareSuma(Sender: TField);
    procedure DefalcareImplicita;
    procedure WriteDefalcare;
    procedure ActivateEvaluator;
    function  GetSumaFromDecontari(AId: Integer): Currency;
  public
    { Public declarations }
    constructor Create(AOwner: TComponent); override;
  end;

  { Intoarcem True daca trebuie sa facem refresh }
  function EditDefalcare(IdCasa: Integer; IdDocum: Integer; Suma: Currency): Boolean;

implementation

{$R *.DFM}

uses DateUnit, Variants, CommonDBVar;

  function EditDefalcare(IdCasa: Integer; IdDocum: Integer; Suma: Currency): Boolean;
  var FakeQry: TZReadOnlyQuery;
   begin
     with TfrmDefalcareDecontare.Create(Application) do
       try
          FIdCasa  := IdCasa;
          FIdDocum := IdDocum;
          edInainte.Value := Suma;
          { Citim informatiile pentru ecranul de culegere }
          FakeQry := GetTmpADOQuery;
          with FakeQry do
            try
               ParamCheck := False;
               Sql.Add('EXEC SP_DECONTARI_GET_DEFALCARE '+IntToStr(IdCasa)+', '+IntToStr(IdDocum));
               Open;
               if not IsEmpty then FIdDecont := FieldByName('ID_GEST_DECONTARI').AsInteger
               else FIdDecont := -1;
               if FIdDecont > -1 then edInainte.Value := GetSumaFromDecontari(FIdDecont);
               FIsNotWrited := FIdDecont = -1;
               DBDefalcareDecont.LoadFromDataSet(FakeQry);
               DBDefalcareDecont.FindField('CURENT').OnValidate := ValidareSuma;
               ActivateEvaluator;
               Close;
               Sql.Clear;
               Sql.Add('exec spGestDecontariDefalcare '+IntToStr(IdDocum));
(*
               Sql.Add('SELECT RTRIM(LTRIM(COD_DOCUM))+'' nr. ''+RTRIM(LTRIM(NR_DOCUM))+'' din ''+CONVERT(VARCHAR(10), DATA_DOCUM, 103)+'' : ''+');
               Sql.Add('       CASE WHEN B.GESTINT = 1 THEN RTRIM(LTRIM(B.NUME)) ELSE RTRIM(LTRIM(C.NUME)) END AS DESCRIERE,');
               Sql.Add('       CASE WHEN ISNULL(B.GESTINT,0) = 0 THEN RTRIM(LTRIM(B.NUME)) ELSE RTRIM(LTRIM(C.NUME)) END AS REPARTITOR');
               Sql.Add('  FROM GEST_DOCUM A JOIN REPARTITORI B ON (A.ID_PREDATOR = B.ID_REPARTITORI)');
               Sql.Add('       JOIN REPARTITORI C ON (C.ID_REPARTITORI = A.ID_PRIMITOR)');
               Sql.Add('       JOIN GEST_TIP_DOCUM D ON (D.ID_GEST_TIP_DOCUM = A.ID_GEST_TIP_DOCUM)');
               Sql.Add(' WHERE A.ID_GEST_DOCUM = '+IntToStr(IdDocum));
*)
               Open;
               LbInfoDocum.Caption := Fields[0].AsString;
               { Aici putem sa facem defalcarea implicita }
               if (FIsNotWrited) and (edTotal.Value = 0) then DefalcareImplicita;
            finally
               FakeQry.Free;
            end;
          Result := ShowModal = mrOk;
          if Result then begin
             WriteDefalcare;
             Result := Result and FIsNotWrited;
          end;
       finally
          Free;
       end;
   end;


procedure TfrmDefalcareDecontare.DefalcareImplicita;
var lTotal, lCurent: Currency;
begin
  with DBDefalcareDecont do begin
    lTotal := edInainte.Value;
    First;
    while (not Eof) and (lTotal > 0) do begin
      lCurent := FieldByName('PRET_LIVRARE_TVA').AsCurrency - FieldByName('TOTAL').AsCurrency;
      if lCurent > 0 then begin
         Edit;
         if lCurent < lTotal then
            FieldByName('CURENT').AsCurrency := lCurent + FieldByName('ANTERIOR').AsCurrency
         else FieldByName('CURENT').AsCurrency := lTotal;
         FieldByName('TOTAL').AsCurrency  := FieldByName('TOTAL').AsCurrency + lCurent;
         FieldByName('ANTERIOR').AsCurrency  := FieldByName('ANTERIOR').AsCurrency + lCurent;
         Post;
      end;
      lTotal := lTotal - lCurent;
      Next;
    end;
    First;
  end;
end;

function TfrmDefalcareDecontare.GetSumaFromDecontari(
  AId: Integer): Currency;
begin
  with GetTmpADOQuery do
    try
       Sql.Add('SELECT SUMA FROM GEST_DECONTARI WHERE ID_GEST_DECONTARI = '+IntToStr(AId));
       Open;
       Result := Fields[0].AsCurrency;
    finally
       Free;
    end;
end;

procedure TfrmDefalcareDecontare.ValidareSuma(Sender: TField);
var lExist, lCurent: Currency;
begin
  with DBDefalcareDecont do begin
    lExist := FieldByName('PRET_LIVRARE_TVA').AsCurrency;
    lCurent := Sender.AsCurrency + FieldByName('TOTAL').AsCurrency - FieldByName('ANTERIOR').AsCurrency;
    if lCurent > lExist then
       raise EContaHandledError.Create('Ati depasit valoarea pe pozitia curenta '+FormatFloat(',0.00;-,0.00', lCurent)+ ' > '+FormatFloat(',0.00;-,0.00', lExist));
  end;
end;

procedure TfrmDefalcareDecontare.WriteDefalcare;
begin
  { Trebuie intai adaugata pozitie in GEST_DECONTARI }
  with GetTmpADOQuery do
    try
       if FIsNotWrited then begin
          Sql.Add('SELECT * FROM GEST_DECONTARI WHERE ID_GEST_DECONTARI = -1');
          Open;
          Append;
          FieldByName('ID_GEST_DOCUM').AsInteger := FIdDocum;
          FieldByName('ID_BREGISTRU').AsInteger  := FIdCasa;
          FieldByName('SUMA').AsCurrency         := edTotal.Value;
          Post;
          FIdDecont := FieldByName('ID_GEST_DECONTARI').AsInteger;
          Close;
       end
       else begin
          Sql.Add('DELETE FROM GEST_DEFALCARE_DECONTARI WHERE ID_GEST_DECONTARI = '+IntToStr(FIdDecont));
          ExecSql;
       end;
       Sql.Clear;
       Sql.Add('INSERT INTO GEST_DEFALCARE_DECONTARI (ID_GEST_DECONTARI, ID_GEST_ITEMSI, SUMA)');
       Sql.Add('VALUES (:ID_GEST_DECO, :ID_GEST_ITEMSI, :CURENT)');
       Params.ParamByName('ID_GEST_DECO').Value := FIdDecont;
       DataSource := DTDefalcareDecontare;
       DBDefalcareDecont.First;
       while not DBDefalcareDecont.Eof do begin
         ExecSQL;
         DBDefalcareDecont.Next;
       end;
    finally
       Free;
    end;
end;

procedure TfrmDefalcareDecontare.BtnOkClick(Sender: TObject);
begin
  edTotal.Value := FATSEvaluator.Formule[0].Calculate;
  if edInainte.Value <> edTotal.Value then
     if MessageDlg('Suma defalcarilor este diferita de suma specificata anterior'#1#310'Doriti folosirea sumei defalcate?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  ModalResult := mrOk;
end;

constructor TfrmDefalcareDecontare.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FATSEvaluator := TATSEvaluator.Create(Self);
end;

procedure TfrmDefalcareDecontare.ActivateEvaluator;
begin
   if FATSEvaluator.FormuleCount = 0 then begin
     FATSEvaluator.Active := False;
     FATSEvaluator.AddDataSet(DBDefalcareDecont);
     FATSEvaluator.AddFormula(DBDefalcareDecont, 'null', 'sum(CURENT)');
     FATSEvaluator.Active := True;
   end;
end;

procedure TfrmDefalcareDecontare.ATSEvaluatorChangeField(
  SrcDataSet: TDataSet; SrcField: TField; DestDataSet: TDataSet;
  DestField: TField; const FieldName: String; Value: Variant;
  Modified: Boolean);
begin
    if Value > edInainte.Value then begin
     DBDefalcareDecont.Cancel;
     raise EContaHandledError.Create('Depasiti suma totala pentru decontarea curenta '+FormatFloat(',0.00;-,0.00', Value)+ ' > '+FormatFloat(',0.00;-,0.00', edInainte.Value));
  end;
  edTotal.Value := Value;
//  ShowMessage(VarToStr(Value));
end;

end.
