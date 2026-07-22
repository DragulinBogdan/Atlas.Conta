unit LogoUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, jpeg, StdCtrls, Vcl.ComCtrls, unit_AutoClientForm;

type
  TfrmLogo = class(TCenterClientForm)
    BackGround: TImage;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    LBCurrent: TLabel;
    Progress: TProgressBar;
    edCurrentObj: TLabel;
    lbVesiune: TLabel;
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

function GetLogosForm(isHidden : Boolean = False): TfrmLogo;
procedure HideLogos(afrmLogo: TfrmLogo); overload;
procedure HideLogos; overload;

var
  lLogos : TFrmLogo;

implementation


{$R *.DFM}

uses
  CommonDBVar;

function GetLogosForm(isHidden : Boolean = False): TfrmLogo;  
begin
  if lLogos = nil then lLogos := TfrmLogo.Create(nil);
  Result := lLogos;
  Result.lbVesiune.Caption  := ExeVersion;
  if isHidden then begin
    Result.BackGround.Free;
    Result.Label1.Free;
    Result.Label2.Free;
    Result.Label3.Free;
    Result.LBCurrent.Free;
    Result.lbVesiune.Free;
    Result.Color    := clSilver;
    Result.Enabled  := False;
  end;
  Result.Show;
end;

procedure HideLogos;
begin
  HideLogos(lLogos);
end;

procedure HideLogos(afrmLogo: TfrmLogo);
begin
  if Assigned(afrmLogo) then begin
    FreeAndNil(afrmLogo);
    lLogos := nil;
  end;
end;

procedure TfrmLogo.FormCreate(Sender: TObject);
begin
  LBCurrent.Caption := FormatDateTime('dddd-dd-mmmm-yyyy', Now);
end;

end.
