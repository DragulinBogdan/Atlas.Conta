unit RapInclude;

interface

procedure LoadReport(aRepId: Integer); overload;
procedure LoadReport(aRepId: Integer; const AParamNames: String; const AParamValues: array of Variant); overload;

var
  IsFastReport : Boolean = True;

implementation

uses
  {RaportExplorer,}
  mainUnit;

procedure LoadReport(aRepId: Integer);
begin
  if IsFastReport then
    MainForm.DoShowReport(aRepId);
//  else  RaportExplorer.LoadReport(aRepId);
end;

procedure LoadReport(aRepId: Integer; const AParamNames: String; const AParamValues: array of Variant);
begin
  if IsFastReport then
    MainForm.DoShowReport(aRepId);
//    MainForm.DoShowReport(aRepId, AParamNames, AParamValues);
end;

end.
