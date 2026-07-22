unit DefalcareNoteImperecheate;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, StdCtrls, Buttons, dxmdaset,
  DBSumLst,
  ExtCtrls, ATSDBEvaluator, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, cxDBData, cxTextEdit, Menus, cxContainer,
  cxCurrencyEdit, cxButtons, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridCustomView, cxClasses, cxGridLevel, cxGrid,
  cxNavigator, dxDateRanges, dxScrollbarAnnotations;

type
  TfrmDefalcareNote = class(TForm)
    DTDefalcareDecontare: TDataSource;
    DBDefalcareDecont1: TdxMemData;
    Label2: TLabel;
    Label3: TLabel;
    Panel1: TPanel;
    Label1: TLabel;
    LbInfoDocum: TLabel;
    cxGridDefalcare: TcxGrid;
    cxGridDefalcareLevel1: TcxGridLevel;
    GridDefalcare: TcxGridDBTableView;
    GridDefalcareID_CNOTE_IMPERECHERE: TcxGridDBColumn;
    GridDefalcareID_CNOTE_ITEMSI: TcxGridDBColumn;
    GridDefalcareTOTAL: TcxGridDBColumn;
    GridDefalcareANTERIOR: TcxGridDBColumn;
    GridDefalcareCURENT: TcxGridDBColumn;
    GridDefalcareEXPLICATIE: TcxGridDBColumn;
    GridDefalcareNRDOC: TcxGridDBColumn;
    GridDefalcareDATA: TcxGridDBColumn;
    GridDefalcareCONT_DEBT: TcxGridDBColumn;
    GridDefalcareCONT_CRED: TcxGridDBColumn;
    GridDefalcareVALOARE: TcxGridDBColumn;
    BtnCancel: TcxButton;
    BtnOk: TcxButton;
    edInainte: TcxCurrencyEdit;
    edTotal: TcxCurrencyEdit;
    procedure BtnOkClick(Sender: TObject);
    procedure ATSEvaluatorChangeField(SrcDataSet: TDataSet;
      SrcField: TField; DestDataSet: TDataSet; DestField: TField;
      const FieldName: String; Value: Variant; Modified: Boolean);
  private
    ATSEvaluator : TATSEvaluator;
    { Private declarations }
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
  function EditDefalcareNota(IdCasa: Integer; IdDocum: Integer; Suma: Currency): Boolean;

implementation

{$R *.DFM}

uses DateUnit, Variants, CommonDBVar;

  function EditDefalcareNota(IdCasa: Integer; IdDocum: Integer; Suma: Currency): Boolean;
  var FakeQry: TZReadOnlyQuery;
   begin
     with TfrmDefalcareNote.Create(Application) do
       try
          FIdCasa  := IdCasa;
          FIdDocum := IdDocum;
          edInainte.Value := Suma;
          { Citim informatiile pentru ecranul de culegere }
          FakeQry := GetTmpADOQuery;
          with FakeQry do
            try
               ParamCheck := False;
               Sql.Add('EXEC spGetNoteImperecheateDefalcare '+IntToStr(IdCasa)+', '+IntToStr(IdDocum));
               Open;
               if not IsEmpty then FIdDecont := FieldByName('ID_CNOTE_IMPERECHERE').AsInteger
               else FIdDecont := -1;
               if FIdDecont > -1 then edInainte.Value := GetSumaFromDecontari(FIdDecont);
               FIsNotWrited := FIdDecont = -1;
               DBDefalcareDecont1.LoadFromDataSet(FakeQry);
               DBDefalcareDecont1.FindField('CURENT').OnValidate := ValidareSuma;
               ActivateEvaluator;
               Close;
               Sql.Clear;
               Sql.Add('exec spNoteDecontariDefalcare '+IntToStr(IdDocum));
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


procedure TfrmDefalcareNote.DefalcareImplicita;
var lTotal, lCurent: Currency;
begin
  with DBDefalcareDecont1 do begin
    lTotal := edInainte.Value;
    First;
    while (not Eof) and (lTotal > 0) do begin
      lCurent := FieldByName('VALOARE').AsCurrency - FieldByName('TOTAL').AsCurrency;
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

function TfrmDefalcareNote.GetSumaFromDecontari(
  AId: Integer): Currency;
begin
  with GetTmpADOQuery do
    try
       Sql.Add('SELECT SUMA FROM CNOTE_IMPERECHERE WHERE ID_CNOTE_IMPERECHERE = '+IntToStr(AId));
       Open;
       Result := Fields[0].AsCurrency;
    finally
       Free;
    end;
end;

procedure TfrmDefalcareNote.ValidareSuma(Sender: TField);
var lExist, lCurent: Currency;
begin
  with DBDefalcareDecont1 do begin
    lExist := FieldByName('VALOARE').AsCurrency;
    lCurent := Sender.AsCurrency + FieldByName('TOTAL').AsCurrency - FieldByName('ANTERIOR').AsCurrency;
    if lCurent > lExist then
       raise EContaHandledError.Create('Ati depasit valoarea pe pozitia curenta '+FormatFloat(',0.00;-,0.00', lCurent)+ ' > '+FormatFloat(',0.00;-,0.00', lExist));
  end;
end;

procedure TfrmDefalcareNote.WriteDefalcare;
begin
  { Trebuie intai adaugata pozitie in GEST_DECONTARI }
  with GetTmpADOQuery do
    try
       if FIsNotWrited then begin
          Sql.Add('SELECT * FROM CNOTE_IMPERECHERE WHERE ID_CNOTE_IMPERECHERE = -1');
          Open;
          Append;
          FieldByName('NR_OBL').AsInteger := FIdDocum;
          FieldByName('NR_PLATA').AsInteger  := FIdCasa;
          FieldByName('SUMA').AsCurrency         := edTotal.Value;
          Post;
          FIdDecont := FieldByName('ID_CNOTE_IMPERECHERE').AsInteger;
          Close;
       end
       else begin
          Sql.Add('DELETE FROM CNOTE_DEFALCARE_DECONTARI WHERE ID_CNOTE_IMPERECHERE = '+IntToStr(FIdDecont));
          ExecSql;
       end;
       Sql.Clear;
       Sql.Add('INSERT INTO CNOTE_DEFALCARE_DECONTARI (ID_CNOTE_IMPERECHERE, ID_CNOTE_ITEMSI, SUMA)');
       Sql.Add('VALUES (:ID_GEST_DECO, :ID_CNOTE_ITEMSI, :CURENT)');
       Params.ParamByName('ID_GEST_DECO').Value := FIdDecont;
       DataSource := DTDefalcareDecontare;
       DBDefalcareDecont1.First;
       while not DBDefalcareDecont1.Eof do begin
         ExecSQL;
         DBDefalcareDecont1.Next;
       end;
    finally
       Free;
    end;
end;

procedure TfrmDefalcareNote.BtnOkClick(Sender: TObject);
begin
  edTotal.Value := ATSEvaluator.Formule[0].Calculate;
  if edInainte.Value <> edTotal.Value then
     if MessageDlg('Suma defalcarilor este diferita de suma specificata anterior'#13#10'Doriti folosirea sumei defalcate?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  ModalResult := mrOk;
end;

constructor TfrmDefalcareNote.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  ATSEvaluator := TATSEvaluator.Create(nil);
end;

procedure TfrmDefalcareNote.ActivateEvaluator;
begin
   if ATSEvaluator.FormuleCount = 0 then begin
     ATSEvaluator.Active := False;
     ATSEvaluator.AddDataSet(DBDefalcareDecont1);
     ATSEvaluator.AddFormula(DBDefalcareDecont1, 'null', 'sum(CURENT)');
     ATSEvaluator.Active := True;
   end;
end;

procedure TfrmDefalcareNote.ATSEvaluatorChangeField(SrcDataSet: TDataSet;
  SrcField: TField; DestDataSet: TDataSet; DestField: TField;
  const FieldName: String; Value: Variant; Modified: Boolean);
begin
  if Value > edInainte.Value then begin
     DBDefalcareDecont1.Cancel;
     raise EContaHandledError.Create('Depasiti suma totala pentru decontarea curenta '+FormatFloat(',0.00;-,0.00', Value)+ ' > '+FormatFloat(',0.00;-,0.00', edInainte.Value));
  end;
  edTotal.Value := Value;
end;

end.
