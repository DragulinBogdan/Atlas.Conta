unit frmRepartitoriSelectUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, Menus,
  StdCtrls, cxButtons, ExtCtrls, cxControls, cxStyles, cxCustomData,
  cxFilter, cxData, cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel,
  cxClasses, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxContainer, cxTextEdit, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, cxSplitter, cxTL, cxTLdxBarBuiltInMenu,
  cxInplaceContainer, cxTLData, cxDBTL, cxCheckBox, cxPC, cxCheckComboBox,
  cxDBCheckComboBox, cxGroupBox, cxCheckGroup, dxScrollbarAnnotations;

type
  TOnSelectCloseEvent = procedure (Sender : TObject; Accept : Boolean) of object;
  TfrmRepartitoriSelect = class(TForm)
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    pnContent: TPanel;
    pnBottom: TPanel;
    pnFilter: TPanel;
    cxSplitter: TcxSplitter;
    TreeRepartitori: TcxDBTreeList;
    TreeRepartitoriID_REPARTITORI: TcxDBTreeListColumn;
    TreeRepartitoriNUME: TcxDBTreeListColumn;
    TreeRepartitoriADRESA: TcxDBTreeListColumn;
    TreeRepartitoriCOD_FISCAL: TcxDBTreeListColumn;
    TreeRepartitoriGESTINT: TcxDBTreeListColumn;
    TreeRepartitoriTIP_GESTIUNE: TcxDBTreeListColumn;
    TreeRepartitoriTIP_REPARTITOR: TcxDBTreeListColumn;
    chkFilter: TcxCheckGroup;
    edFilter: TcxCheckComboBox;
    procedure FormCreate(Sender: TObject);
    procedure edFilterPropertiesCloseUp(Sender: TObject);
    procedure edFilterPropertiesEditValueChanged(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edFilterPropertiesStatesToEditValue(Sender: TObject;
      const ACheckStates: TcxCheckStates; out AValue: Variant);
    procedure edFilterPropertiesEditValueToStates(Sender: TObject;
      const AValue: Variant; var ACheckStates: TcxCheckStates);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure pnFilterResize(Sender: TObject);
    procedure chkFilterPropertiesEditValueToStates(Sender: TObject;
      const AValue: Variant; var ACheckStates: TcxCheckStates);
    procedure chkFilterPropertiesStatesToEditValue(Sender: TObject;
      const ACheckStates: TcxCheckStates; out AValue: Variant);
    procedure chkFilterPropertiesEditValueChanged(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure TreeRepartitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
    FilterChanged : Boolean;
    FilterList : TStringList;
    FCont: String;
    FIdRepartitori: Integer;
    FNumeRepartitor : String;
    FFoundById: Boolean;
    FOkClosed: Boolean;
    FOnSelectCloseEvent: TOnSelectCloseEvent;
    procedure CreateFilters;
    procedure SetCont(const Value: String);
    function  GetNumeRepartitor: String;
    procedure SetIdRepartitor(const Value: Integer);
  protected
    function GetNodeByIdRepartitor(aIdRepartitor : Integer ) : TcxdbTreeListNode;
    procedure RepFilter(DataSet: TDataSet;  var Accept: Boolean);
  public
    { Public declarations }
    function GetNumeByIdRepartitor(aIdRepartitori : Integer) : string;
    procedure SetFilter(aValue : String);
    property Cont : String read FCont write SetCont;
    property IdRepartitor: Integer read FIdRepartitori write SetIdRepartitor;
    property NumeRepartitor : String read GetNumeRepartitor;
    property OkClosed : Boolean read FOkClosed;
    property FoundById : Boolean read FFoundById;
    property OnSelectCloseEvent : TOnSelectCloseEvent read FOnSelectCloseEvent write FOnSelectCloseEvent;
  end;


implementation

uses
  ZeosDBUtile, dateUnit, Math;

{$R *.dfm}

procedure TfrmRepartitoriSelect.CreateFilters;
begin
  chkFilter.Properties.Items.Clear;
  edFilter.Properties.Items.Clear;
  if not frmData.QryRepTipuri.Active then
    DBRefresh(frmData.QryRepTipuri);

  with edFilter.Properties.Items.Add do begin
    Tag := -1;
    Description := 'Toate Tipurile';
    ShortDescription := 'Toate Tipurile';
  end;
  with chkFilter.Properties.Items.Add do begin
    Tag := -1;
    Caption := 'Toate Tipurile';
  end;

  with frmData.QryRepTipuri do
  try
    DisableControls;
    First;
    while not Eof do begin
      with edFilter.Properties.Items.Add do begin
        Tag := FieldByName('ID_REPARTITORI_TIPURI').AsInteger;
        Description := FieldByName('CONT').AsString  + ' ' + FieldByName('DENUMIRE').AsString;
        ShortDescription := FieldByName('DENUMIRE').AsString;
      end;
      with chkFilter.Properties.Items.Add do begin
        Tag := FieldByName('ID_REPARTITORI_TIPURI').AsInteger;
        Caption := FieldByName('CONT').AsString  + ' ' + FieldByName('DENUMIRE').AsString;
      end;
      Next;
    end;
  finally
    EnableControls;
  end;
 
  FilterChanged := False;
end;

procedure TfrmRepartitoriSelect.FormCreate(Sender: TObject);
begin
  FilterList := TStringList.Create;
  CreateFilters;
  cxSplitter.CloseSplitter;
end;

procedure TfrmRepartitoriSelect.edFilterPropertiesCloseUp(Sender: TObject);
begin
  if FilterChanged then SetFilter(edFilter.EditValue);
end;

procedure TfrmRepartitoriSelect.edFilterPropertiesEditValueChanged(
  Sender: TObject);
begin
  FilterChanged := True;
end;

procedure TfrmRepartitoriSelect.SetCont(const Value: String);
begin
  FCont := Value;

end;

procedure TfrmRepartitoriSelect.SetFilter;
begin
  FilterList.Clear;
  if aValue <> '' then begin
    FilterList.CommaText := aValue;
  end;
  frmData.QryRepartitori.Filtered := False;
  frmData.QryRepartitori.OnFilterRecord := RepFilter;
  frmData.QryRepartitori.Filtered := (FilterList.IndexOf('-1') = -1) ;
  FilterChanged := False;
end;

procedure TfrmRepartitoriSelect.RepFilter(DataSet: TDataSet;
  var Accept: Boolean);
var
  I : Integer;
begin
  Accept := False;
  for I := 0 to FilterList.Count - 1 do begin
     Accept := Accept or (Pos( '|' + FilterList[I] + '|',  DataSet.FieldByName('TIP_REPARTITOR').AsString) > 0);
     if Accept then Break;
  end;
end;

procedure TfrmRepartitoriSelect.FormDestroy(Sender: TObject);
begin
  FilterList.Free;
end;

procedure TfrmRepartitoriSelect.edFilterPropertiesStatesToEditValue(
  Sender: TObject; const ACheckStates: TcxCheckStates;
  out AValue: Variant);
var
  I : Integer;
  lStr : string;
begin
  lStr := '';
  for I := 0 to edFilter.Properties.Items.Count - 1 do
     if ACheckStates[I]= cbsChecked then
        lStr := lStr + IntToStr(edFilter.Properties.Items[I].Tag) + ',';
  if lStr <> '' then
    Delete(lStr, Length(lStr), 1);
  AValue := lStr;
end;

procedure TfrmRepartitoriSelect.edFilterPropertiesEditValueToStates(
  Sender: TObject; const AValue: Variant;
  var ACheckStates: TcxCheckStates);
var
  lStr : string;
  lStrList : TStringList;
  I : Integer;
begin
  lStr := AValue;
  lStrList := TStringList.Create;
  lStrList.CommaText := lStr;
  for I := 0 to edFilter.Properties.Items.Count - 1 do
    if lStrList.IndexOf(IntToStr(edFilter.Properties.Items[I].Tag)) > -1 then
      ACheckStates[I] := cbsChecked
    else
      ACheckStates[I] := cbsUnchecked;
  lStrList.Free;
end;

procedure TfrmRepartitoriSelect.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  frmData.QryRepartitori.Filtered := False;
  frmData.QryRepartitori.OnFilterRecord := nil;
end;

procedure TfrmRepartitoriSelect.pnFilterResize(Sender: TObject);
begin
  if cxSplitter.State = ssOpened then begin
    chkFilter.Properties.Columns := chkFilter.Width div 150;
    pnFilter.Height :=  20 + Ceil(chkFilter.Properties.Items.Count / chkFilter.Properties.Columns) * 15 ;
  end;
end;

procedure TfrmRepartitoriSelect.chkFilterPropertiesEditValueToStates(
  Sender: TObject; const AValue: Variant;
  var ACheckStates: TcxCheckStates);
var
  lStr : string;
  lStrList : TStringList;
  I : Integer;
begin
  lStr := AValue;
  lStrList := TStringList.Create;
  lStrList.CommaText := lStr;
  for I := 0 to chkFilter.Properties.Items.Count - 1 do
    if lStrList.IndexOf(IntToStr(chkFilter.Properties.Items[I].Tag)) > -1 then
      ACheckStates[I] := cbsChecked
    else
      ACheckStates[I] := cbsUnchecked;
  lStrList.Free;
end;

procedure TfrmRepartitoriSelect.chkFilterPropertiesStatesToEditValue(
  Sender: TObject; const ACheckStates: TcxCheckStates;
  out AValue: Variant);
var
  I : Integer;
  lStr : string;
begin
  lStr := '';
  for I := 0 to chkFilter.Properties.Items.Count - 1 do
     if ACheckStates[I]= cbsChecked then
        lStr := lStr + IntToStr(chkFilter.Properties.Items[I].Tag) + ',';
  if lStr <> '' then
    Delete(lStr, Length(lStr), 1);
  AValue := lStr;
end;


procedure TfrmRepartitoriSelect.chkFilterPropertiesEditValueChanged(
  Sender: TObject);
begin
  SetFilter(chkFilter.EditValue);
end;

function TfrmRepartitoriSelect.GetNumeRepartitor: String;
begin
  Result := FNumeRepartitor;
end;

procedure TfrmRepartitoriSelect.SetIdRepartitor(const Value: Integer);
var
  lNode : TcxDBTreeListNode;
begin
  FIdRepartitori := Value;
  lNode := GetNodeByIdRepartitor(FIdRepartitori);
  FFoundById := (lNode <> nil);//frmdata.QryRepartitori.Locate('ID_REPARTITORI', FIdRepartitori, []);
  FNumeRepartitor := '';
  if FFoundById then begin
    FNumeRepartitor := lNode.Texts[TreeRepartitoriNUME.ItemIndex];
    lNode.MakeVisible;
    lNode.Focused := True;
  end;
end;

procedure TfrmRepartitoriSelect.BtnOkClick(Sender: TObject);
begin
   if (frmData.QryRepartitori.RecordCount <> 0) then begin
     FIdRepartitori := frmData.QryRepartitori.FieldbyName('ID_REPARTITORI').AsInteger;
     FNumeRepartitor := frmData.QryRepartitori.FieldbyName('NUME').AsString;
     FOkClosed := True;
   end;
   if Assigned(FOnSelectCloseEvent) then
      FOnSelectCloseEvent(Self, FOkClosed)
   else
     Close;
end;

procedure TfrmRepartitoriSelect.BtnCancelClick(Sender: TObject);
begin
  FOkClosed := False;
   if Assigned(FOnSelectCloseEvent) then
      FOnSelectCloseEvent(Self, FOkClosed)
   else
     Close;
end;

procedure TfrmRepartitoriSelect.TreeRepartitoriDblClick(Sender: TObject);
begin
   BtnOkClick(nil);
end;

procedure TfrmRepartitoriSelect.TreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
     BtnCancel.Click;
  if (Key = VK_RETURN) and (TcxDBTreeList(TreeRepartitori).FocusedNode <> nil) then
     BtnOk.Click;
end;

function TfrmRepartitoriSelect.GetNodeByIdRepartitor(
  aIdRepartitor: Integer): TcxdbTreeListNode;
begin
   Result := TcxDBTreeListNode(TreeRepartitori.FindNodeByKeyValue(aIdRepartitor));
end;

function TfrmRepartitoriSelect.GetNumeByIdRepartitor(
  aIdRepartitori: Integer): string;
var
  lNode : TcxDBTreeListNode;
begin
  Result := '';
  lNode := GetNodeByIdRepartitor(aIdRepartitori);
  if lNode <> nil then Result := lNode.Texts[TreeRepartitoriNUME.ItemIndex];
end;

end.
