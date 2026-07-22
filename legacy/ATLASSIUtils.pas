unit ATLASSIUtils;

interface
uses Windows, Messages, Forms, Classes, SysUtils, dxBar;

type
  TCopyDataType = (cdtMenu, cdtRegisterModule);

  TRegisterModuleRecord = packed record
    AppNode: Integer;
    MainFormHandle: HWND;
  end;

const
  SIM_PROCMESS = WM_USER + 123;

  wpSendMenu = 101;
  wpInitForm = 102;
  wpResize = 103;
  wpExecuteAction = 104;
  wpCloseApp = 105;
  wpHideFromTaskBar = 107;

var
  FAppForm: TCustomForm;
  FSIHandle: HWND;
  FSIAppNode: Integer;
  FMenuBar: TdxBar;

  procedure SendData(cds: TCopyDataStruct; AHandle: HWND);
  procedure InitData(AMainForm: TCustomForm; AMenuBar: TdxBar = nil);
  procedure FreeData;
  procedure RegisterSIModule;
  procedure SendMenu(AMenuBar: TdxBar);
  procedure MenuClick(AMenuItem: Integer);
  procedure ResizeForm;
  procedure DoProcessMessage(var AMsg: TMessage);
  
implementation

//------------------------------------------------------------------------------
procedure SendData(cds: TCopyDataStruct; AHandle: HWND);
begin
  if AHandle > 0 then
    SendMessage(AHandle, WM_COPYDATA, Integer(FAppForm.Handle), Integer(@cds));
end;
//------------------------------------------------------------------------------
procedure InitData(AMainForm: TCustomForm; AMenuBar: TdxBar = nil);
var i: Integer;
begin
  FAppForm := AMainForm;
  FMenuBar := AMenuBar;

  for i := 1 to ParamCount do
  if SameText(Copy(ParamStr(i), 1, 10), '/PWHANDLE:') then
    FSIHandle := StrToInt(Copy(ParamStr(i), 11, length(ParamStr(i))-10))
  else
    if SameText(Copy(ParamStr(i), 1, 9), '/APPNODE:') then
      FSIAppNode := StrToInt(Copy(ParamStr(i), 10, length(ParamStr(i))-9));

  if FSIHandle > 0 then
    RegisterSIModule;
end;
//------------------------------------------------------------------------------
procedure FreeData;
begin
  if FSIHandle <> 0 then
    SendMessage(FSIHandle, SIM_PROCMESS, wpCloseApp, FAppForm.Handle);
end;
//------------------------------------------------------------------------------
procedure RegisterSIModule;
var
  RegMod : TRegisterModuleRecord;
  copyDataStruct : TCopyDataStruct;
begin
  RegMod.AppNode := FSIAppNode;
  RegMod.MainFormHandle := FAppForm.Handle;
  copyDataStruct.dwData := Integer(cdtRegisterModule);
  copyDataStruct.cbData := SizeOf(RegMod);
  copyDataStruct.lpData := @RegMod;
  SendData(copyDataStruct, FSIHandle);

  ShowWindow(Application.Handle, SW_HIDE);   //Ascunde buton din taskbar
//  Align := alClient;
//  WindowState := wsMaximized;
end;
//------------------------------------------------------------------------------
procedure SendMenu(AMenuBar: TdxBar);
var
  sl: TStringList;
  ms: TMemoryStream;
  cds: TCopyDataStruct;
  //============================================================================
  procedure AddItems(ILinks: TdxBarItemLinks; ALevel: Integer = 0);
  var
    i: Integer;
  begin
    for i := 0 to ILinks.Count - 1 do
      if ILinks.Items[i].Caption > '' then
      begin
        sl.Append(Format('%d|%s|%d', [ALevel, ILinks.Items[i].Caption, Integer(ILinks.Items[i])]));
        if ILinks.Items[i].Item is TdxBarSubItem then
          AddItems(TdxBarSubItem(ILinks.Items[i].Item).ItemLinks, ALevel + 1);
      end;
  end;
  //============================================================================
begin
  if FSIHandle = 0 then exit;
  if not Assigned(AMenuBar) then exit;
  
  sl := TStringList.Create;
  ms := TMemoryStream.Create;
  try
    sl.Append(IntToStr(FSIAppNode));
    AddItems(AMenuBar.ItemLinks);

    //showmessage(sl.Text);

    sl.SaveToStream(ms);
    cds.dwData := Integer(cdtMenu);
    cds.cbData := ms.Size;
    cds.lpData := ms.Memory;
    SendData(cds, FSIHandle);
  finally
    sl.Free;
    ms.Free;
  end;
end;
//------------------------------------------------------------------------------
procedure MenuClick(AMenuItem: Integer);
var
  lItem: TdxBarItemLink;
begin
  lItem := TdxBarItemLink(AMenuItem);
  if Assigned(lItem) and Assigned(lItem.Item) then
    if lItem.Item is TdxBarButton then
      TdxBarButton(lItem.Item).Click;
  FAppForm.SendToBack;
end;
//------------------------------------------------------------------------------
procedure ResizeForm;
begin
  with FAppForm do
  begin
    Visible := False;
    WindowState := wsNormal;
    WindowState := wsMaximized;
    Visible := True;
  end;
end;
//------------------------------------------------------------------------------
procedure DoProcessMessage(var AMsg: TMessage);
begin
  case AMsg.WParam of
    wpSendMenu: SendMenu(FMenuBar);
    wpExecuteAction: MenuClick(AMsg.LParam);
    wpResize: ResizeForm;
    wpCloseApp: FAppForm.Close;
    wpHideFromTaskBar: ShowWindow(Application.Handle, SW_HIDE);
  end;
end;
//------------------------------------------------------------------------------
end.
