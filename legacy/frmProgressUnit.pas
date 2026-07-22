unit frmProgressUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxControls, cxContainer, cxEdit, cxLabel, cxProgressBar, ExtCtrls,
  cxGraphics, cxLookAndFeelPainters, cxLookAndFeels, Vcl.Menus, Vcl.StdCtrls,
  cxButtons, frxClass;

const
  WM_START_BUTTON = WM_USER + 1;

type
  TLoadReport = reference to procedure(Index: Integer; AReport: TfrxReport);
  PLoadReport = ^TLoadReport;
  TfrmProgressRap = class(TForm)
    lbInfo      : TcxLabel;
    progressBar: TcxProgressBar;
    btnCancel: TcxButton;
    procedure btnCancelClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
  private
    FCanceled: Boolean;
    { Private declarations }
    procedure HideWindow;
    procedure StartButton(var AMessage: TMessage); message WM_START_BUTTON;
  protected
    FReportID: Integer;
    FTotalReports: Integer;
    FLoader : TLoadReport;
  public
    { Public declarations }
    property Canceled: Boolean read FCanceled;
  end;


procedure ShowReportList(ACaption: String; AReportID: Integer; ATotalItems: Integer; ALoader: TLoadReport);

implementation

{$R *.dfm}

uses mainUnit;

procedure ShowReportList(ACaption: String; AReportID: Integer; ATotalItems: Integer; ALoader: TLoadReport);
var
  lForm: TfrmProgressRap;
begin
  lForm := TfrmProgressRap.Create(nil);
  try
    lForm.Caption := ACaption;
    lForm.FReportID := AReportID;
    lForm.FTotalReports := ATotalItems;
    lForm.FLoader := ALoader;
    lForm.ShowModal;
  finally
    lForm.Free;
  end;
end;

{ TfrmProgress }

procedure TfrmProgressRap.btnCancelClick(Sender: TObject);
begin
  if MessageDlg('Doriti intreruperea generarii raportului ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    FCanceled := True;
end;

procedure TfrmProgressRap.FormShow(Sender: TObject);
begin
  PostMessage(Self.Handle, WM_START_BUTTON, 0, 0);
end;

procedure TfrmProgressRap.HideWindow;
begin
  ModalResult := mrOk;
  Hide;
  Application.ProcessMessages;
end;

procedure TfrmProgressRap.StartButton(var AMessage: TMessage);
var
  I: Integer;
  lReport: TfrxReport;
begin
  progressBar.Properties.Min := 0;
  progressBar.Properties.Max := FTotalReports - 1;
  lReport := mainForm.FRrapExplorer.LoadReport(FReportId, False);
  try
    lReport.OnPreview := nil;
    lReport.OnClosePreview := nil;
    FCanceled := False;
    for I := 0 to FTotalReports - 1 do begin
      if FCanceled then
        Break;
      progressBar.EditValue := I;
      Application.ProcessMessages;
      FLoader(I, lReport);
      lReport.PrepareReport(False);
    end;
    if not FCanceled then begin
      HideWindow;
      lReport.Preview := nil;
      lReport.PreviewOptions.Modal := True;
      lReport.ShowPreparedReport;
    end;
  finally
    lReport.Free;
  end;
end;

end.
