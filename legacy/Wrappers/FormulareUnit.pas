unit FormulareUnit;

interface

uses Classes, Windows, Controls, Forms, cxPC;

type
  TFormProvider = class(TComponent)
  private
    FFormularList: TStringList;
    FTabControl: TcxTabControl;
    FClientArea: TWinControl;
    procedure SetTabControl(const Value: TcxTabControl);
  protected
    function GetFormByClassFromScreen(AClass: TFormClass): TCustomForm;
    function GetFormByClassFromTab(AClass: TFormClass): TCustomForm;
    function GetFormByClass(AClass: TFormClass): TCustomForm;
    function GetTabFromForm(AForm: TCustomForm): Integer;
    procedure RegisterTabForm(AForm: TCustomForm; ACaption: String = '');
    procedure HideAllForms(AForm: TCustomForm);
    procedure SelectTab(Sender: TObject);
    procedure CloseTab(Sender: TObject; ATabIndex: Integer; var ACanClose: Boolean);
  public
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;
    procedure UpdatePageVisible(TabCount: Integer = 0);
    function  GetNewForm        (AClass: TFormClass; ACaption: String = ''; ANewInstance: Boolean = False): TCustomForm;
    procedure SetNewForm        (AForm: TCustomForm);
    procedure RegisterFormular  (AClassDesc: String; AClass: TFormClass);
    procedure UnRegisterFormular(AClass: TFormClass);
    procedure Notification(AComponent: TComponent; AOperation: TOperation); override;
  public
    property  FormularList: TStringList read FFormularList;
    property  TabControl : TcxTabControl read FTabControl write SetTabControl;
    property  ClientArea: TWinControl read FClientArea write FClientArea;
  end;


function  FormProvider: TFormProvider;
function  FormularList: TStringList;
function  GetNewForm(AClass: TFormClass; ACaption : String = ''; ANewInstance: Boolean = False): TCustomForm;
procedure SetNewForm(AForm: TCustomForm);

procedure RegisterFormular(AClassDesc: String; AClass: TFormClass);
procedure UnRegisterFormular(AClass: TFormClass);


implementation

uses
  TypInfo, SysUtils;

var
  gFormProvider : TFormProvider = nil;

function FormProvider: TFormProvider;
begin
  if not Assigned(gFormProvider) then
     gFormProvider := TFormProvider.Create(Application);
  Result := gFormProvider;
end;

function  FormularList: TStringList;
begin
  Result := FormProvider.FormularList;
end;

function GetNewForm(AClass: TFormClass; ACaption : String = ''; ANewInstance: Boolean = False): TCustomForm;
begin
  Result := FormProvider.GetNewForm(AClass, ACaption, ANewInstance);
end;

procedure SetNewForm(AForm: TCustomForm);
begin
  FormProvider.SetNewForm(AForm);
end;

procedure RegisterFormular(AClassDesc: String; AClass: TFormClass);
begin
  FormProvider.RegisterFormular(AClassDesc, AClass);
end;

procedure UnRegisterFormular(AClass: TFormClass);
begin
  FormProvider.UnRegisterFormular(AClass);
end;

{ TFormProvider }

procedure TFormProvider.CloseTab(Sender: TObject; ATabIndex: Integer; var ACanClose: Boolean);
var
  lForm : TForm;
begin
  if not Assigned(TabControl) or not Assigned(TabControl.Tabs.Objects[ATabIndex]) then Exit;

  lForm := TForm(TabControl.Tabs.Objects[ATabIndex]);
  ACanClose := False;
  if Assigned(lForm) then begin
    if lForm.Tag = -9999 then begin
      if lForm.FormStyle = fsMdiChild then
        lForm.FormStyle := fsNormal;
      lForm.RemoveFreeNotification(Self);
      lForm.Hide;
      ACanClose := True;
    end
    else
    if lForm.Tag = -8888 then begin
      lForm.Close;
      lForm.RemoveFreeNotification(Self);
      ACanClose := True;
    end
    else begin
      ACanClose := true;
      if Assigned(lForm.OnCloseQuery) then
        lForm.OnCloseQuery(nil, ACanClose)
      else
        ACanClose := True; //daca nu este tratat evenimentul OnCloseQuery inchidem forma
      if ACanClose then  begin
        lForm.Close;
        lForm.RemoveFreeNotification(Self);
      end;
    end;
  end;
  if ACanClose = True then begin
    TabControl.Tabs.Objects[ATabIndex] := nil;
    UpdatePageVisible(1);
  end;
end;

constructor TFormProvider.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FFormularList := TStringList.Create;  
end;

destructor TFormProvider.Destroy;
begin
  FFormularList.Free;
  inherited Destroy;
end;

function TFormProvider.GetTabFromForm(AForm: TCustomForm): Integer;
var
  I: Integer;
begin
  Result := -1;
  if Assigned(TabControl) then
    for I := 0 to TabControl.Tabs.Count-1 do begin
      if (TabControl.Tabs.Objects[I] = AForm) then begin
         Result := I;
         Break;
      end;
    end;
end;

function TFormProvider.GetFormByClass(AClass: TFormClass): TCustomForm;
begin
  if FTabControl <> nil then
    Result := GetFormByClassFromTab(AClass)
  else
    Result := GetFormByClassFromScreen(AClass);
end;

function TFormProvider.GetFormByClassFromScreen(
  AClass: TFormClass): TCustomForm;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to Screen.FormCount-1 do begin
    if Screen.Forms[I].InheritsFrom(AClass) then begin
      Result := Screen.Forms[I];
      Break;
    end;
  end;
end;

function TFormProvider.GetFormByClassFromTab(AClass: TFormClass): TCustomForm;
type
  TBooleanFunc = function (): Boolean;
var
  I: Integer;

    function IsMultiInstance(AForm: TCustomForm): Boolean;
    var
      lPropInfo: PPropInfo;
      lMethod  : TMethod;
    begin
      lPropInfo := GetPropInfo(AForm, 'IsMultiInstance', [tkMethod]);
      Result := Assigned(lPropInfo);
      if Result then begin
        lMethod := GetMethodProp(AForm, lPropInfo);
        Result := TBooleanFunc(lMethod.Code)();
      end;
    end;

begin
  Result := nil;
  if Assigned(TabControl) then
    for I := 0 to TabControl.Tabs.Count-1 do begin
      Result := TCustomForm(TabControl.Tabs.Objects[I]);
      if Result.InheritsFrom(AClass) and not IsMultiInstance(Result) then
         Break
      else
         Result := nil;
    end;
end;

function TFormProvider.GetNewForm(AClass: TFormClass; ACaption: String = ''; ANewInstance: Boolean = False): TCustomForm;
var
  lTab: Integer;
begin
  if not ANewInstance then Result := GetFormByClass(AClass)
  else Result := nil;
  if ANewInstance or not Assigned(Result) then begin
    Result := AClass.Create(Application);
    RegisterTabForm(Result, ACaption);
  end
  else
  if TabControl <> nil then begin
    lTab := GetTabFromForm(Result);
    if lTab > -1 then
      TabControl.TabIndex := lTab;
  end;
end;

procedure TFormProvider.HideAllForms(AForm: TCustomForm);
var
  I: Integer;
  lForm: TForm;
begin
  for I := 0 to TabControl.Tabs.Count-1 do begin
    lForm := TForm(TabControl.Tabs.Objects[I]);
    if Assigned(lForm) and (lForm <> AForm) and lForm.Visible then begin
      lForm.Hide;
    end;
  end;
end;

procedure TFormProvider.Notification(AComponent: TComponent;
  AOperation: TOperation);
var
  lPos : Integer;
begin
  inherited Notification(AComponent, AOperation);
  if (AOperation = opRemove) and (AComponent is TCustomForm) then begin
    lPos := GetTabFromForm(TCustomForm(AComponent));
    while lPos > -1 do begin
      AComponent.RemoveFreeNotification(Self);
      { Sa evitam distrugerea de 2 ori }
      TabControl.Tabs.Objects[lPos] := nil;
      TabControl.Tabs.Delete(lPos);
      lPos := GetTabFromForm(TCustomForm(AComponent));
    end;
  end;
end;

procedure TFormProvider.RegisterFormular(AClassDesc: String;
  AClass: TFormClass);
begin
  if FFormularList.IndexOfObject(TObject(AClass)) = -1 then
     FFormularList.AddObject(AClassDesc, TObject(AClass));
end;

procedure TFormProvider.RegisterTabForm(AForm: TCustomForm; ACaption: String = '');
var
  lTabIndex : Integer;
  lCaption  : String;
begin
  if ACaption > '' then
    lCaption := ACaption
  else
    lCaption := AForm.Caption;
  if Assigned(TabControl) then begin
    lTabIndex           := TabControl.Tabs.AddObject(lCaption, AForm);
    AForm.Parent        := FClientArea;
    AForm.BorderStyle   := bsNone;
    AForm.WindowState   := wsMaximized;
    AForm.Align         := alClient;
    TabControl.TabIndex := lTabIndex;
    UpdatePageVisible(1);
  end;
  AForm.FreeNotification(Self);  
end;

procedure TFormProvider.SelectTab(Sender: TObject);
var
  lForm: TCustomForm;
begin
  if Assigned(FTabControl)
    and (FTabControl.TabIndex >= 0)
    and Assigned(FTabControl.Tabs.Objects[FTabControl.TabIndex])
 then begin
    lForm := TCustomForm(FTabControl.Tabs.Objects[FTabControl.TabIndex]);
    if Assigned(lForm) then begin
      HideAllForms(lForm);
      if not lForm.Visible then
        lForm.Show;
    end;
  end;
end;

procedure TFormProvider.SetNewForm(AForm: TCustomForm);
begin
  RegisterTabForm(AForm);
end;

procedure TFormProvider.SetTabControl(const Value: TcxTabControl);
begin
  FTabControl := Value;
  if Assigned(FTabControl) then begin
     FTabControl.OnCanCloseEx  := CloseTab;
     FTabControl.OnChange      := SelectTab;
  end;
end;

procedure TFormProvider.UnRegisterFormular(AClass: TFormClass);
var
  lIndex: Integer;
begin
  lIndex := FormularList.IndexOfObject(TObject(AClass));
  if lIndex > -1 then
     FormularList.Delete(lIndex);
end;

procedure TFormProvider.UpdatePageVisible(TabCount: Integer = 0);
{
var
  lVisible: Boolean;
}
begin
  {
  lVisible := TabControl.Tabs.Count > TabCount;
  if FClientArea.Visible <> lVisible then
    FClientArea.Visible := lVisible;
  if FTabControl.Visible <> lVisible then
    FTabControl.Visible := lVisible;
  }

  SelectTab(TabControl);
end;

end.
