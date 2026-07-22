unit AlegDecontUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, dxCntner, dxEditor, dxExEdtr, dxEdLib, CommonCasa,
  ExtCtrls, HeadPanel, dxDBCtrl, dxfQuickTyp, dxDBTL;

type
  TfrmAlegDecont = class(TForm)
    StyleController: TdxEditStyleController;
    pnTop: THeadPanel;
    pnRest: TPanel;
    pnBottom: TPanel;
    lbNrDec: TLabel;
    lbDataDec: TLabel;
    edtDataDec: TdxDateEdit;
    edtNrDec: TdxSpinEdit;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    btnDecont: TSpeedButton;
    Label1: TLabel;
    edtRep: TdxPopupEdit;
    procedure btnOkClick(Sender: TObject);
    procedure edtNrDecChange(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure btnDecontClick(Sender: TObject);
    procedure edtRepEnter(Sender: TObject);
    procedure edtRepKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtRepCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    FCodGest: Integer;
    FNrDecont: Integer;
    FDataDecont: TDateTime;
    OriginalDeconturi : TStringList;
    FHouseIndex: Integer;
    FDeconturi: TStringList;
    procedure SetCodGest(const Value: Integer);
    procedure SetDeconturi(const Value: TStringList);
    procedure SetDataDecont(const Value: TDateTime);
    procedure SetNrDecont(const Value: Integer);
  protected
    { Private declarations }
  public
    { Public declarations }
    FTreeList  : TdxDBTreeList;
    property Deconturi :  TStringList read FDeconturi write SetDeconturi;
    property CodGest : Integer read FCodGest write SetCodGest;
    property HouseIndex : Integer read FHouseIndex write FHouseIndex;
    property NrDecont : Integer read FNrDecont write SetNrDecont;
    property DataDecont : TDateTime read FDataDecont write SetDataDecont;
  end;


var frmAlegDecont : TfrmAlegDecont;

implementation

uses CommonDBVar, dxTL, DecontPickUnit, DB, ContainerUnit;

{$R *.DFM}



{ TfrmAlegDecont }

procedure TfrmAlegDecont.btnOkClick(Sender: TObject);
begin
  if IsValidDateStr(edtDataDec.Text)  then edtDataDec.ValidateEdit;
  if not IsValidDateStr(edtDataDec.Text) then raise EContaHandledError.Create('Va rugam precizati o DATA pentru DECONT !');
  ModalResult := mrOk;
end;

procedure TfrmAlegDecont.edtNrDecChange(Sender: TObject);
var Index : Integer;
    aDate : PDecontInf;
    FindStr : String;
begin
  if FCodGest = -1 then
    FindStr := IntToStr(FHouseIndex)+ '|'+edtNrDec.Text + '~'
  else
    FindStr := IntToStr(FHouseIndex)+ '|'+edtNrDec.Text + '~' + Trim(IntToStr(FCodGest));
  if Deconturi.Find(FindStr, Index) then
    if Assigned(Deconturi.Objects[Index]) then begin
      aDate :=  PDecontInf(Deconturi.Objects[Index]);
      edtDataDec.Date := aDate.DataDecont;
    end;
end;

procedure TfrmAlegDecont.SetCodGest(const Value: Integer);
var I : Integer;
    S : String;
    aNode : TdxDBTreeListNode;
begin
  if OriginalDeconturi <> nil then
    Deconturi.Assign(OriginalDeconturi);
  FCodGest := Value;
  if FCodGest = -1 then begin
   FDeconturi.Sorted := False;
    for I := 0 to Deconturi.Count - 1 do begin
      S := FDeconturi.Strings[I];
      S := Copy(S, 1, Pos('~',S));
      FDeconturi.Strings[I] := S;
    end;
    FDeconturi.Sorted := True;
  end;

  if FCodGest <> -1 then begin
     edtRep.Text := '';
     if not Assigned(edtRep.PopupControl) then Exit;
     if not (edtRep.PopupControl is TdxDBTreeList) then Exit;
     aNode := TdxDBTreeList(edtRep.PopupControl).FindNodeByKeyValue(FCodGest);
     if aNode = nil then Exit;
     edtRep.Text := aNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index];
  end;
end;

procedure TfrmAlegDecont.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;


procedure TfrmAlegDecont.btnDecontClick(Sender: TObject);
var
  frmDecontPick : TfrmDecontPick;
begin
   frmDecontPick := TFrmDecontPick.Create(Self);
   with frmDecontPick do
     try
       DechideSetDate(-1,FHouseIndex, FCodGest);
       ShowModal;
       if ModalResult = mrOk then begin
         edtNrDec.OnChange := nil;
         edtNrDec.IntValue := NrDecont;
         edtDataDec.Date := DataDecont;
         edtNrDec.OnChange := edtNrDecChange;
         CodGest := CodRepartitor;
       end;
     finally
       Free;
     end;
end;

procedure TfrmAlegDecont.edtRepEnter(Sender: TObject);
begin
  if Trim(edtRep.Text) ='' then
    edtRep.DroppedDown := True;
end;

procedure TfrmAlegDecont.edtRepKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_DELETE then begin
    edtRep.Text := '';
    CodGest := -1;
  end;
end;

procedure TfrmAlegDecont.edtRepCloseUp(Sender: TObject; var Text: String;
  var Accept: Boolean);
var aNode : TdxDBTreeListNode;
begin
 if Accept then begin
   with TdxPopupEdit(Sender) do
       aNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(aNode) then begin
          CodGest := aNode.Id;
          Text := Trim(aNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index]);
       end;
    end;
end;

procedure TfrmAlegDecont.FormCreate(Sender: TObject);
begin
     FDeconturi := TStringList.Create;
     FDeconturi.Sorted := True;
     OriginalDeconturi := nil;
end;

procedure TfrmAlegDecont.FormDestroy(Sender: TObject);
begin
     FDeconturi.Free;
     OriginalDeconturi.Free;
end;

procedure TfrmAlegDecont.SetDeconturi(const Value: TStringList);
begin
     FDeconturi.Assign(Value);
     if (OriginalDeconturi = nil) and Assigned(FDeconturi) then begin
        OriginalDeconturi := TStringList.Create;
        OriginalDeconturi.Assign(FDeconturi);
     end;
end;


procedure TfrmAlegDecont.SetDataDecont(const Value: TDateTime);
begin
  FDataDecont := Value;
  edtDataDec.Date := FDataDecont;
end;

procedure TfrmAlegDecont.SetNrDecont(const Value: Integer);
begin
  FNrDecont := Value;
  edtNrDec.IntValue := FNrDecont;
end;

end.
