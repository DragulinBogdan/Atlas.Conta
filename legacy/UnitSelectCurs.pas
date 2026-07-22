unit UnitSelectCurs;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, dxCntner, Buttons, ExtCtrls, HeadPanel, ComCtrls, dxEditor,
  dxEdLib, StdCtrls, dxExEdtr, ZDataSet, cxControls, cxContainer, cxEdit,
  cxTextEdit, cxCurrencyEdit, 
  cxGraphics, cxLookAndFeelPainters,
  cxLookAndFeels;

type
  TfrmSelectCursValutar = class(TForm)
    pnTop: THeadPanel;
    pnBottom: TPanel;
    btnCancel: TSpeedButton;
    pnRest: TPanel;
    StyleController: TdxEditStyleController;
    btnOk: TSpeedButton;
    PageControl: TPageControl;
    tabCursNegociat: TTabSheet;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Bevel1: TBevel;
    edTipValuta: TdxEdit;
    edFurnizor: TdxEdit;
    edContract: TdxImageEdit;
    tabConfigurareCurs: TTabSheet;
    btnOpenNomenclator: TSpeedButton;
    Label5: TLabel;
    Label6: TLabel;
    Bevel2: TBevel;
    Label9: TLabel;
    edDataNomenclator: TdxDateEdit;
    edTipValuta1: TdxEdit;
    tabCursOnline: TTabSheet;
    Label7: TLabel;
    Label8: TLabel;
    Bevel3: TBevel;
    btnCursOnline: TSpeedButton;
    edTipValuta2: TdxEdit;
    edDataCursOnline: TdxDateEdit;
    Label10: TLabel;
    edValoareOnline: TcxCurrencyEdit;
    edValNomenclator: TcxCurrencyEdit;
    edValoareValuta: TcxCurrencyEdit;
    procedure btnCancelClick(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure btnOpenNomenclatorClick(Sender: TObject);
    procedure btnCursOnlineClick(Sender: TObject);
    procedure edTipValuta2Change(Sender: TObject);
  private
    FDataCurs: TDateTime;
    FValutaValue: Currency;
    FTipValuta: Integer;
    FIdRep: Integer;
    FSynonim : String;
    function GetValutaValue: Currency;
    { Private declarations }
  public
    { Public declarations }
    procedure CompleteScreenInformation;
    property  DataCurs    : TDateTime read FDataCurs    write FDataCurs;
    property  TipValuta   : Integer   read FTipValuta   write FTipValuta;
    property  IdRep       : Integer   read FIdRep       write FIdRep;
    property  ValutaValue : Currency  read GetValutaValue write FValutaValue;
  end;


implementation

uses DateUnit, DB, ImportCurs;


{$R *.dfm}

procedure TfrmSelectCursValutar.btnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmSelectCursValutar.btnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmSelectCursValutar.btnOpenNomenclatorClick(Sender: TObject);
begin
  // intretinere nomenclator
end;

procedure TfrmSelectCursValutar.CompleteScreenInformation;
var
  aQry : TZReadOnlyQuery;
begin
  //
  aQry := GetTmpADOQuery;
  with aQry do
    try
      SQL.Add('EXEC SP_GET_CURS_INFORMATION :TipValuta, :Data, :IdRep');
      Params.ParamByName('TipValuta').Value := FTipValuta;
      Params.ParamByName('Data').Value := FDataCurs;
      Params.ParamByName('IdRep').Value := FIdRep;
      Open;
      PopulateImage(aQry, edContract.Values, edContract.Descriptions, 'CURS_CONTRAT', 'NUME_CONTRACT');
      edFurnizor.Text := FieldByName('NUME_REPARTITOR').AsString;
      edTipValuta.Text := FieldByName('DENUMIRE_VALUTA').AsString;
      edTipValuta1.Text := FieldByName('DENUMIRE_VALUTA').AsString;
      edTipValuta2.Text := FieldByName('DENUMIRE_VALUTA').AsString;
      edDataNomenclator.Date := FieldByName('DATA_CURS').AsDateTime;
      edDataCursOnline.Date  := FieldByName('DATA_CURS').AsDateTime;
      FSynonim := FieldByName('ONLINE_SYNONIM').AsString;
    finally
      Free;
    end;
end;

procedure TfrmSelectCursValutar.btnCursOnlineClick(Sender: TObject);
begin
  //serviciul de adus online folosit la pitesti www.infovalutar.ro
  //tre verificat sa fie conexiune internet
  edValoareOnline.Value := GetValoareCurs(edDataCursOnline.Date, FSynonim);
end;

procedure TfrmSelectCursValutar.edTipValuta2Change(Sender: TObject);
begin
  FSynonim := edTipValuta2.Text;
end;

function TfrmSelectCursValutar.GetValutaValue: Currency;
begin
  if PageControl.ActivePage = tabCursNegociat then begin
    FValutaValue := edValoareValuta.Value;
  end else
  if PageControl.ActivePage = tabConfigurareCurs then begin
    FValutaValue := edValNomenclator.Value;
  end else
  if PageControl.ActivePage = tabCursOnline then begin
    FValutaValue := edValoareOnline.Value;
  end;
  Result := FValutaValue;
end;

end.
