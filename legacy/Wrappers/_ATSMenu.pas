unit ATSMenu;

interface

uses
  SysUtils, Menus, Windows, Messages, Classes, Graphics, Controls, Forms, Dialogs,
  MethodProvider, DB;

type

  TATSCmdType = (ctBar, ctPopup, ctWindowList, ctReportList, ctAction, ctMainPopup, ctRaport, ctReportListFast, ctRaportFast);

  TATSCommand = class;
  TATSAppCommands = class;

  TATSOnExecuteCmd = procedure(Sender: TObject; Command: TATSCommand; var Allow: Boolean) of Object;
  TATSOnAddCommand = procedure(Sender: TObject; Command: TATSCommand) of Object;

  TATSCommandList = class(TList)
  private
    FParent: TATSCommand;
    function GetCommands(Index: Integer): TATSCommand;
  public
    constructor Create(AParent: TATSCommand); virtual;
    procedure Clear; override;
    destructor Destroy; override;
    property Commands[Index: Integer]: TATSCommand read GetCommands;
    property Parent: TATSCommand read FParent;
  end;

  TATSCommand = class(TObject)
  private
    FTag: Integer;
    FCommands: TATSCommandList;
    FCaption: String;
    FHint: String;
    FAction: String;
    FCmdType: TATSCmdType;
    FParent: TATSCommand;
    FValue: Variant;
    FCategory: String;
    FVisible: Boolean;
    FEnabled: Boolean;
    FShortCut: TShortCut;
    FOnExecute: TATSOnExecuteCmd;
    function GetComandCount: Integer;
    function GetCommands(Index: Integer): TATSCommand;
  protected
    procedure AddCommand(ACommand: TATSCommand);
    procedure Assign(ACommand: TATSCommand);
  public
    constructor Create(AParent: TATSCommand); virtual;
    destructor Destroy; override;

    function  GetCommandByName(const CommandName: String): TATSCommand;
    function  Execute: Boolean; virtual;

    procedure LoadFromCmds(ACmds: TATSAppCommands);
    procedure LoadFromMenu(AItem: TMenuItem; AEvent: TATSOnExecuteCmd = nil);
    procedure ClearCurentCmds;
    property Action : String read FAction write FAction;
    property Caption: String read FCaption write FCaption;
    property Category: String read FCategory write FCategory;
    property Commands[Index: Integer]: TATSCommand read GetCommands; default;
    property CommandCount: Integer read GetComandCount;
    property Parent : TATSCommand read FParent;
    property Hint   : String read FHint write FHint;
    property Tag    : Integer read FTag write FTag;
    property Value  : Variant read FValue write FValue;
    property ShortCut: TShortCut read FShortCut write FShortCut;
    property CmdType: TATSCmdType read FCmdType write FCmdType;
    property Visible: Boolean read FVisible write FVisible;
    property Enabled: Boolean read FEnabled write FEnabled;
    property OnExecute: TATSOnExecuteCmd read FOnExecute write FOnExecute;
  end;

  TATSAppCommands = class(TComponent)
  private
    FCommands: TList;
    FActive: Boolean;
    FReadedActive: Boolean;
    FKeyField: String;
    FHintField: String;
    FCaptionField: String;
    FParentField: String;
    FCmdField: String;
    FOnExecute: TATSOnExecuteCmd;
    FAfterOpen: TNotifyEvent;
    FBeforeOpen: TNotifyEvent;
    FRootKey: Variant;
    FTypeField: String;
    FOnNewCommand: TATSOnAddCommand;
    FCategoryField: String;
    FMethods: TMethodProvider;
    FShortCutField: String;
    procedure SetActive(const Value: Boolean);
    procedure SetCaptionField(const Value: String);
    procedure SetKeyField(const Value: String);
    procedure SetParentField(const Value: String);
    function GetCommand(Index: Integer): TATSCommand;
    function GetCommandCount: Integer;

    procedure DoInternalLoad;
    procedure DoInternalClear;
  protected

    FDataSet: TDataSet;
    procedure SetDataSet(DataSet: TDataSet); virtual;
    procedure InternalExecute(Command: TATSCommand); virtual;

    function GetEnabled(ACommand: TATSCommand): Boolean; virtual;

  public

    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;

    function  GetCommandByName(const CommandName: String): TATSCommand;
    procedure Execute(Command: TATSCommand); overload;
    procedure Execute(Command: String); overload;

    procedure Notification(AComponent: TComponent; AOperation: TOperation); override;
    procedure Loaded; override;

    property Commands[Index: Integer] : TATSCommand read GetCommand; default;
    property CommandCount: Integer read GetCommandCount;
    property DataSet  : TDataSet read FDataSet write SetDataSet;

  published

    property Active: Boolean read FActive write SetActive;
    property KeyField: String read FKeyField write SetKeyField;
    property ParentField: String read FParentField write SetParentField;
    property CaptionField: String read FCaptionField write SetCaptionField;
    property CategoryField: String read FCategoryField write FCategoryField;
    property HintField: String read FHintField write FHintField;
    property ShortCutField: String read FShortCutField write FShortCutField; 
    property CmdField : String read FCmdField write FCmdField;
    property TypeField: String read FTypeField write FTypeField;
    property Methods: TMethodProvider read FMethods write FMethods;
    property RootKey  : Variant read FRootKey write FRootKey;
    property BeforeOpen: TNotifyEvent read FBeforeOpen write FBeforeOpen;
    property AfterOpen : TNotifyEvent read FAfterOpen write FAfterOpen;
    property OnExecute : TATSOnExecuteCmd read FOnExecute write FOnExecute;
    property OnNewCommand: TATSOnAddCommand read FOnNewCommand write FOnNewCommand;
  end;

  TATSDBAppCommands = class(TATSAppCommands)
  published
    property DataSet;
  end;

implementation

uses ShellApi, Variants;

{ TATSCommandList }

procedure TATSCommandList.Clear;
var I : Integer;
begin
  for I := 0 to Count-1 do TAtsCommand(Items[I]).Free;
  inherited Clear;
end;

constructor TATSCommandList.Create(AParent: TATSCommand);
begin
  inherited Create;
  FParent := AParent;
end;

destructor TATSCommandList.Destroy;
begin
  inherited Destroy;
end;

function TATSCommandList.GetCommands(Index: Integer): TATSCommand;
begin
  Result := TATSCommand(inherited Items[Index]);
end;

{ TATSAppCommands }

constructor TATSAppCommands.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FCommands := TList.Create; 
end;

destructor TATSAppCommands.Destroy;
begin
  DoInternalClear;
  FCommands.Free;
  inherited Destroy;
end;

procedure TATSAppCommands.Execute(Command: TATSCommand);
var Allow: Boolean;
begin
  Allow := True;
  if Assigned(FOnExecute) then
     FOnExecute(Self, Command, Allow);
  if Allow then InternalExecute(Command);
end;

procedure TATSAppCommands.DoInternalClear;
var I: Integer;
begin
  with FCommands do begin
    for I := 0 to Count-1 do TATSCommand(Items[I]).Free;
    Clear;
  end;
  FActive := False;
end;

procedure TATSAppCommands.DoInternalLoad;
var
  { Pentru a nu risca sa ciclam }
  FOldDataSetActive : Boolean;
  lValList : TStringList; // Pentru a evita sa parcurgem recursiv si pozitiile cu revenire
  lKey,
  lParent,
  lCategory,
  lShortCut,
  lCaption,
  lHint,
  lType,
  lCmd : TField;

  procedure InitContructor(RootComp: TATSCommand; Root: Variant);
    var LastPoz: TBookMark;
        lCommand: TATSCommand;
   begin
    if (FDataSet.Locate(FParentField, Root, [])) and
       (lValList.IndexOf(VarToStr(Root)) = -1) then begin
       lValList.Add(VarToStr(Root));
       while (not FDataSet.Eof) and (lParent.Value = Root) do begin
         { Marcam Parintele ca fiind de tipul Popup }
//         if RootComp <> nil then RootComp.CmdType := ctPopup;
         lCommand := TATSCommand.Create(RootComp);
         with lCommand do begin
           Value := lKey.Value;
           if Assigned(lCaption) then Caption := lCaption.AsString;
           if Assigned(lHint) then Hint := lHint.AsString;
           if Assigned(lCmd)  then Action := lCmd.AsString;
           if Assigned(lType) then CmdType := TATSCmdType(lType.AsInteger);
           if Assigned(lCategory) then Category := lCategory.AsString;
           if Assigned(lShortCut) then ShortCut := TextToShortCut(lShortCut.AsString);
         end;
         if RootComp = nil then FCommands.Add(lCommand);
         { In Cazul in care are copii ii citim aici }
         LastPoz := FDataSet.GetBookmark;
         try
            InitContructor(lCommand, lCommand.Value);
         finally
            FDataSet.GotoBookmark(LastPoz);
            FDataSet.FreeBookmark(LastPoz);
         end;
         if Assigned(FOnNewCommand) then
            FOnNewCommand(Self, lCommand);
         FDataSet.Next;
       end;
    end;
   end;

begin
  if Assigned(FBeforeOpen) then FBeforeOpen(Self);
  FOldDataSetActive := FDataSet.Active;
  { Daca nu este deschis DataSet-ul il Deschidem }
  if not FDataSet.Active then FDataSet.Active := True;
  { Citim Inregistrarile }
  { Presupunem ca sunt ordonate dupa ParentField }
  if not Assigned(FDataSet) then
     raise Exception.Create('Nu ati specificat DataSet-ul. Operatia nu poate continua !');
  lKey := FDataSet.FindField(FKeyField);
  if not Assigned(lKey) then
     raise Exception.Create('Nu s-a gasit campul asociat cheii : '+FKeyField+'. Operatia nu poate continua !');
  lParent := FDataSet.FindField(FParentField);
  if not Assigned(lParent) then
     raise Exception.Create('Nu s-a gasit campul asociat parintelui : '+FParentField+'. Operatia nu poate continua !');
  lShortCut:= FDataSet.FindField(FShortCutField);
  lCaption := FDataSet.FindField(FCaptionField);
  lHint    := FDataSet.FindField(FHintField);
  lCmd     := FDataSet.FindField(FCmdField);
  lType    := FDataSet.FindField(FTypeField);
  lCategory:= FDataSet.FindField(FCategoryField);
  if not (lType is TNumericField) then lType := nil;
  { Incepem Parcurgerea }
  lValList := TStringList.Create;
  try
     InitContructor(nil, FRootKey);
  finally
    lValList.Free;
  end;

  if Assigned(FAfterOpen) then FAfterOpen(Self);
  
  FActive := True;
  if FDataSet.Active <> FOldDataSetActive then
     FDataSet.Active := FOldDataSetActive;

end;

procedure TATSAppCommands.InternalExecute(Command: TATSCommand);
var AMethod: TMethod;
    ANotify: TNotifyEvent;
    IsHandled, IsInternal : Boolean;

    function GetFirstParentForm(AControl: TControl): TCustomForm;
    begin
      Result := GetParentForm(AControl, True);
    end;

begin
  { Executam Methoda Asociata in Cazul in care exista }
  IsHandled  := Command.Execute;
  if not IsHandled then begin
    IsHandled :=  (FMethods <> nil) and (Command.CmdType in [ctBar, ctAction]) and (Command.Action > '');
    if (IsHandled) and (GetFirstParentForm(Screen.ActiveControl) = Application.MainForm) then begin
       AMethod := FMethods.GetMethodByName(Command.Action);
       IsInternal := aMethod.Code <> nil;
       if IsInternal then begin
          ANotify := TNotifyEvent(AMethod);
          ANotify(Command);
       end
       else begin
         { Incercam un ShellExecute ... aici putem sa incercam si un LoadLibrary si sa executam o
           Functie Exportata }
         ShellExecute(0, PChar('open'), PChar(Command.Action), nil, nil, SW_SHOWNORMAL);
       end;
    end;
  end;
end;

function TATSAppCommands.GetCommand(Index: Integer): TATSCommand;
begin
  Result := TATSCommand(FCommands[Index]);
end;

function TATSAppCommands.GetCommandCount: Integer;
begin
  Result := FCommands.Count;
end;

function TATSAppCommands.GetEnabled(ACommand: TATSCommand): Boolean;
begin
  Result := (Assigned(FMethods)) and
            (FMethods.MethodExists(ACommand.Action));
end;

procedure TATSAppCommands.Loaded;
begin
  inherited Loaded;
  if FReadedActive then DoInternalLoad;
end;

procedure TATSAppCommands.Notification(AComponent: TComponent;
  AOperation: TOperation);
begin
  if (AOperation = opRemove) and (FDataSet = AComponent) then
     FDataSet := nil;
  if (AOperation = opRemove) and (AComponent = FMethods) then
     FMethods := nil;
  inherited Notification(AComponent, AOperation);
end;

procedure TATSAppCommands.SetActive(const Value: Boolean);
begin
  if csLoading in ComponentState then
     FReadedActive := Value
  else if Value then DoInternalLoad
       else DoInternalClear;
end;

procedure TATSAppCommands.SetCaptionField(const Value: String);
begin
  FCaptionField := Value;
end;

procedure TATSAppCommands.SetDataSet(DataSet: TDataSet);
begin
  FDataSet := DataSet;
end;

procedure TATSAppCommands.SetKeyField(const Value: String);
begin
  FKeyField := Value;
end;

procedure TATSAppCommands.SetParentField(const Value: String);
begin
  FParentField := Value;
end;

procedure TATSAppCommands.Execute(Command: String);
var
  lCommand: TATSCommand;
begin
  lCommand := GetCommandByName(Command);
  if Assigned(lCommand) then
    Execute(lCommand);
end;

function TATSAppCommands.GetCommandByName(
  const CommandName: String): TATSCommand;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to CommandCount-1 do begin
    Result := Commands[I];
    if SameText(CommandName, Result.Action) then
      Break
    else begin
      Result := Result.GetCommandByName(CommandName);
      if Assigned(Result) then
        Break;
    end;
  end;
end;

{ TATSCommand }

procedure TATSCommand.AddCommand(ACommand: TATSCommand);
begin
  FCommands.Add(ACommand);
end;

procedure TATSCommand.Assign(ACommand: TATSCommand);
var I: Integer;
    lCommand: TATSCommand;
begin
  Action   := ACommand.Action;
  Caption  := ACommand.Caption;
  Category := ACommand.Category;
  Hint     := ACommand.Hint;
  Tag      := ACommand.Tag;
  Value    := ACommand.Value;
  CmdType  := ACommand.CmdType;
  Enabled  := ACommand.Enabled;
  Visible  := ACommand.Visible;
  { Copiem si subcomenzile }
  FCommands.Clear;
  { Aici o sa se apeleze singur recursiv exact cum ne trebuie }
  for I := 0 to ACommand.CommandCount-1 do begin
    lCommand := TATSCommand.Create(Self);
    lCommand.Assign(ACommand.Commands[I]);
  end;
end;

procedure TATSCommand.ClearCurentCmds;
begin
  FCommands.Clear;
end;

constructor TATSCommand.Create(AParent: TATSCommand);
begin
  inherited Create;
  FEnabled := True;
  FVisible := True;
  FParent  := AParent;
  FCommands := TATSCommandList.Create(Self);
  if FParent <> nil then FParent.AddCommand(Self);
end;

destructor TATSCommand.Destroy;
begin
  FCommands.Free;
  inherited Destroy;
end;

function TATSCommand.Execute: Boolean;
begin
  Result := Assigned(FOnExecute);
  if Result then
    FOnExecute(FCommands, Self, Result);
end;

function TATSCommand.GetComandCount: Integer;
begin
  Result := FCommands.Count;
end;

function TATSCommand.GetCommandByName(
  const CommandName: String): TATSCommand;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to CommandCount - 1 do begin
    Result := Commands[I];
    if SameText(Result.Action, CommandName) then
      Break
    else begin
      Result := Result.GetCommandByName(CommandName);
      if Assigned(Result) then
        Break;
    end;
  end;
end;

function TATSCommand.GetCommands(Index: Integer): TATSCommand;
begin
  Result := FCommands.Commands[Index];
end;

procedure TATSCommand.LoadFromCmds(ACmds: TATSAppCommands);
var I: Integer;
    lCmd: TATSCommand;
begin
//  ClearCurentCmds;
  for I := 0 to ACmds.CommandCount-1 do begin
    lCmd := TATSCommand.Create(Self);
    lCmd.Assign(ACmds.Commands[I]);
  end;
end;

procedure TATSCommand.LoadFromMenu(AItem: TMenuItem; AEvent: TATSOnExecuteCmd = nil);
var
  I: Integer;
  lCommand : TATSCommand;
begin
  for I := 0 to AItem.Count-1 do begin
    lCommand := TATSCommand.Create(Self);
    lCommand.Caption   := AItem.Items[I].Caption;
    lCommand.Hint      := AItem.Items[I].Hint;
    lCommand.Tag       := AItem.Items[I].Tag;
    lCommand.Value     := AItem.Items[I].Tag;
    lCommand.Action    := '';
    if AItem.Items[I].Count > 0 then begin
//      lCommand.CmdType := ctPopup;
      lCommand.LoadFromMenu(AItem.Items[I], AEVent);
    end
    else begin
      lCommand.CmdType := ctBar;
      if lCommand.Tag <> 0 then
        lCommand.OnExecute := AEvent;
    end;
  end;
end;

end.
