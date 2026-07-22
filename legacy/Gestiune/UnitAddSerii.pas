unit UnitAddSerii;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Buttons, ExtCtrls, HeadPanel, dxExEdtr, dxCntner,
  dxEditor, StdCtrls, ZDataSet, Menus, cxLookAndFeelPainters, cxButtons,
  cxMaskEdit, cxSpinEdit, cxControls, cxContainer, cxEdit, cxTextEdit,
  cxCheckBox, cxCurrencyEdit, 
  cxGraphics,
  cxLookAndFeels;

type
  TfrmAddSerii = class(TForm)
    pnTop: THeadPanel;
    pnBottom: TPanel;
    pnRest: TPanel;
    StyleController: TdxEditStyleController;
    btnAccept: TcxButton;
    btnCancel: TcxButton;
    edtGrupaPrefix: TcxTextEdit;
    Label1: TLabel;
    Label2: TLabel;
    Bevel1: TBevel;
    Label3: TLabel;
    edtPozitii: TcxSpinEdit;
    Label4: TLabel;
    Label5: TLabel;
    edtGrupaSufix: TcxTextEdit;
    edtGrupaNI: TcxSpinEdit;
    edtGrupaStep: TcxSpinEdit;
    edtGrupaFormat: TcxSpinEdit;
    lbGrupaNI: TLabel;
    lbGrupaStep: TLabel;
    lbGrupaFormat: TLabel;
    chkGrupaSN: TcxCheckBox;
    Label9: TLabel;
    edtDenPrefix: TcxTextEdit;
    Label10: TLabel;
    edtDenSufix: TcxTextEdit;
    chkDenSN: TcxCheckBox;
    edtDenNI: TcxSpinEdit;
    lbDenNI: TLabel;
    lbDenStep: TLabel;
    edtDenStep: TcxSpinEdit;
    lbDenFormat: TLabel;
    edtDenFormat: TcxSpinEdit;
    Bevel2: TBevel;
    Bevel3: TBevel;
    Bevel4: TBevel;
    Bevel5: TBevel;
    Bevel6: TBevel;
    edtGrupaStart: TcxTextEdit;
    edtDenStart: TcxTextEdit;
    edtGrupaEnd: TcxTextEdit;
    edtDenEnd: TcxTextEdit;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    procedure btnAcceptClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure chkGrupaSNPropertiesChange(Sender: TObject);
    procedure chkDenSNPropertiesChange(Sender: TObject);
    procedure edtGrupaPrefixPropertiesChange(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    function FormatSpecific (Prefix : String; Sufix : String; Number : Integer;  NumberDisplay: Integer; DisplayBit : Boolean) : string;
    procedure GenNumbers;
  end;


function AddSeriiFromCurrent(lCulgestDocum : Integer; lCulgestItemsi : Integer) : Boolean;


implementation

uses DateUnit, DB;

{$R *.dfm}
function AddSeriiFromCurrent(lCulgestDocum : Integer; lCulgestItemsi : Integer) : Boolean;
var
   lfrm  : TfrmAddSerii;
   lQry : TZReadOnlyQuery;
begin
  Result := False;
  lQry := GetTmpADOQuery;
  lfrm := TfrmAddSerii.Create(nil);

  with lfrm, lQry do
  try
    lQry.SQL.Add('exec spGestInfoOnCul ' + IntToStr(lCulgestDocum) + ', ' + IntToStr(lCulgestItemsi));
    lQry.Open;
    if lQry.IsEmpty then Exit;
    edtGrupaPrefix.Text  := FieldByName('grupaPrefix').AsString;
    edtGrupaSufix.Text   := FieldByName('grupaSufix').AsString;
    edtGrupaNI.Value     := FieldByName('grupaNR').AsInteger;
    edtGrupaStep.Value   := FieldByName('grupaStep').AsInteger;
    edtGrupaFormat.Value := FieldByName('grupaFormat').AsInteger;
    chkGrupaSN.Checked   := FieldByName('grupaHasNr').AsBoolean;

    edtDenPrefix.Text    := FieldByName('denPrefix').AsString;
    edtDenSufix.Text     := FieldByName('denSufix').AsString;
    edtDenNI.Value       := FieldByName('denNR').AsInteger;
    edtDenStep.Value     := FieldByName('denStep').AsInteger;
    edtDenFormat.Value   := FieldByName('denFormat').AsInteger;
    chkDenSN.Checked     := FieldByName('denHasNr').AsBoolean;

    chkGrupaSNPropertiesChange(nil);
    chkDenSNPropertiesChange(nil);
    lQry.Close;
    lQry.SQL.Clear;
    ShowModal;
    if ModalResult = mrOk then begin
      lQry.SQL.Add('exec spCulGestAddSerii ' + IntToStr(lCulgestDocum) + ', ' + IntToStr(lCulgestItemsi) + ',' + IntToStr(edtPozitii.Value)+', :grupaHasNr, :denHasNr,');
      lQry.SQL.Add(' :grupaPrefix, :grupaSufix, :grupaNR, :grupaStep, :grupaFormat, :denPrefix, :denSufix, :denNR, :denStep, :denFormat');
      with lQry.Params do begin
        ParamByName('grupaPrefix').Value := lfrm.edtGrupaPrefix.Text;
        ParamByName('grupaSufix').Value := lfrm.edtGrupaSufix.Text;
        ParamByName('grupaNR').Value := lfrm.edtGrupaNI.Value;
        ParamByName('grupaStep').Value := lfrm.edtGrupaStep.Value;
        ParamByName('grupaFormat').Value := lfrm.edtGrupaFormat.Value;
        ParamByName('grupaHasNr').Value := lfrm.chkGrupaSN.Checked;

        ParamByName('denPrefix').Value := lfrm.edtDenPrefix.Text;
        ParamByName('denSufix').Value := lfrm.edtDenSufix.Text;
        ParamByName('denNR').Value := lfrm.edtDenNI.Value;
        ParamByName('denStep').Value := lfrm.edtDenStep.Value;
        ParamByName('denFormat').Value := lfrm.edtDenFormat.Value;
        ParamByName('denHasNr').Value := lfrm.chkDenSN.Checked;
      end;
      try
        lQry.ExecSQL;
        Result := True;
      except
        Result := False;
      end;
    end;
  finally
    lfrm.Free;
    lQry.Free;
  end;
end;


procedure TfrmAddSerii.btnAcceptClick(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TfrmAddSerii.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmAddSerii.chkGrupaSNPropertiesChange(Sender: TObject);
begin
  edtGrupaNI.Visible := chkGrupaSN.Checked;
  edtGrupaStep.Visible := chkGrupaSN.Checked;
  edtGrupaFormat.Visible := chkGrupaSN.Checked;
  lbGrupaNI.Visible := chkGrupaSN.Checked;
  lbGrupaStep.Visible := chkGrupaSN.Checked;
  lbGrupaFormat.Visible := chkGrupaSN.Checked;
  GenNumbers;
end;

procedure TfrmAddSerii.GenNumbers;
begin
  edtGrupaStart.Text := FormatSpecific(edtGrupaPrefix.Text, edtGrupaSufix.Text, edtGrupaNI.Value, edtGrupaFormat.Value, chkGrupaSN.Checked);
  edtGrupaEnd.Text := FormatSpecific(edtGrupaPrefix.Text, edtGrupaSufix.Text, edtGrupaNI.Value + edtGrupaStep.Value * edtPozitii.Value, edtGrupaFormat.Value, chkGrupaSN.Checked);
  edtDenStart.Text := FormatSpecific(edtDenPrefix.Text, edtDenSufix.Text, edtDenNI.Value, edtDenFormat.Value, chkDenSN.Checked);
  edtDenEnd.Text := FormatSpecific(edtDenPrefix.Text, edtDenSufix.Text, edtDenNI.Value + edtDenStep.Value * edtPozitii.Value, edtDenFormat.Value, chkDenSN.Checked);
end;

procedure TfrmAddSerii.chkDenSNPropertiesChange(Sender: TObject);
begin
  edtDenNI.Visible := chkDenSN.Checked;
  edtDenStep.Visible := chkDenSN.Checked;
  edtDenFormat.Visible := chkDenSN.Checked;
  lbDenNI.Visible := chkDenSN.Checked;
  lbDenStep.Visible := chkDenSN.Checked;
  lbDenFormat.Visible := chkDenSN.Checked;
  GenNumbers;
end;

function TfrmAddSerii.FormatSpecific(Prefix, Sufix: String; Number, NumberDisplay: Integer; DisplayBit : Boolean): string;
var
  lNumber : string;
begin
  Result := '';
  if DisplayBit then begin
    lNumber := IntToStr(Number);
    if Length(lNumber) < NumberDisplay then
      lNumber := StringOfChar('0',NumberDisplay - Length(lNumber)) + lNumber;
    Result := lNumber;
  end;
  Result := Prefix + Result + Sufix;
end;

procedure TfrmAddSerii.edtGrupaPrefixPropertiesChange(Sender: TObject);
begin
   GenNumbers;
end;

end.
