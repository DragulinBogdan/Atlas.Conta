unit InflProduseFields;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, cxControls, cxContainer, cxEdit,
  cxGroupBox, StdCtrls, cxButtons, ExtCtrls, ZDataSet, DB, cxCheckGroup,
  cxCheckBox, Menus, 
  cxGraphics,
  cxLookAndFeels;

type
  TfrmInflProdusFields = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    grpTipProd: TcxGroupBox;
    lbField: TLabel;
    procedure btnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    FFieldName: String;
    FIdGestDefaDocum: Integer;
    procedure SetFieldName(const Value: String);
    { Private declarations }
  public
    { Public declarations }
    //procedure PopulateCurent(IdGestTipStoc : Integer; IdGestTipDocum : Integer; TipPredator : Integer; SemnItems : Integer);
    procedure PopulateScreenWithProd;
    procedure SetCurentTipProduse;
    property FieldName : String read FFieldName write SetFieldName;
    property IdGestDefaDocum : Integer read FIdGestDefaDocum write FIdGestDefaDocum;
  end;


procedure InfluentareTipProduseField(lIdGestDefaDocum: Integer; lFieldName :String);



implementation

uses DateUnit;

{$R *.dfm}

procedure InfluentareTipProduseField(lIdGestDefaDocum: Integer; lFieldName :String);
var
  lfrmInflProdusFields: TfrmInflProdusFields;
begin
  lfrmInflProdusFields := TfrmInflProdusFields.Create(nil);
  lfrmInflProdusFields.PopulateScreenWithProd;
  with lfrmInflProdusFields do
  try
    IdGestDefaDocum := lIdGestDefaDocum;
    FieldName := lFieldName;
    SetCurentTipProduse;
    ShowModal;
  finally
    Free;
  end;
end;


{ TfrmInflTipProduse }

procedure TfrmInflProdusFields.PopulateScreenWithProd;
var
  aQry : TZReadOnlyQuery;
  I : Integer;
  aCheckBox : TcxCheckBox;
begin
  for I := 0 to grpTipProd.ComponentCount -1 do
    if grpTipProd.Components[I] is TcxCheckBox then TcxCheckBox(grpTipProd.Components[I]).Free;

  aQry := GetTmpADOQuery;
  I := 0;
  with aQry do
    try
      SQL.Clear;
      SQL.Add('SELECT * FROM GEST_TIP_PRODUSE');
      Open;
      First;
      I := 0;
      while not eof do begin
        aCheckBox := TcxCheckBox.Create(Self);
        aCheckBox.Parent := grpTipProd;
        aCheckBox.Top := 10 + I * 20;
        aCheckBox.Left := 10;
        aCheckBox.Caption := FieldByName('DENUMIRE').AsString;
        aCheckBox.AutoSize := True;
        aCheckBox.Tag := FieldByName('ID_GEST_TIP_PRODUSE').AsInteger;
        I := I + 1;
        Next;
      end;
    finally
      Self.Height := 150 + I * 20 + pnBottom.Height;
      Free;
    end;
end;

procedure TfrmInflProdusFields.btnOkClick(Sender: TObject);
var
  I : Integer;
  lStrTipProdus : String;
  aQry : TZReadOnlyQuery;
begin
  lStrTipProdus := '';
  for I := 0 to Self.ComponentCount - 1 do
   if Self.Components[I] is TcxCheckBox then
     if TcxCheckBox(Self.Components[I]).Checked then
       if lStrTipProdus = '' then
         lStrTipProdus := IntToStr(TcxCheckBox(Self.Components[I]).Tag)
       else
         lStrTipProdus := lStrTipProdus + ',' + IntToStr(TcxCheckBox(Self.Components[I]).Tag);

  aQry := GetTmpADOQuery;
  with aQry do
    try
      SQL.Add('exec sp_gest_update_field_produse  :id_gest_defa_docum, :fieldName, :tip_produs');
      Params.ParamByName('id_gest_defa_docum').Value := IdGestDefaDocum;
      Params.ParamByName('fieldName').Value := FieldName;
      if lStrTipProdus = '' then
        Params.ParamByName('tip_produs').Value := null
      else
        Params.ParamByName('tip_produs').Value := lStrTipProdus;
      ExecSQL;
    finally
      Free;
    end;
end;

procedure TfrmInflProdusFields.SetCurentTipProduse;
var
   aQry : TZReadOnlyQuery;
   I : Integer;
begin
  aQry := GetTmpADOQuery;
  with aQry do
  try
    SQL.Add('exec sp_gest_get_def_field_produs :id_gest_defa_docum, :fieldName');
    Params.ParamByName('id_gest_defa_docum').Value := IdGestDefaDocum;
    Params.ParamByName('fieldName').Value := FieldName;
    Open;
    for I := 0 to Self.ComponentCount - 1 do
      if Self.Components[I] is TcxCheckBox then
         TcxCheckBox(Self.Components[I]).Checked := Locate('ID_GEST_TIP_PRODUSE', Self.Components[I].Tag, []);
  finally
    Free;
  end;
end;

procedure TfrmInflProdusFields.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  btnOk.Click;
end;

procedure TfrmInflProdusFields.SetFieldName(const Value: String);
begin
  FFieldName := Value;
  lbField.Caption := Value;
end;

end.
