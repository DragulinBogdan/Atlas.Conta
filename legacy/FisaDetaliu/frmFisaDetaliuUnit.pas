unit frmFisaDetaliuUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics,
  Controls, Forms, Dialogs, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxPCdxBarPopupMenu, cxPC, cxStyles, cxCustomData,
  cxFilter, cxData, cxDataStorage, cxEdit, DB, cxDBData, cxMaskEdit,
  cxCalendar, cxImageComboBox, cxCurrencyEdit, cxGridLevel,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxClasses,
  cxGridCustomView, cxGrid, cxNavigator, ZDataset,
  ZAbstractRODataset, ZAbstractDataset, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxTL, cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL,
  ExtCtrls, Menus, ToolWin, ComCtrls, StdCtrls, cxButtons, FisaDetaliuUnit;

type

  TfrmFisaDetaliu = class(TForm)
    tabTipuriFise: TcxTabControl;
    cxGridFisaDetaliu: TcxGrid;
    GridFisaDetaliu: TcxGridDBTableView;
    cxGridFisaDetaliuLevel1: TcxGridLevel;
    dsFisaMaterial: TDataSource;
    qryFisaDetaliu: TZQuery;
    cxGridPopupMenu: TcxGridPopupMenu;
    TreeDetaliu: TcxDBTreeList;
    pnTree: TPanel;
    pnToolsTree: TPanel;
    BtnExpandTree: TcxButton;
    BtnCollapseTree: TcxButton;
    NiveleTree: TToolBar;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure tabTipuriFiseChange(Sender: TObject);
    procedure BtnCollapseTreeClick(Sender: TObject);
    procedure BtnExpandTreeClick(Sender: TObject);
  private
    { Private declarations }
    FSQLDescribe : string;
    FSQLParameters : string;
    FFirstSet  : TStringList;
    procedure EmptyTipuriFise;
    procedure ExpandTreeButton(Sender : TObject);
    function GetHasTabs: Boolean;
  published
    procedure ClickButton(Sender: TToolButton);
    procedure PopulateTipFise;
    procedure ConstructTreeLevels;
  public
    { Public declarations }
    procedure ControlBeginUpdate(aTab : TTipFisa);
    procedure ControlEndUpdate(aTab : TTipFisa; const AMustBestFit :Boolean = False );
    procedure PopulateFields(aTab : TTipFisa);
    procedure SetParam(aParamName : string; aValue : Variant);
    procedure RefreshDataSet;
    function CloseDataSet : Boolean;
    procedure OpenDataSet;
    constructor Create(AOwner: TComponent;aSQLDescribe, aSQLParameters : String);

    property HasTabs : Boolean read GetHasTabs;
  end;


implementation

uses
  DateUnit, CommonDBVar;


{$R *.dfm}

procedure ExpandInner(aNode: TcxTreeListNode; Level: Integer);
var
  J : Integer;
begin
  if Level > 0 then begin
     aNode.Expand(False);
     for J := 0 to aNode.Count-1 do
       ExpandInner(aNode.Items[J], Level-1);
  end;
end;

procedure InternalExpandTree(aToolButton : TToolButton; aTree : TcxDBTreeList);
var I: Integer;
begin
  with aToolButton do begin
    aTree.BeginUpdate;
    aTree.FullCollapse;
    try
       for I := 0 to aTree.Count-1 do
         ExpandInner(aTree.Items[I], Tag-1);
    finally
       aTree.EndUpdate;
    end;
  end;
end;

procedure TfrmFisaDetaliu.ClickButton(Sender: TToolButton);
begin
  Sender.Down := True;
  Sender.Click;
end;

function TfrmFisaDetaliu.CloseDataSet : Boolean;
begin
  Result := qryFisaDetaliu.Active;
  if Result then
     qryFisaDetaliu.Close;
  Result := Result and IsMyFormVisible(Self);
end;

constructor TfrmFisaDetaliu.Create(AOwner: TComponent; aSQLDescribe,
  aSQLParameters: String);
begin
  inherited Create(AOwner);
   FSQLDescribe := aSQLDescribe;
   FSQLParameters := aSQLParameters;
   qryFisaDetaliu.Close;
   while qryFisaDetaliu.SQL.Count < 2 do
     qryFisaDetaliu.SQL.Add('');

   qryFisaDetaliu.SQL[0] := '';
   qryFisaDetaliu.SQL[1] := aSQLParameters;
   PopulateTipFise;    
end;

procedure TfrmFisaDetaliu.EmptyTipuriFise;
begin
  EmptyTipFise(tabTipuriFise.Tabs);
end;

procedure TfrmFisaDetaliu.FormCreate(Sender: TObject);
begin
  FFirstSet := TStringList.Create;
  FFirstSet.Duplicates := dupIgnore;
  tabTipuriFiseChange(nil);
end;

procedure TfrmFisaDetaliu.FormDestroy(Sender: TObject);
begin
  FFirstSet.Free;
  EmptyTipuriFise;
end;

procedure TfrmFisaDetaliu.PopulateFields(ATab : TTipFisa);
var
  I : Integer;
begin
  qryFisaDetaliu.DisableControls;
  try
    if ATab.TipLista = 0 then begin
      GridFisaDetaliu.ClearItems;
      cxCreateMissingColumns(qryFisaDetaliu, GridFisaDetaliu);
      for I := 0 to GridFisaDetaliu.ColumnCount - 1 do
        GridFisaDetaliu.Columns[I].Visible :=
            (AnsiCompareText(aTab.FieldId, GridFisaDetaliu.Columns[I].DataBinding.FieldName) <> 0) and
            (AnsiCompareText(aTab.FieldParinte, GridFisaDetaliu.Columns[I].DataBinding.FieldName) <> 0)
        ;
     end
     else begin
        for I := TreeDetaliu.ColumnCount - 1 downto 0  do
          TreeDetaliu.Columns[I].Free;
       cxCreateMissingColumns(qryFisaDetaliu, TreeDetaliu);
        for I := 0 to TreeDetaliu.ColumnCount - 1 do
          TreeDetaliu.Columns[I].Visible :=
            (AnsiCompareText(aTab.FieldId, TcxDBItemDataBinding(TreeDetaliu.Columns[I].DataBinding).FieldName) <> 0) and
            (AnsiCompareText(aTab.FieldParinte, TcxDBItemDataBinding(TreeDetaliu.Columns[I].DataBinding).FieldName) <> 0)
     end;


  finally
    qryFisaDetaliu.EnableControls;
  end;
end;

procedure TfrmFisaDetaliu.PopulateTipFise;
begin
  PopulateTipFiseBySQL(FSQLDescribe, tabTipuriFise.Tabs);
end;

procedure TfrmFisaDetaliu.RefreshDataSet;
var
 lMustBestFit : Boolean;
 lId : Integer;
 lTab :  TTipFisa;
begin
  lMustBestFit := False;
  if not HasTabs then Exit;
  lTab := PTipFisa(TStrings(tabTipuriFise.Tabs).Objects[tabTipuriFise.TabIndex])^;
  ControlBeginUpdate(lTab);
  try
     qryFisaDetaliu.Close;
     qryFisaDetaliu.Open;
     lMustBestFit := (FFirstSet.IndexOf(IntToStr(lTab.Id))=-1);
     if lMustBestFit then begin
       PopulateFields(lTab);
       FFirstSet.Add(IntToStr(lTab.Id));
     end;
  finally
    ControlEndUpdate(lTab,  lMustBestFit);
  end;
end;


procedure TfrmFisaDetaliu.SetParam(aParamName: string; aValue: Variant);
var
  lOpen : Boolean;
  lParam : TParam;
begin
  lParam := qryFisaDetaliu.Params.FindParam(aParamName);
  if lParam = nil then Exit;
  lOpen := CloseDataSet;
  if lParam.Value <> aValue then begin
      lParam.Value := aValue;
  end;
  if lOpen and not qryFisaDetaliu.Active then qryFisaDetaliu.Open;
end;


procedure TfrmFisaDetaliu.tabTipuriFiseChange(Sender: TObject);
var
  lTipFisa : PTipFisa;
  lOpen : Boolean;
  lIndex : Integer;
begin
  lOpen := qryFisaDetaliu.Active;
  qryFisaDetaliu.Close;
  if not HasTabs then Exit;
  lTipFisa := PTipFisa(TStrings(tabTipuriFise.Tabs).Objects[tabTipuriFise.TabIndex]);
  qryFisaDetaliu.SQL[0] := 'exec [' + lTipFisa^.SQLProc + '] ';

   GridFisaDetaliu.DataController.KeyFieldNames := lTipFisa^.FieldId;
   TreeDetaliu.DataController.KeyField := lTipFisa^.FieldId;
   TreeDetaliu.DataController.ParentField := lTipFisa^.FieldParinte;

  if (lTipFisa^.TipLista = 0)
    and ( not(GridFisaDetaliu.Visible) or(GridFisaDetaliu.DataController.DataSource = nil)) then
  begin
     pnTree.Visible := False;
     TreeDetaliu.Visible := False;
     cxGridFisaDetaliu.Visible := True;
     TreeDetaliu.DataController.DataSource := nil;
     GridFisaDetaliu.DataController.DataSource := dsFisaMaterial;
  end
  else
    if (lTipFisa^.TipLista = 1)
      and ( not(TreeDetaliu.Visible) or(TreeDetaliu.DataController.DataSource = nil)) then
    begin
       pnTree.Visible := True;
       TreeDetaliu.Visible := True;
       cxGridFisaDetaliu.Visible := False;
       TreeDetaliu.DataController.DataSource := dsFisaMaterial;
       GridFisaDetaliu.DataController.DataSource := nil;
    end;



  lIndex := FFirstSet.IndexOf(IntToStr(lTipFisa^.Id));
  if lIndex <> -1 then
    FFirstSet.Delete(lIndex);
  if lOpen then RefreshDataSet;
end;

procedure TfrmFisaDetaliu.BtnCollapseTreeClick(Sender: TObject);
begin
  if NiveleTree.ButtonCount > 0 then
     ClickButton(NiveleTree.Buttons[0]);
end;

procedure TfrmFisaDetaliu.BtnExpandTreeClick(Sender: TObject);
begin
  if NiveleTree.ButtonCount > 0 then
     ClickButton(NiveleTree.Buttons[NiveleTree.ButtonCount-1]);
end;

type TCrackToolButton = class(TToolButton);

procedure TfrmFisaDetaliu.ConstructTreeLevels;
var I: Integer;
    lToolButton : TToolButton;
    lLevel : Integer;
begin
  { Stergem butoanele anterioare pentru Functional }
  for I := NiveleTree.ButtonCount-1 downto 0 do
    NiveleTree.Buttons[I].Free;
  lLevel := GetMaxLevel(TcxTreeList(TreeDetaliu));
  { Creem noile Butoane pentru Functional }
  for I := lLevel+1 downto 1 do begin
    lToolButton := TToolButton.Create(NiveleTree);
    lToolButton.Style   := tbsCheck;
    lToolButton.Tag     := I;
    lToolButton.Grouped := True;
    lToolButton.Caption := IntToStr(I);
    lToolButton.OnClick := ExpandTreeButton;
    TCrackToolButton(lToolButton).SetToolBar(NiveleTree);
  end;
  NiveleTree.Width := (lLevel + 1) * NiveleTree.ButtonWidth;
  BtnExpandTree.Enabled := lLevel > 0;
  BtnCollapseTree.Enabled := lLevel > 0;
end;

procedure TfrmFisaDetaliu.ExpandTreeButton(Sender: TObject);
begin
  InternalExpandTree(TToolButton(Sender), TreeDetaliu);
end;

procedure TfrmFisaDetaliu.ControlBeginUpdate(aTab: TTipFisa);
begin
  if aTab.TipLista = 0 then begin
    GridFisaDetaliu.BeginUpdate  {$IFDEF NOU}(lsimImmediate){$ENDIF};
  end
  else begin
    TreeDetaliu.BeginUpdate;
  end;
end;

procedure TfrmFisaDetaliu.ControlEndUpdate(aTab: TTipFisa;
  const AMustBestFit :Boolean );
begin
  if aTab.TipLista = 0 then begin
    GridFisaDetaliu.EndUpdate;
    if AMustBestFit then GridFisaDetaliu.ApplyBestFit;
  end
  else begin
    TreeDetaliu.EndUpdate;
    ConstructTreeLevels;
    if AMustBestFit then TreeDetaliu.ApplyBestFit;
  end;
end;

function TfrmFisaDetaliu.GetHasTabs: Boolean;
begin
   Result := (tabTipuriFise.Tabs.Count > 0) and (tabTipuriFise.TabIndex > -1);
end;

procedure TfrmFisaDetaliu.OpenDataSet;
begin
  if IsMyFormVisible(Self.tabTipuriFise) then
   qryFisaDetaliu.Open;
end;

end.
