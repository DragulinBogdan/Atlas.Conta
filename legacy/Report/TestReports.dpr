program TestReports;

uses
  ExceptionLog,
  Forms,
  ReportExplorer in 'ReportExplorer.pas' {fmRepExplorer},
  CustomReport in 'CustomReport.pas' {Reports: TDataModule};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TReports, Reports);
  Reports.Explore;
  Application.Run;
end.
