unit UnitPerioade;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, HeadPanel, StdCtrls, Buttons, dxExEdtr, dxEdLib,  dxCntner,
  dxEditor;

type
  TfrmSelectPeriod = class(TForm)
    pnTop: THeadPanel;
    pnBottom: TPanel;
    pnRest: TPanel;
    BitBtn1: TSpeedButton;
    BitBtn2: TSpeedButton;
    pnTimeSaptamana: TPanel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    edSaptamana: TdxImageEdit;
    edLunaAn: TdxImageEdit;
    rbSaptamana: TRadioButton;
    pnTimeZiua: TPanel;
    edZi: TdxDateEdit;
    rbZiua: TRadioButton;
    pnTimeLunaAn: TPanel;
    edLuna: TdxImageEdit;
    rbLuna: TRadioButton;
    pnTimeAnul: TPanel;
    edAn: TdxImageEdit;
    rbAnul: TRadioButton;
    pnTimePeriod: TPanel;
    edNrZile: TdxSpinEdit;
    rb_Zile: TRadioButton;
    rb_Sapt: TRadioButton;
    rb_Luni: TRadioButton;
    rb_Ani: TRadioButton;
    rbLast: TRadioButton;
    pnTimeDelaLa: TPanel;
    Label2: TLabel;
    Label3: TLabel;
    edDataDeLa: TdxDateEdit;
    edDataLa: TdxDateEdit;
    rbDelaLa: TRadioButton;
    StyleController: TdxEditStyleController;
    procedure BitBtn1Click(Sender: TObject);
    procedure BitBtn2Click(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmSelectPeriod: TfrmSelectPeriod;

implementation

{$R *.DFM}

procedure TfrmSelectPeriod.BitBtn1Click(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TfrmSelectPeriod.BitBtn2Click(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

end.
