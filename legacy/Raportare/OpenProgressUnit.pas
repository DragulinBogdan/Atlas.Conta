unit OpenProgressUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  DB, StdCtrls, Buttons, ComCtrls, ExtCtrls, Menus, cxLookAndFeelPainters,
  cxButtons, cxGraphics, cxLookAndFeels;

type
  TFrmOpenProgress = class(TForm)
    ProgressTotal: TProgressBar;
    Animatie: TAnimate;
    ProgressPartial: TProgressBar;
    LbCurrent: TLabel;
    LbTotal: TLabel;
    RefreshTime: TTimer;
    BtnCancel: TcxButton;
    procedure BtnCancelClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure RefreshTimeTimer(Sender: TObject);
  private
    FActive: Boolean;
    FNrFaze: Integer;
    Switch : Boolean;
    procedure SetActive(const Value: Boolean);
    procedure SetNrFaze(const Value: Integer);
    { Private declarations }
  public
    IsCanceled: Boolean;
    DataStart : TDateTime;
    FCurentLabel : String;
    CurentThread : TThread;
    procedure SetTotalCount(Value: Integer);
    procedure SetPartialCount(Value: Integer);
    procedure SetPartialPos(Value: Integer; Desc: String);
    procedure SetTotalPos(Value: Integer; Desc: String);
    procedure StepIt;
    property  Active: Boolean read FActive write SetActive;
    property  NrFaze : Integer read FNrFaze write SetNrFaze;
    { Public declarations }
  end;

implementation

{$R *.DFM}

{ TFrmOpenProgress }

procedure TFrmOpenProgress.SetPartialCount(Value: Integer);
begin
  ProgressPartial.Min := 0;
  ProgressPartial.Max := Value;
end;

procedure TFrmOpenProgress.SetPartialPos(Value: Integer; Desc: String);
begin
  ProgressPartial.Position := Value;
  LbCurrent.Caption := Desc;
end;

procedure TFrmOpenProgress.SetTotalCount(Value: Integer);
begin
  ProgressTotal.Min := 0;
  ProgressTotal.Max := Value;
  Application.ProcessMessages;
end;

procedure TFrmOpenProgress.SetTotalPos(Value: Integer; Desc: String);
begin
  FCurentLabel := Desc;
  Switch       := True;
  NrFaze       := 1;
  ProgressTotal.Position := Value;
  Application.ProcessMessages;
end;

procedure TFrmOpenProgress.StepIt;
begin
  Application.ProcessMessages;
end;

procedure TFrmOpenProgress.BtnCancelClick(Sender: TObject);
begin
  with CreateMessageDialog('Doriti abandonul executiei raportului?', mtConfirmation, [mbYes, mbNo]) do
    try
      FormStyle := fsStayOnTop;
      Position := poScreenCenter;
      if ShowModal = mrOk then begin
         CurentThread.Suspend;
         IsCanceled := True;
      end;
    finally
      Free;
    end;
end;

procedure TFrmOpenProgress.FormCreate(Sender: TObject);
begin
  IsCanceled := False;
end;

procedure TFrmOpenProgress.SetActive(const Value: Boolean);
begin
  FActive := Value;
  RefreshTime.Enabled := Value;
  Animatie.Active     := Value;
  ProgressPartial.Position := 0;
end;

procedure TFrmOpenProgress.RefreshTimeTimer(Sender: TObject);
begin
  { Folosita in momentul executarii }
  LbCurrent.Caption := 'Pornit : '+TimeToStr(DataStart)+' Trecut : '+TimeToStr(Time - DataStart);
  ProgressPartial.Position := (ProgressPartial.Position + 1) mod ProgressPartial.Max;
  if (ProgressPartial.Position <10) and (not Switch) then begin
     Switch := True;
     NrFaze := NrFaze + 1;
  end;
  if (ProgressPartial.Position > 90) and (Switch) then Switch := False;
end;

procedure TFrmOpenProgress.SetNrFaze(const Value: Integer);
begin
  FNrFaze := Value;
  LbTotal.Caption := FCurentLabel+' (Faza : '+IntToStr(Value)+')';
end;

end.
