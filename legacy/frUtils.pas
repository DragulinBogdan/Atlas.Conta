unit frUtils;

interface
uses frxClass, frxADOComponents;

const
  ctCommandTimeout = 30; //500;

procedure SetReportTimeOut(AReport: TfrxReport; ATimeOut: Integer);

implementation

//------------------------------------------------------------------------------
procedure SetReportTimeOut(AReport: TfrxReport; ATimeOut: Integer);
var i: Integer;
begin
  for i := 0 to AReport.DataSets.Count - 1 do
    if AReport.DataSets.Items[i].DataSet is TfrxADOQuery then
      TfrxADOQuery(AReport.DataSets.Items[i].DataSet).CommandTimeout := ATimeOut;
end;
//------------------------------------------------------------------------------

end.
 
