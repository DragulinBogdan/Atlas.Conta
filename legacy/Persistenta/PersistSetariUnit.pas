unit PersistSetariUnit;

interface

uses
  Classes;

procedure SalveazaSetari(AObject: TComponent; const ASectionName: string); overload;
procedure SalveazaSetari(AObject: TComponent; const ASectionName: string; AIgnoreClass : array of String; AIgnoreNames: array of String); overload;
procedure IncarcaSetari(AObject: TComponent; const ASectionName: string); overload;
procedure IncarcaSetari(AObject: TComponent; const ASectionName: string; AIgnoreClass : array of String; AIgnoreNames: array of String); overload;

implementation

uses
  CommonDBVar,
  Forms,
  SysUtils;

var
  gStoreFileName : String;

function StoreFileName : String;
begin
  if Trim(gStoreFileName) = '' then
     gStoreFileName := ChangeFileExt(GetAppFileName(), '.set');
  Result := gStoreFileName;
end;

procedure SalveazaSetari(AObject: TComponent; const ASectionName: string; AIgnoreClass : array of String; AIgnoreNames: array of String);
begin
end;

procedure SalveazaSetari(AObject: TComponent; const ASectionName: string);
begin
  SalveazaSetari(AObject, ASectionName, [], []);
end;

procedure IncarcaSetari(AObject: TComponent; const ASectionName: string; AIgnoreClass : array of String; AIgnoreNames: array of String);
begin
end;

procedure IncarcaSetari(AObject: TComponent; const ASectionName: string); overload;
begin
  IncarcaSetari(AObject, ASectionName, [], []);
end;

end.
