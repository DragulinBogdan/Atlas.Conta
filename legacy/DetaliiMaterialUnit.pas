unit DetaliiMaterialUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls, dxCntner,
  dxEditor, dxEdLib, dxExEdtr, Menus, DegradePanel,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmSetareDetaliiNomenclator = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    btnCancel: TcxButton;
    pnContent: TPanel;
    Label1: TLabel;
    Bevel1: TBevel;
    Label2: TLabel;
    Label3: TLabel;
    edtTipMaterial: TdxEdit;
    EditStyle: TdxEditStyleController;
    edtDenumireMaterial: TdxEdit;
    edtUnitateMasura: TdxEdit;
    edtPretFaraTva: TdxCurrencyEdit;
    Label4: TLabel;
    Label5: TLabel;
    edtCotaTVA: TdxCurrencyEdit;
    Label6: TLabel;
    edtPretCuTVA: TdxCurrencyEdit;
    pnTop: TDegradePanel;
  private
    { Private declarations }
  public
    { Public declarations }
  end;


function AdministrareMaterial(var TipMaterial, DenumireMaterial, UM : String; PretFaraTva, Cota_TVA, PretCuTVA : Currency) : Boolean;


implementation

{$R *.dfm}

function AdministrareMaterial(var TipMaterial, DenumireMaterial, UM : String; PretFaraTva, Cota_TVA, PretCuTVA : Currency) : Boolean;
begin
  with TfrmSetareDetaliiNomenclator.Create(nil) do
    try
      edtTipMaterial.Text := TipMaterial;
      edtDenumireMaterial.Text := DenumireMaterial;
      edtUnitateMasura.Text := UM;
      edtPretFaraTva.Value := PretFaraTva;
      edtCotaTVA.Value := Cota_TVA;
      edtPretCuTVA.Value := PretCuTVA;
      ShowModal;
      Result := (ModalResult = mrOK);
      if Result then begin
        TipMaterial := edtTipMaterial.Text;
        DenumireMaterial := edtDenumireMaterial.Text;
        UM := edtUnitateMasura.Text;
      end;
    finally
      Free;
    end;
end;


end.
