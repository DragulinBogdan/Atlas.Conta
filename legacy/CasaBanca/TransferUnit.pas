unit TransferUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, dxCntner, dxEditor, dxExEdtr, dxEdLib, CommonCasa,
  ExtCtrls, HeadPanel, dxDBCtrl, dxDBTL;

type
  TfrmTransfer = class(TForm)
    StyleController: TdxEditStyleController;
    pnTop: THeadPanel;
    pnRest: TPanel;
    pnBottom: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    lbDataIesire: TLabel;
    lbDataIntrare: TLabel;
    lbNrDec: TLabel;
    lbDataDec: TLabel;
    edTransferHouse: TdxPopupEdit;
    chkConfirm: TdxCheckEdit;
    edtSuma: TdxCurrencyEdit;
    edtCasa: TdxEdit;
    edtDataPlec: TdxDateEdit;
    edtDataDest: TdxDateEdit;
    ledDataDec: TdxDateEdit;
    edtNrDec: TdxSpinEdit;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    btnDecont: TSpeedButton;
    procedure chkConfirmClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure edTransferHouseChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edtNrDecChange(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure edTransferHouseCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure FormShow(Sender: TObject);
    procedure btnDecontClick(Sender: TObject);
    procedure edTransferHouseKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    FPrintingBon: Boolean;
    FCodGest: String;
    FHouseIndex: Integer;
    FTipDestinatie: TSetTransferTo;
    FDefaultIndex: Integer;
    FContCrsp: String;
    procedure SetCodGest(const Value: String);
    procedure SetTipDestinatie(const Value: TSetTransferTo);
    procedure SetRight(aId : Integer);
    procedure SetDefaultIndex(const Value: Integer);
    procedure SetContCrsp(const Value: String);
    { Private declarations }
  public
    { Public declarations }
    Deconturi :  TStringList;
    FTreeList  : TdxDBTreeList;
    procedure SetTreeList;
    procedure DestroyTreeList;
    procedure PrintBonTransfer;
    property PrintingBon : Boolean read FPrintingBon write FPrintingBon;
    property CodGest : String read FCodGest write SetCodGest;
    property ContCrsp: String read FContCrsp write SetContCrsp;
    property HouseIndex : Integer read FHouseIndex write FHouseIndex default -1;
    property TipDestinatie : TSetTransferTo read FTipDestinatie write SetTipDestinatie;
    property DefaultIndex : Integer read FDefaultIndex write SetDefaultIndex default -1;
  end;

implementation

uses CommonDBVar, dxTL, DecontPickUnit, DB;

{$R *.DFM}

{ TfrmTransfer }
procedure TfrmTransfer.PrintBonTransfer;
begin
  //todo sa se printeze un bon de transfer
end;

procedure TfrmTransfer.chkConfirmClick(Sender: TObject);
begin
  //controls
  edtDataPlec.Visible := not(chkConfirm.Checked);
  edtDataDest.Visible := not(chkConfirm.Checked);
  //labels
  lbDataIesire.Visible  := not(chkConfirm.Checked);
  lbDataIntrare.Visible := not(chkConfirm.Checked);
end;

procedure TfrmTransfer.FormCreate(Sender: TObject);
begin
  FHouseIndex := -1;
  FDefaultIndex := -1;
  Deconturi := TStringList.Create;
end;

procedure TfrmTransfer.btnOkClick(Sender: TObject);
begin
  //edtDataPlec.ValidateEdit;
  //edtDataDest.ValidateEdit;

  if HouseIndex = -1 then raise EContaHandledError.Create('Va rugam selectati casa destinatie !');
  if ((tt_BancaDecont in TipDestinatie) or (tt_Casadecont in TipDestinatie)) then begin
     if edtNrDec.IntValue <=0 then raise EContaHandledError.Create('Va rugam precizati un NUMAR DECONT !');
     if IsValidDateStr(ledDataDec.EditText)  then ledDataDec.ValidateEdit;
     if not IsValidDateStr(ledDataDec.EditText) then raise EContaHandledError.Create('Va rugam precizati o DATA pentru DECONT !');
  end;
  if FPrintingBon then PrintBonTransfer;
  ModalResult := mrOk;
{  if FIsAvans then begin}
     {trebuie sa verificam daca mai exista acest numar}
 { end;}
end;

procedure TfrmTransfer.edTransferHouseChange(Sender: TObject);
begin
  BtnOk.Enabled := edTransferHouse.Text <> '';
end;

procedure TfrmTransfer.FormDestroy(Sender: TObject);
begin
  DestroyTreeList;
  Deconturi.Free;
end;

procedure TfrmTransfer.edtNrDecChange(Sender: TObject);
var Index : Integer;
    aDate : PDecontInf;
    FindStr : String;
begin
  if Trim(edTransferHouse.Text) = '' then Exit;
  if Trim(FCodGest) = '' then
    FindStr := IntToStr(FHouseIndex)+ '|'+edtNrDec.Text
  else
    FindStr := IntToStr(FHouseIndex)+ '|'+edtNrDec.Text + '~' + Trim(FCodGest);
  if Deconturi.Find(FindStr, Index) then
    if Assigned(Deconturi.Objects[Index]) then begin
      aDate :=  PDecontInf(Deconturi.Objects[Index]);
      ledDataDec.Date := aDate.DataDecont;
    end;
end;

procedure TfrmTransfer.SetCodGest(const Value: String);
var I : Integer;
    S : String;
begin
  FCodGest := Value;
  if Trim(FCodGest) = '' then begin
    Deconturi.Sorted := False;
    for I := 0 to Deconturi.Count - 1 do begin
      S := Deconturi.Strings[I];
      S := Copy(S, 1, Pos('~',S));
      Deconturi.Strings[I] := S;
    end;
    Deconturi.Sorted := True;
  end;

  if FCodGest <> '' then
    with FTreeList.DataSource.DataSet do
      try
        if Locate('ID_REPARTITORI', FCodGest, []) then
          FHouseIndex := FieldByName('COD_CB').AsInteger;
      except
      end;
end;

procedure TfrmTransfer.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmTransfer.SetTipDestinatie(const Value: TSetTransferTo);
var
  aFilter : String;
  aDataSource : TDataSource;


 function AddToFilter(Filter, NewFilter : String) : String;
 begin
   if Trim(Filter) = '' then
     Result := NewFilter
   else
     Result := Filter + ' OR ' + NewFilter;
 end;

begin
  FTipDestinatie := Value;

  aDataSource := FTreeList.DataSource;
  FTreeList.DataSource := nil;
  aDataSource.DataSet.Filtered := False;
  aFilter := '';
  if tt_Casa in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 0 AND IS_TEMPOR = 0 AND IS_BANCA = 0 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  if tt_Banca in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 0 AND IS_TEMPOR = 0 AND IS_BANCA = 1 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  if tt_Casadecont in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 1 AND IS_TEMPOR = 0 AND IS_BANCA = 0 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  if tt_BancaDecont in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 1 AND IS_TEMPOR = 0 AND IS_BANCA = 1 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  if tt_CasaTempor in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 1 AND IS_TEMPOR = 1 AND IS_BANCA = 0 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  if tt_BancaTempor in FTipDestinatie then
      aFilter := AddToFilter(aFilter, '(IS_AVANS = 1 AND IS_TEMPOR = 1 AND IS_BANCA = 0 AND COD_CB <> '+ IntToStr(FDefaultIndex)+')');

  aDataSource.DataSet.Filter := aFilter;
  aDataSource.DataSet.Filtered := (Trim(aDataSource.DataSet.Filter) <> '');
  FTreeList.DataSource  :=  aDataSource;
  FTreeList.TopNode.MakeVisible;
  FTreeList.TopNode.Focused := True;
  SetDefaultIndex(FDefaultIndex);
end;

procedure TfrmTransfer.edTransferHouseCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var aNode : TdxDBTreeListNode;
begin
 if Accept then begin
   with TdxPopupEdit(Sender) do
       aNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(aNode) then begin
          Text := Trim(aNode.Strings[FTreeList.FindColumnByFieldName('DENUMIRE').Index]);
          FHouseIndex := aNode.Values[FTreeList.FindColumnByFieldName('COD_CB').Index];
          SetRight(FHouseIndex);
       end;
 end
 else begin
   Text := '';
   FHouseIndex := -1;
 end;
end;

procedure TfrmTransfer.SetTreeList;
begin
  edTransferHouse.PopupControl := FTreeList;
  edTransferHouse.PopupFormBorderStyle := pbsSysPanel;
end;

procedure TfrmTransfer.SetRight(aId: Integer);
var aTipCasa : TTipCasa;
begin
  if IsRightEnable then begin
    aTipCasa := nil;
    CommonCasa.GetTipCasa(aId, FTreeList.DataSource.DataSet, aTipCasa);

    if IsAdmin then chkConfirm.Enabled := True
    else
      if td_Administrator in aTipCasa.Drepturi then chkConfirm.Enabled := True
      else
        if td_Validator in aTipCasa.Drepturi then chkConfirm.Enabled := False
        else
          if td_Casier in aTipCasa.Drepturi then chkConfirm.Enabled := False;
  end;
  chkConfirm.Enabled := chkConfirm.Enabled and ([tt_CasaDecont, tt_BancaDecont] * TipDestinatie = []);
  chkConfirmClick(nil);
end;

procedure TfrmTransfer.SetDefaultIndex(const Value: Integer);
begin
  FDefaultIndex := Value;
end;

procedure TfrmTransfer.FormShow(Sender: TObject);
var aNode  : TdxTreeListNode;
    OK : Boolean;
    FIsAvansLocal : Boolean;
begin
 FIsAvansLocal := ((tt_BancaDecont in TipDestinatie) or (tt_Casadecont in TipDestinatie));

 lbNrDec.Visible    := FIsAvansLocal;
 lbDataDec.Visible  := FIsAvansLocal;
 edtNrDec.Visible   := FIsAvansLocal;
 ledDataDec.Visible := FIsAvansLocal;
 btnDecont.Visible  := FIsAvansLocal;

  if FIsAvansLocal then begin
   chkConfirm.Visible := False;
   chkConfirm.Checked := False;
   chkConfirmClick(nil);
  end;


 OK := False;
 if FHouseIndex > -1  then
    if Assigned(FTreeList) then begin
       aNode := FTreeList.FindNodeByKeyValue(FHouseIndex);
       if Assigned(aNode) then begin
         aNode.MakeVisible;
         aNode.Focused := True;
         edTransferHouse.Text := aNode.Strings[FTreeList.FindColumnByFieldName('DENUMIRE').Index];
         FHouseIndex := aNode.Values[FTreeList.FindColumnByFieldName('COD_CB').Index];
         SetRight(FHouseIndex);
         OK := True;
       end;
    end;
 if not OK then
    if Assigned(FTreeList) then begin
      if FTreeList.Count = 1 then
        if FTreeList.Items[0].Count = 1 then begin
          aNode := FTreeList.Items[0].Items[0];
          if Assigned(aNode) then begin
            aNode.MakeVisible;
            aNode.Focused := True;
            edTransferHouse.Text := aNode.Strings[FTreeList.FindColumnByFieldName('DENUMIRE').Index];
            FHouseIndex := aNode.Values[FTreeList.FindColumnByFieldName('COD_CB').Index];
            SetRight(FHouseIndex);
            OK := True;
          end;
        end;
    end;


 if not OK then
     edTransferHouse.DroppedDown := True;
end;

procedure TfrmTransfer.DestroyTreeList;
begin
  if (FTreeList <> nil) and (FTreeList.DataSource.DataSet.Filtered) then begin
    FTreeList.DataSource.DataSet.Filtered := False;
    FTreeList.DataSource.DataSet.Filter := '';
    FTreeList := nil;
  end;
end;

procedure TfrmTransfer.btnDecontClick(Sender: TObject);
var
  frmDecontPick : TfrmDecontPick;
  aNode : TdxTreeListNode;
begin
   frmDecontPick := TFrmDecontPick.Create(Self);
   with frmDecontPick do
     try
       CasaPlecare := FDefaultIndex;
       if FHouseIndex > -1 then
          CasaSosire  := FHouseIndex;
       ShowModal;
       if ModalResult = mrOk then begin
         edtNrDec.OnChange := nil;
         edtNrDec.IntValue := NrDecont;
         ledDataDec.Date := DataDecont;
         if FHouseIndex = -1 then begin
            FHouseIndex := ArriveHouse;
            if Assigned(FTreeList) then begin
               aNode := FTreeList.FindNodeByKeyValue(FHouseIndex);
               if Assigned(aNode) then begin
                 aNode.MakeVisible;
                 aNode.Focused := True;
                 edTransferHouse.Text := aNode.Strings[FTreeList.FindColumnByFieldName('DENUMIRE').Index];
                 FHouseIndex := aNode.Values[FTreeList.FindColumnByFieldName('COD_CB').Index];
                 SetRight(FHouseIndex);
               end;
            end;
         end;
       end;
     finally
       Free;
     end;
end;

procedure TfrmTransfer.edTransferHouseKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_DELETE then begin
    edTransferHouse.Text := '';
    FHouseIndex := -1;
  end;
end;

procedure TfrmTransfer.SetContCrsp(const Value: String);
var
  lNode   : TdxTreeListNode;
  lColumn : TdxDBTreeListColumn;

    function IsCorrectNode(ANode: TdxTreeListNode): Boolean;
    begin
      Result := SameText(ANode.Strings[lColumn.Index], Value);
    end;

    function FindCasa: TdxTreeListNode;
    var
      I, J: Integer;
    begin
      Result := nil;
      for I := 0 to FTreeList.Count-1 do begin
        Result := FTreeList.Items[I];
        if IsCorrectNode(Result) then begin
          Break;
        end
        else begin
          Result := nil;
          for J := 0 to FTreeList.Items[I].Count-1 do begin
            Result := FTreeList.Items[I].Items[J];
            if IsCorrectNode(Result) then
              Break
            else
              Result := nil;
          end;
        end;
      end;
    end;

begin
  FContCrsp := Value;
  if Assigned(FTreeList) then begin
    lColumn := FTreeList.ColumnByFieldName('CRSP_LEI');
    if Assigned(lColumn) then begin
      lNode := FindCasa;
      if not Assigned(lNode) then begin
        if tt_BancaDecont in TipDestinatie then
          raise Exception.CreateFmt('Nu exista banca de decont care sa aiba contul corespondent %s', [FContCrsp]);
        if tt_CasaDecont in TipDestinatie then
          raise Exception.CreateFmt('Nu exista casa de decont care sa aiba contul corespondent %s', [FContCrsp]);
      end
      else begin
        edTransferHouse.Text := Trim(lNode.Strings[FTreeList.FindColumnByFieldName('DENUMIRE').Index]);
        SetRight(lNode.Values[FTreeList.FindColumnByFieldName('COD_CB').Index]);
      end;
    end;
  end;
end;

end.
