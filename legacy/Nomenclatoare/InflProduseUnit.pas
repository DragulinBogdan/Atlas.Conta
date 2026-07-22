unit InflProduseUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, cxControls, cxContainer, cxEdit,
  cxGroupBox, StdCtrls, cxButtons, ExtCtrls, ZDataSet, DB, cxCheckGroup,
  cxCheckBox, Menus, 
  cxGraphics,
  cxLookAndFeels;

type
  TfrmInflTipProduse = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    grpTipProd: TcxGroupBox;
    procedure btnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    FIdGestTipStoc: Integer;
    FIdGestTipDocum: Integer;
    FSemnItems: Integer;
    FTipPredator: Integer;
    FSemn: Variant;
    FOldSemn: Variant;
    { Private declarations }
  public
    { Public declarations }
    //procedure PopulateCurent(IdGestTipStoc : Integer; IdGestTipDocum : Integer; TipPredator : Integer; SemnItems : Integer);
    procedure PopulateScreenWithProd;
    procedure SetCurentTipProduse;
    property IdGestTipStoc : Integer read FIdGestTipStoc write FIdGestTipStoc;
    property IdGestTipDocum : Integer read FIdGestTipDocum write FIdGestTipDocum;
    property TipPredator : Integer read FTipPredator write FTipPredator;
    property SemnItems : Integer read FSemnItems write FSemnItems;
    property Semn : Variant read FSemn write FSemn;
    property OldSemn : Variant read FOldSemn write FOldSemn;
  end;


procedure InfluentareTipProduse(lIdGestTipStoc : Integer; lIdGestTipDocum : Integer; lTipPredator : Integer; lSemnItems : Integer; lSemn : Variant; const lOldSemn : Variant);



implementation

uses DateUnit;

{$R *.dfm}

procedure InfluentareTipProduse(lIdGestTipStoc : Integer; lIdGestTipDocum : Integer; lTipPredator : Integer; lSemnItems : Integer; lSemn : Variant; const lOldSemn : Variant);
var
  frmInflTipProduse: TfrmInflTipProduse;
begin
  frmInflTipProduse := TfrmInflTipProduse.Create(nil);
  frmInflTipProduse.PopulateScreenWithProd;
  with frmInflTipProduse do
  try
    IdGestTipStoc := lIdGestTipStoc;
    IdGestTipDocum := lIdGestTipDocum;
    TipPredator :=  lTipPredator;
    SemnItems := lSemnItems;
    Semn := lSemn;
    if lOldSemn = -999 then
      OldSemn := Semn
    else
      OldSemn := lOldSemn;
    SetCurentTipProduse;
    if lSemn = Null then
      btnOkClick(nil)
    else
      ShowModal;
  finally
    Free;
  end;
end;


{ TfrmInflTipProduse }

procedure TfrmInflTipProduse.PopulateScreenWithProd;
var
  aQry : TZReadOnlyQuery;
  I : Integer;
  aCheckBox : TcxCheckBox;
begin
  for I := 0 to grpTipProd.ComponentCount -1 do
    if grpTipProd.Components[I] is TcxCheckBox then TcxCheckBox(grpTipProd.Components[I]).Free;

  aQry := GetTmpADOQuery;
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
      Self.Height := 50 + I * 20 + pnBottom.Height;
      Free;
    end;
end;

procedure TfrmInflTipProduse.btnOkClick(Sender: TObject);
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
      SQL.Add('exec sp_gest_update_tip_stoc_produse  :id_gest_tip_stoc, :id_gest_tip_docum, :predator, :semn_items, :semn, :tip_produs');
      Params.ParamByName('id_gest_tip_stoc').Value := IdGestTipStoc;
      Params.ParamByName('id_gest_tip_docum').Value := IdGestTipDocum;
      Params.ParamByName('predator').Value := TipPredator;
      Params.ParamByName('semn_items').Value := SemnItems;
      Params.ParamByName('semn').Value := Semn;
      if lStrTipProdus = '' then
        Params.ParamByName('tip_produs').Value := null
      else
        Params.ParamByName('tip_produs').Value := lStrTipProdus;
      ExecSQL;
    finally
      Free;
    end;
end;

procedure TfrmInflTipProduse.SetCurentTipProduse;
var
   aQry : TZReadOnlyQuery;
   I : Integer;
begin
  aQry := GetTmpADOQuery;
  with aQry do
  try
    SQL.Add('exec sp_gest_get_def_stoc_produs :id_gest_tip_stoc, :id_gest_tip_docum, :predator, :semn_items, :semn');
    Params.ParamByName('id_gest_tip_stoc').Value := IdGestTipStoc;
    Params.ParamByName('id_gest_tip_docum').Value := IdGestTipDocum;
    Params.ParamByName('predator').Value := TipPredator;
    Params.ParamByName('semn_items').Value := SemnItems;
    Params.ParamByName('semn').Value := OldSemn;
    Open;
    for I := 0 to Self.ComponentCount - 1 do
      if Self.Components[I] is TcxCheckBox then
         TcxCheckBox(Self.Components[I]).Checked := Locate('ID_GEST_TIP_PRODUSE', Self.Components[I].Tag, []);
  finally
    Free;
  end;
end;

procedure TfrmInflTipProduse.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  btnOk.Click;
end;

end.
