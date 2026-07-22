unit SaveToLocalUnit;

interface
uses Controls, Classes;

type
  TSaveRec = record
    StartInterval : TDate;
    EndInterval   : TDate;
    CurentHouse   : Integer;
    DataSold      : TDateTime;
    SoldInitial   : Currency;
    RegSize       : Integer;
    RegStream     : array of Byte;

  end;


implementation

end.
