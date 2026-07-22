unit UnitStocPerioada;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Buttons, ExtCtrls, HeadPanel, dxExEdtr, dxEdLib, dxCntner,
  dxEditor, StdCtrls, ZDataSet, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmCopyStock = class(TForm)
    pnTop: THeadPanel;
    pnBottom: TPanel;
    pnRest: TPanel;
    Label1: TLabel;
    edtDataStock: TdxDateEdit;
    StyleController: TdxEditStyleController;
    Label2: TLabel;
    lbGestiuneStoc: TLabel;
    Label4: TLabel;
    edtContStock: TdxImageEdit;
    edtPredator: TdxEdit;
    btnAccept: TcxButton;
    btnCancel: TcxButton;
    procedure btnAcceptClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
  private
    FPredator: Integer;
    FDataStoc: TDateTime;
    procedure SetPredator(const Value: Integer);
    procedure ReadConturiPredator;
    function GetContStoc: String;
    { Private declarations }
  public
    { Public declarations }
    property DataStoc : TDateTime read FDataStoc write FDataStoc;
    property CodPredator : Integer read FPredator write SetPredator;
    property ContStoc : String read GetContStoc;
  end;


implementation

uses DateUnit, DB;

{$R *.dfm}

procedure TfrmCopyStock.btnAcceptClick(Sender: TObject);
begin
  edtPredator.SetFocus; 
  ModalResult := mrOk;
end;

procedure TfrmCopyStock.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

function TfrmCopyStock.GetContStoc: String;
begin
  Result := edtContStock.Text;
end;

procedure TfrmCopyStock.ReadConturiPredator;
var
  aQry : TZReadOnlyQuery;
begin
  aQry := GetTmpADOQuery;
  with aQry do
    try
      SQL.Add('EXEC SP_GET_CONTURI_GESTIUNE ' + IntToStr(FPredator));
      Open;
      if IsEmpty then
        FPredator := -1
      else begin
        lbGestiuneStoc.Caption := FieldByName('GESTIUNE').AsString;
        if FDataStoc = -1 then
          edtDataStock.Date := Date
        else
          edtDataStock.Date := FDataStoc;
        edtContStock.Clear;
        PopulateImage(aQry, edtContStock.Values, edtContStock.Descriptions,'CONT', 'DENUMIRE_CONT', True, 'Toate');
      end;
    finally
      Free;
      edtPredator.Text := IntToStr(FPredator);
    end;
end;

procedure TfrmCopyStock.SetPredator(const Value: Integer);
begin
  FPredator := Value;
  ReadConturiPredator;
end;

end.
