unit AlopIntretinereConturi;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, DegradePanel, 
   cxControls, cxPC, cxGraphics,
  cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel,
  cxClasses, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, ZDataSet, Menus, cxLookAndFeelPainters, StdCtrls,
  cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, dxBarBuiltInMenu,
  cxNavigator, dxDateRanges, dxScrollbarAnnotations;


const
  cl_SelectedTab = $00A8FFFF;


type
  TfrmAlopIntretinereCont = class(TForm)
    pnTop: TDegradePanel;
    pnContent: TPanel;
    TabControl: TcxTabControl;
    GridConturi: TcxGridDBTableView;
    cxGridConturiL: TcxGridLevel;
    cxGridConturi: TcxGrid;
    DTConturi: TDataSource;
    qryConturi: TZQuery;
    btnAdd: TcxButton;
    btnDel: TcxButton;
    GridConturiid: TcxGridDBColumn;
    GridConturicont: TcxGridDBColumn;
    GridConturiromana: TcxGridDBColumn;
    procedure TabControlChange(Sender: TObject);
    procedure btnDelClick(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;


procedure IntretinereAlopConturi;

implementation

uses
  ZeosDBUtile, dateUnit, CommonDBVar, PlanConturiUnit, FormulareUnit;

{$R *.dfm}

procedure IntretinereAlopConturi;
begin
  with GetNewForm(TfrmAlopIntretinereCont) do
   Show;
end;

procedure TfrmAlopIntretinereCont.TabControlChange(Sender: TObject);
var I : Integer;
begin
  DoCheckClose(qryConturi);
  for I := 0 to TabControl.Tabs.Count - 1 do
    TabControl.Tabs[I].Color := TabControl.Color; 
  TabControl.Tabs[TabControl.TabIndex].Color := cl_SelectedTab;
  qryConturi.Params.ParamByName('tip').Value := TabControl.TabIndex;
  DoCheckOpen(qryConturi);
end;

procedure TfrmAlopIntretinereCont.btnDelClick(Sender: TObject);
var
  aCont : String;
  aId : String;
begin
  if GridConturi.Controller.FocusedRecord = nil then Exit;
  if not GridConturi.Controller.FocusedRecord.IsData then Exit;  
  aCont := GridConturi.Controller.FocusedRecord.Values[GridConturicont.Index];
  aId := GridConturi.Controller.FocusedRecord.Values[GridConturiid.Index];
  if (MessageDlg(Format('Doriti stergere contului %s din lista ?', [aCont] ), mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Abort;
  DBExecSQLFmt('exec [spAlopConturiDel] %s', [aId]);
  DBRefresh(qryConturi);
end;

procedure TfrmAlopIntretinereCont.btnAddClick(Sender: TObject);
var
  lCont : String;
  aTip : String;
  aQry : TZReadOnlyQuery;
begin
  lCont := '1';
  if SelectareContPlan(lCont) then
  begin
    if lCont = '<Anulat>' then Abort;
    aQry := GetTmpADOQuery;
    with aQry do
      try
         SQL.Add('Select .dbo.fnIsValidCont(' + QuotedStr(lCont) + ', 1) as EsteAnalitic ');
         Open;
         if not Fields[0].AsBoolean then
           if (MessageDlg(Format('Contul selectat ( %s ) este un cont sintetic. Doriti adaugarea analiticelor acestui cont ?', [lCont] ), mtWarning, [mbYes, mbNo], 0) <> mrYes) then Abort;
      finally
        Free;
      end;
    aTip :=  IntToStr(TabControl.TabIndex);
    DBExecSQLFmt('exec [spAlopConturiAdd] %s, %d', [ValueToStr(lCont), TabControl.TabIndex]);
    //+ QuotedStr(lCont) + ', ' + aTip);
    DBRefresh(qryConturi);
  end;
end;




procedure TfrmAlopIntretinereCont.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmAlopIntretinereCont.FormCreate(Sender: TObject);
var
 lDataSet : TDataSet;
begin
  TabControl.OnChange := nil;
  try
    TabControl.Tabs.Clear;
    lDataSet := DBNewQuery('exec spAlopTipConturi');
    try
      lDataSet.Open;
      lDataSet.First;
      while not lDataSet.Eof do begin
        TabControl.Tabs.Add(lDataSet.FieldByName('DENUMIRE').AsString);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
  finally
    TabControl.OnChange := TabControlChange;
  end;
  if TabControl.Tabs.Count > 0 then begin
    TabControl.TabIndex := 0;
    TabControlChange(TabControl);
  end
  else begin
    btnAdd.Visible := False;
    btnDel.Visible := False;    
  end;
end;

end.
