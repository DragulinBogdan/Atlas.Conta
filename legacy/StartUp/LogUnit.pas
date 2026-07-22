unit LogUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, Buttons, StdCtrls, Gauges, jpeg, unit_AutoClientForm;

type
  TLogos = class(TCenterClientForm)
    Fundal: TImage;
    CancelBtn: TSpeedButton;
    Progress: TGauge;
    lbAppDescription: TLabel;
    lbStareCaption: TLabel;
    lbStareCurenta: TLabel;
    LbLicentaStr: TLabel;
    lbVersiune: TLabel;
    lbSvnVersion: TLabel;
    procedure CancelBtnClick(Sender: TObject);
  private
    FBitMap : TBitmap;
    FAppName: String;
    FModulName: String;
    FStare: String;
    procedure SetAppName(const Value: String);
    procedure SetModulName(const Value: String);
    procedure SetStare(const Value: String);
    { Private declarations }
  public
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;
  protected
    procedure WmEraseBackGrnd(var Message: TWMEraseBkgnd); message WM_ERASEBKGND;
    procedure WMNCHitTest(var Message: TWMNCHitTest); message WM_NCHITTESt;
  public
    property AppName : String read FAppName write SetAppName;
    property ModulName: String read FModulName write SetModulName;
    property Stare: String read FStare write SetStare;
    { Public declarations }
  end;

procedure ShowLogos;
procedure HideLogos;
function GetClientDBVersion: String;

var
  Logo : TLogos;

implementation

{$R *.DFM}

uses
  W32APIATLUnit,
  ZeosDBUtile,
  ZClasses,
  SVNRevision;

function GetClientDBVersion: String;
begin
  Result := ' Zeos DB : ' + ZEOS_VERSION;
end;

procedure ShowLogos;
begin
  Logo := TLogos.Create(Application);
  Logo.AppName := Application.Title;
  Logo.Show;
end;

procedure HideLogos;
begin
  if Assigned(Logo) then
    FreeAndNil(Logo);
end;

{ TLogos }

procedure TLogos.SetAppName(const Value: String);
begin
  FAppName := Value;
  Caption := Value + ' se incarca !';
end;

procedure TLogos.SetModulName(const Value: String);
begin
  FModulName := Value;
end;

procedure TLogos.CancelBtnClick(Sender: TObject);
begin
  if MessageDlg('Doriti Oprirea Aplicatiei ?',mtConfirmation,[mbYes,mbNo],0)=mrYes then begin
     bIsCanceling := True;
     Close;
  end;
end;

procedure TLogos.SetStare(const Value: String);
begin
  FStare := Value;
  lbStareCurenta.Caption := Value;
end;

procedure TLogos.WmEraseBackGrnd(var Message: TWMEraseBkgnd);
begin
  inherited;
  if not bIsParented and not Fundal.Visible then
    Message.Result := Ord(StretchBlt(Message.DC
                                     ,0, 0, Self.Width, Self.Height
                                     ,FBitMap.Canvas.Handle
                                     ,0, 0, FBitMap.Width, FBitMap.Height
                                     ,SRCCOPY));
end;

type
  TCrackGraphic = class(TGraphic);

constructor TLogos.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  if not bIsParented then begin
    FBitMap := TBitmap.Create;
    FBitMap.Width  := Fundal.Picture.Graphic.Width;
    FBitMap.Height := Fundal.Picture.Graphic.Height;
    TCrackGraphic(Fundal.Picture.Graphic).Draw(FBitMap.Canvas, Rect(0, 0, FBitMap.Width, FBitMap.Height));
    lbVersiune.Caption    := 'ATLAS   : ' + GetCurrentW32ModuleVersion + ' ' + gDBVersion;
    LbLicentaStr.Caption  := 'Licenta : ' + ValueToStr(GetRegKeyValueReadOnly('Software\Atlas\Licence\Licence'), False, 'nespecificata');
  end
  else begin
    Fundal.Visible              := False;
    CancelBtn.Visible           := False;
    lbAppDescription.Visible    := False;
    LbLicentaStr.Visible        := False;
    lbVersiune.Caption          := GetCurrentW32ModuleVersion + ' ' + gDBVersion;
    lbVersiune.Font.Color       := clHotLight;
    lbStareCaption.Font.Color   := clHotLight;
    lbStareCurenta.Font.Color   := clHotLight;
    lbSvnVersion.Font.Color     := clHotLight;
    Self.Color                  := clBtnFace;
  end;
  LBSVNVersion.Caption := Format(' SVN [%s/%s - %s]', [SVNCommitRevNo, SVNUpdateRevNo, SVNBuildDate]);
end;

destructor TLogos.Destroy;
begin
  if not bIsParented and Assigned(FBitMap) then FBitMap.Free;
  inherited Destroy;
end;

procedure TLogos.WMNCHitTest(var Message: TWMNCHitTest);
begin
  inherited;
  if not bIsParented and not (csDesigning in ComponentState) and (Message.Result = HTCLIENT) then
    Message.Result := HTCAPTION;
end;

end.
