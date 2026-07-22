unit AnexeCopy;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, StdCtrls, cxControls, cxContainer, cxEdit,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox, 
  ExtCtrls, DegradePanel, Menus, cxLookAndFeelPainters, cxButtons,
  cxCheckBox, DB, ZDataSet,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels;

type
  TfrmAnexaCopy = class(TForm)
    pnTop: TDegradePanel;
    Label1: TLabel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    ckRanduri: TcxCheckBox;
    ckColoane: TcxCheckBox;
    ckFormule: TcxCheckBox;
    qryLstAnexe: TZQuery;
    edAnexa: TcxImageComboBox;
    procedure ckRanduriClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    function GetBifaColoane: Boolean;
    function GetBifaFormule: Boolean;
    function GetBifaRanduri: Boolean;
    function GetIdAnexeBilant: Integer;
    { Private declarations }
  public
    { Public declarations }
    property IdAnexeBilant : Integer read GetIdAnexeBilant;
    property BifaRanduri : Boolean read GetBifaRanduri;
    property BifaColoane : Boolean read GetBifaColoane;
    property BifaFormule : Boolean read GetBifaFormule;
  end;


implementation

uses
  dxCompsUtile, ZeosDBUtile;

{$R *.dfm}

procedure TfrmAnexaCopy.ckRanduriClick(Sender: TObject);
begin
  if ckFormule.Checked then
     ckFormule.Checked := ckRanduri.Checked and ckColoane.Checked;
end;

procedure TfrmAnexaCopy.FormCreate(Sender: TObject);
begin
  DBRefresh(qryLstAnexe);
  FillImageCombo(edAnexa.Properties, qryLstAnexe, 'ID_ANEXE_BILANT', 'DENUMIRE');
  if not qryLstAnexe.IsEmpty then
    edAnexa.EditValue := qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
end;

function TfrmAnexaCopy.GetBifaColoane: Boolean;
begin
  Result := ckColoane.Checked;
end;

function TfrmAnexaCopy.GetBifaFormule: Boolean;
begin
  Result := ckFormule.Checked;
end;

function TfrmAnexaCopy.GetBifaRanduri: Boolean;
begin
  Result := ckRanduri.Checked;
end;

function TfrmAnexaCopy.GetIdAnexeBilant: Integer;
begin
  Result := edAnexa.EditValue;
end;

end.
