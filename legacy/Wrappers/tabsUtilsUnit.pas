unit tabsUtilsUnit;

interface

uses
  Controls, Forms, rkSmartTabs;

type
  TChromeProvider = class
  private
    class var FTabs   : TrkSmartTabs;
    class var FClient : TWinControl;
    class var FActiveForm: TCustomForm;
    class procedure SetActiveForm(const Value: TCustomForm); static;
    class function GetFormCount: Integer; static;
    class function GetForms(Index: Integer): TCustomForm; static;
  protected
    class function TabFromForm(AForm: TCustomForm): Integer;
    class procedure DoSelectTab(Sender: TObject);
    class procedure DoCloseTab(Sender: TObject; Index: Integer; var Close: Boolean);
  public
    class function SelectTabFromForm(AForm: TCustomForm): Integer;
  public
    class property ActiveForm: TCustomForm read FActiveForm write SetActiveForm;
    class property FormCount: Integer read GetFormCount;
    class property Forms[Index: Integer]: TCustomForm read GetForms;
    class property Tabs: TrkSmartTabs read FTabs write FTabs;
    class property Client: TWinControl read FClient write FClient;
  end;

implementation

{ TChromeProvider }

class procedure TChromeProvider.DoCloseTab(Sender: TObject; Index: Integer; var Close: Boolean);
begin
  Forms[Index].Free;
  Close := True;
end;

class procedure TChromeProvider.DoSelectTab(Sender: TObject);
begin
  ActiveForm := Forms[FTabs.ActiveTab];
end;

class function TChromeProvider.GetFormCount: Integer;
begin
  Result := FTabs.Tabs.Count;
end;

class function TChromeProvider.GetForms(Index: Integer): TCustomForm;
begin
  Result := TCustomForm(FTabs.Tabs.Objects[Index]);
end;

class function TChromeProvider.SelectTabFromForm(AForm: TCustomForm): Integer;
begin
  Result := TabFromForm(AForm);
  if Result = -1 then
    // In momentul in care adaugam un tab nou se apeleaza onChange si acolo facem afisarea
    FTabs.AddObject(AForm.Caption, AForm)
  else
    // Apeleaza evenimentul de onChange
    FTabs.ActiveTab := Result;
end;

class procedure TChromeProvider.SetActiveForm(const Value: TCustomForm);
var
  I: Integer;
begin
  FActiveForm := Value;
  for I := 0 to FormCount-1 do
    Forms[I].Visible := Forms[I] = FActiveForm;
end;

class function TChromeProvider.TabFromForm(AForm: TCustomForm): Integer;
var
  I: Integer;
begin
  Result := -1;
  for I := 0 to FTabs.Tabs.Count-1 do
    if FTabs.Tabs.Objects[I] = AForm then begin
      Result := I;
      Break;
    end;
end;

end.
