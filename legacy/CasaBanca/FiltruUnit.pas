unit FiltruUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, StdCtrls, dxCntner, dxEditor, dxExEdtr, dxEdLib, 
  dxTL, ZDataSet, DBCtrls, dxInspct,
  dxDBInsp, dxInspRw, dxDBInRw, Buttons, Db, dxDBELib, dxDBGrid,
  CommonCasa, dxDBTL, dxDBTLCl, dxDBCtrl, dxGrClms,
  ZAbstractRODataset, ZAbstractDataset;

type

  TFrmFiltru = class(TForm)
    pnTop: TPanel;
    bvTop: TBevel;
    lbTop: TLabel;
    pnTimeDelaLa: TPanel;
    edDataDeLa: TdxDateEdit;
    Label2: TLabel;
    Label3: TLabel;
    edDataLa: TdxDateEdit;
    pnTimeSaptamana: TPanel;
    Label4: TLabel;
    Label5: TLabel;
    edSaptamana: TdxImageEdit;
    Label6: TLabel;
    edLunaAn: TdxImageEdit;
    pnTimeZiua: TPanel;
    edZi: TdxDateEdit;
    pnTimeAnul: TPanel;
    edAn: TdxImageEdit;
    pnTimeLunaAn: TPanel;
    edLuna: TdxImageEdit;
    pnTimePeriod: TPanel;
    edNrZile: TdxSpinEdit;
    rb_Zile: TRadioButton;
    rb_Sapt: TRadioButton;
    rb_Luni: TRadioButton;
    rb_Ani: TRadioButton;
    rbDelaLa: TRadioButton;
    rbZiua: TRadioButton;
    rbSaptamana: TRadioButton;
    rbLuna: TRadioButton;
    rbAnul: TRadioButton;
    rbLast: TRadioButton;
    pnFiltre: TPanel;
    DTFiltre: TDataSource;
    btnSave: TBitBtn;
    btnDelete: TBitBtn;
    MemFiltre: TZQuery;
    btnNewFilter: TBitBtn;
    btnSub: TBitBtn;
    btnSwitchToFiltre: TBitBtn;
    DTCache: TDataSource;
    QryCache: TZQuery;
    pnRest: TPanel;
    pnRight: TPanel;
    GridRecentFilter: TdxDBGrid;
    GridRecentFilterID_CACHE_FILTRE: TdxDBGridMaskColumn;
    GridRecentFilterDENUMIRE: TdxDBGridMaskColumn;
    GridRecentFilterFILTER_STRING: TdxDBGridMaskColumn;
    GridRecentFilterCOMENT: TdxDBGridMaskColumn;
    GridRecentFilterDATA_FILTRU: TdxDBGridDateColumn;
    GridRecentFilterSTARE: TdxDBGridMaskColumn;
    GridRecentFilterID_UTILIZATOR: TdxDBGridMaskColumn;
    GridRecentFilterID_LOGIN: TdxDBGridMaskColumn;
    pnFiltru: TPanel;
    Splitter1: TSplitter;
    DBInspector: TdxDBInspector;
    DBInspectorDENUMIRE: TdxInspectorDBMaskRow;
    DBInspectorFILTER_STRING: TdxInspectorDBMaskRow;
    DBInspectorID_UTILIZATOR: TdxInspectorDBMaskRow;
    DBInspectorLOGIN_MOD: TdxInspectorDBMaskRow;
    DBInspectorSTARE: TdxInspectorDBMaskRow;
    DBInspectorCOMENT: TdxInspectorDBMemoRow;
    DBInspectorRow7: TdxInspectorDBRow;
    DBInspectorRow8: TdxInspectorDBRow;
    FilterTree: TdxDBTreeList;
    FilterTreeRecId: TdxDBTreeListColumn;
    FilterTreeDENUMIRE: TdxDBTreeListMaskColumn;
    FilterTreeFILTER_STRING: TdxDBTreeListMaskColumn;
    FilterTreeID_PARENT: TdxDBTreeListMaskColumn;
    FilterTreeID_FILTRE: TdxDBTreeListMaskColumn;
    FilterTreeID_UTILIZATOR: TdxDBTreeListMaskColumn;
    FilterTreeLOGIN_MOD: TdxDBTreeListMaskColumn;
    FilterTreeSTARE: TdxDBTreeListMaskColumn;
    Splitter2: TSplitter;
    pnTopFiltre: TPanel;
    edtFiltru: TdxDBButtonEdit;
    pnRightTop: TPanel;
    edtCacheFiltru: TdxDBButtonEdit;
    lbFiltru: TLabel;
    procedure FormCreate(Sender: TObject);
    procedure edLunaAnChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edDataDeLaDateChange(Sender: TObject);
    procedure edDataDeLaKeyPress(Sender: TObject; var Key: Char);
    procedure edtFiltruButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure btnDeleteClick(Sender: TObject);
    procedure MemFiltreNewRecord(DataSet: TDataSet);
    procedure btnNewFilterClick(Sender: TObject);
    procedure btnSubClick(Sender: TObject);
    procedure btnSwitchToFiltreClick(Sender: TObject);
    procedure edtFiltruChange(Sender: TObject);
    procedure edtCacheFiltruChange(Sender: TObject);
    procedure edtCacheFiltruButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure FilterTreeChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridRecentFilterChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
  private
    FStartDate: TDateTime;
    FEndDate: TDateTime;
    FDataSource: TDataSource;
    FFilter: String;
    procedure SetStateButMe(aBifa: TRadioButton; aState: Boolean);
    function GetFilter: String;
    { Private declarations }
  protected
    procedure ActualizarePerioade;
    procedure FillLunaAn(StartLuna, StartAn, EndLuna, EndAn : Integer; var aEdit : TdxImageEdit);
    procedure FillSapt(Year, Month : Integer; var aEdit:TdxImageEdit);
    procedure RegisterPanel(var aGroup : TList; aRadioButon : TRadioButton);
    procedure SetStareRadio(var Message: TMessage); message WM_REFRESH_RADIO;
    procedure RBClick(Sender: TObject; Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
    function  AnLnToDate(aStr : String) : TDate;
    procedure AddFilterToList(aFilter : String);
  public
    { Public declarations }
    PerioadaMin : TDate;
    PerioadaMax : TDate;
    GroupList : TList;
    procedure SetDatasetFilter;
    procedure PopulateMemFilter;
    procedure CheckAndSave;
    function  GetPanelFromGroup : TPanel;
    procedure SetInterval(var StartDate, EndDate :TDateTime; aPanel : TPanel);
    property  StartDate : TDateTime read FStartDate write FStartDate;
    property  EndDate : TDateTime read FEndDate write FEndDate;
    property  DataSource : TDataSource read FDataSource write FDataSource;
    property  Filter : String read GetFilter write FFilter;
  end;

function Min(a,b: Integer) : Integer;


implementation

uses UnitFormule, DateUnit, CommonDBVar;

{$R *.DFM}


function Min(a,b: Integer) : Integer;
begin
   if a>b then Result := b else Result := a;
end;


procedure TFrmFiltru.ActualizarePerioade;
var StartLuna, StartAn, EndLuna, EndAn, StartDay, EndDay : Word;
begin
   DecodeDate(PerioadaMin, StartAn, StartLuna,  StartDay);
   DecodeDate(PerioadaMax, EndAn, EndLuna,  EndDay);

  {de la data la data}
     //nimic de actualizat
  {in Saptamana}
     // umplem luniile din an
     FillLunaAn(StartLuna, StartAn, EndLuna, EndAn, edLunaAn);
  {in Ziua}

  {in Anul}

  {in Luna}
      FillLunaAn(StartLuna, StartAn, EndLuna, EndAn, edLuna);
  {Ultimiile n}
end;

procedure TFrmFiltru.FillLunaAn(StartLuna, StartAn, EndLuna, EndAn: Integer;
  Var aEdit : TdxImageEdit);
var An, Luna:Integer;
begin
  aEdit.Descriptions.Clear;
  aEdit.Values.Clear;
  for An := StartAn to EndAn do
    for Luna:= 1 to 12 do
      if (An>StartAn) or ((Luna>=StartLuna) and (An = StartAn)) or ((Luna<=EndLuna) and (An = EndAn)) then begin
        aEdit.Descriptions.Add(Format(cst_LunaDinAn, [Luni[Luna], IntToStr(An)]));
        aEdit.Values.Add(Format('%8.0f',[EncodeDate(An,Luna, 01)]));
      end;
  aEdit.Text := '';
end;

procedure TFrmFiltru.FormCreate(Sender: TObject);
begin
  PerioadaMin := Date-1000;
  PerioadaMax := Date;

  GroupList := TList.Create;
  RegisterPanel(GroupList, rbDelaLa);
  RegisterPanel(GroupList, rbAnul);
  RegisterPanel(GroupList, rbLast);
  RegisterPanel(GroupList, rbLuna);
  RegisterPanel(GroupList, rbSaptamana);
  RegisterPanel(GroupList, rbZiua);

  ActualizarePerioade;

  RBClick(rbLast, mbLeft, [ssLeft], 0, 0);
  PopulateMemFilter;
end;

procedure TFrmFiltru.edLunaAnChange(Sender: TObject);
var aDate : TDateTime;
    An, Luna, Zi : Word;
begin
   with TdxImageEdit(Sender) do
      aDate := StrToFloat(Text);
   DecodeDate(aDate, An, Luna, Zi);
   FillSapt(an, Luna, edSaptamana);
end;

procedure TFrmFiltru.FillSapt(Year, Month : Integer; Var aEdit:TdxImageEdit);
var I, Max :Integer;
begin
  aEdit.Descriptions.Clear;
  aEdit.Values.Clear;
  Max := MonthDays[IsLeapYear(Year), Month];
  for I:= 1 to (Max div 7) + Integer((Max mod 7)>0) do begin
    aEdit.Descriptions.Add(Format(cst_Saptamana, [IntToStr(I), IntToStr((I-1)*7+1), IntToStr(Min(I*7, Max))]));
    aEdit.Values.Add(IntToStr(I));
  end;
  aEdit.Text := '';
end;

procedure TFrmFiltru.SetDatasetFilter;
var aPanel :TPanel;
begin
  //functia realizaeaza 2 lucruri
  //1 detecteaza panelul curent selectat
  aPanel := GetPanelFromGroup;
  //2 in functie de parametrii panelului seteaza FStartDate si FEndDate
  SetInterval(FStartDate, FEndDate, aPanel);
end;

procedure TFrmFiltru.RegisterPanel(var aGroup: TList;
  aRadioButon: TRadioButton);
begin
   aGroup.Add(aRadioButon);
   aRadioButon.OnMouseDown := RBClick;
end;

procedure TFrmFiltru.FormDestroy(Sender: TObject);
begin
  GroupList.Free;
end;

procedure TFrmFiltru.SetStareRadio(var Message: TMessage);
var I : Integer;
begin
  for I:= 0 to GroupList.Count -1 do
    if GroupList.Items[I]<> Pointer(Message.WParam) then
    begin
      TRadioButton(GroupList.Items[I]).Checked := False;
      SetStateButMe(TRadioButton(GroupList.Items[I]), False);
    end;
end;

procedure TFrmFiltru.RBClick(Sender: TObject; Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
begin
  SendMessage(FiltruHandle, WM_REFRESH_RADIO, Integer(Sender), 0);
  SetStateButMe(TRadioButton(Sender), True);
end;


procedure TFrmFiltru.SetStateButMe(aBifa: TRadioButton; aState : Boolean);
var I : Integer;
    aParent : TPanel;
begin
  aParent := TPanel(aBifa.Parent);
  for I:= 0 to aParent.ControlCount- 1 do
    if aParent.Controls[I] <> aBifa then
        aParent.Controls[I].Enabled := aState;
end;


function TFrmFiltru.GetPanelFromGroup: TPanel;
var  I: Integer;
begin
  Result := nil; 
  for I:= 0 to GroupList.Count-1 do
    if TRadioButton(GroupList.Items[I]).Checked then  begin
       Result := TPanel(TRadioButton(GroupList.Items[I]).Parent);
       Break;
    end;
end;

procedure TFrmFiltru.SetInterval(var StartDate, EndDate: TDateTime;
  aPanel: TPanel);
var Year, Month, Day :Word;
    aDate  : TDate;
    aNr : Integer;
begin
   Case aPanel.Tag of
      {pnTimedelala}
      1: begin
           StartDate := edDataDeLa.Date;
           EndDate   := edDataLa.Date;
         end;
      {pnTimeSaptamana}
      2: begin
           aDate := AnLnToDate(edLunaAn.Text);
           DecodeDate(aDate, Year, Month, Day);
           StartDate := EncodeDate(Year, Month, (7 * (StrToInt(edSaptamana.Text)-1))+1);
           EndDate   := EncodeDate(Year, Month, Min(7 * StrToInt(edSaptamana.Text) ,MonthDays[IsLeapYear(Year), Month]));
         end;
      {pnTimeLunaAn}
      3: begin
           aDate := AnLnToDate(edLuna.Text);
           DecodeDate(aDate, Year, Month, Day);
           StartDate := aDate;
           EndDate   := EncodeDate(Year, Month,MonthDays[IsLeapYear(Year), Month]);
         end;
      {pnTimedAnul}
      4: begin
           aNr := StrToInt(edAn.Text);
           StartDate := EncodeDate(aNr, 01, 01);
           EndDate   := EncodeDate(aNr, 12, 31);
         end;
      {pnTimeziua}
      5: begin
           StartDate := edZi.Date;
           EndDate   := edZi.Date;
         end;
      {pnTimedelala}
      6: begin
           {avem unitatea 1 zi}
           {1saptamana =7 zile}
           {1 luna = MonthDays[2004,month - .....]}
           {1 an = 365 + (2004) }
           aDate := Date;
           DecodeDate(aDate, Year, Month, Day);
           aNr := Trunc(edNrZile.Value);
           if rb_Zile.Checked then
              StartDate := aDate - aNr
           else
             if rb_Sapt.Checked then
                StartDate := aDate - 7* aNr
             else
               if rb_Luni.Checked then begin
                  Year := Year - (aNr div 12) - Integer(Month <= (aNr mod 12));
                  Month := 12* Integer(Month <= (aNr mod 12)) + Month-(aNr mod 12);
                  StartDate := EncodeDate(Year, Month, Day);
               end
                 else
                   if rb_Ani.Checked then begin
                      Year := Year - aNr;
                      StartDate := EncodeDate(Year, Month, Day);
                   end;
           EndDate := Date;
         end;

   end;
end;

function TFrmFiltru.AnLnToDate(aStr: String): TDate;
var aInt : Integer;
//    Month, Year : Word;
begin
    aInt := StrToInt(aStr);
    {Month := aInt mod 100;
    Year :=  1900+(aInt div 100);
    Result := EncodeDate(Year, Month, 01);}
    Result := aInt;
end;

procedure TFrmFiltru.edDataDeLaDateChange(Sender: TObject);
begin
  edDataLa.Date := edDataDeLa.Date;
end;

procedure TFrmFiltru.edDataDeLaKeyPress(Sender: TObject; var Key: Char);
begin
  if Key = #13 then
    if edDataDeLa.Focused then edDataDeLa.SetFocus
    else edDataLa.SetFocus;
end;

procedure TFrmFiltru.edtFiltruButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var aFilter : String;
    OldEdit : Boolean;
begin
   aFilter := edtFiltru.Text;
   if FilterCondition(FDataSource, aFilter) then begin
       OldEdit := MemFiltre.State in [dsEdit, dsInsert];
       if not OldEdit then MemFiltre.Edit;
       edtFiltru.Text := aFilter;
       MemFiltre.Post;
       if OldEdit then MemFiltre.Edit;
   end;
end;

procedure TFrmFiltru.PopulateMemFilter;
begin
   with MemFiltre do begin
     Params.ParamByName('ID_UTILIZATOR').Value := IdUtilizator;
     Open;
   end;
   with QryCache do begin
     Params.ParamByName('ID_UTILIZATOR').Value := IdUtilizator;
     Open;
   end;
end;

procedure TFrmFiltru.CheckAndSave;
begin

end;

procedure TFrmFiltru.btnDeleteClick(Sender: TObject);
var aNode : TdxTreeListNode;
    HasChildren : Boolean;
    aId : Integer;
begin
  if not Assigned(FilterTree.FocusedNode) then Exit;
  aNode := FilterTree.FocusedNode;
  HasChildren := aNode.HasChildren;
  aId := aNode.Values[FilterTreeID_FILTRE.Index];
  with GetTmpADOQuery do
    try
      SQL.Add('DELETE FROM FILTRE WHERE ID_FILTRE = :ID_FILTRE ');
      Params.ParamByName('ID_FILTRE').Value := aId;
      ExecSql;
      if HasChildren then
        if MessageDlg('Doriti sa stergeti si inregistrariile copil ? ', mtWarning, [mbYes, mbNo], 0) = mrYes then begin
          Sql.Clear;
          Sql.Add('DELETE FROM FILTRE WHERE ID_PARENT = :ID_FILTRE');
        end
        else begin
          Sql.Clear;
          Sql.Add('UPDATE FILTRE SET ID_PARENT = NULL WHERE ID_PARENT = :ID_FILTRE');
        end;
      Params.ParamByName('ID_FILTRE').Value := aId;
      ExecSql;
    finally
      Free;
    end;
  MemFiltre.Close;
  MemFiltre.Params.ParamByName('ID_UTILIZATOR').Value := IdUtilizator;
  MemFiltre.Open;
end;

procedure TFrmFiltru.MemFiltreNewRecord(DataSet: TDataSet);
begin
   DataSet.FieldByName('DENUMIRE').AsString := 'Filtru Nou';
   DataSet.FieldByName('ID_UTILIZATOR').AsInteger := IdUtilizator;
   DataSet.FieldByName('LOGIN_MOD').AsInteger := IdLogin;
   DataSet.FieldByName('STARE').AsInteger := 1;
end;

procedure TFrmFiltru.btnNewFilterClick(Sender: TObject);
var aId : Integer;
    aNode : TdxTreeListNode;
begin
  MemFiltre.Insert;
  MemFiltre.Post;
  aId := MemFiltre.FieldByName('ID_FILTRE').AsInteger;
  aNode := FilterTree.FindNodeByKeyValue(aId);
  if aNode <> nil then begin
     aNode.MakeVisible;
     aNode.Focused := True;
  end;

end;

procedure TFrmFiltru.btnSubClick(Sender: TObject);
var aNode : TdxTreeListNode;
    aId, aParentId : Integer;
begin
  aNode := FilterTree.FocusedNode;
  aParentId := -1;
  if aNode <> nil then aParentId := aNode.Values[FilterTreeID_FILTRE.Index];

  MemFiltre.Insert;
  if aParentId > 0 then
    MemFiltre.FieldByName('ID_PARENT').AsInteger := aParentId;
  MemFiltre.Post;
  aId := MemFiltre.FieldByName('ID_FILTRE').AsInteger;
  aNode := FilterTree.FindNodeByKeyValue(aId);
  if aNode <> nil then begin
     aNode.MakeVisible;
     aNode.Focused := True;
  end;
end;

procedure TFrmFiltru.btnSwitchToFiltreClick(Sender: TObject);
var aNode : TdxTreeListNode;
begin
  {mutam din istoric filtru in lista de filtre}
  if not Assigned(GridRecentFilter.FocusedNode) then Exit;
  aNode := GridRecentFilter.FocusedNode;
  with MemFiltre do begin
    Append;
    FieldByName('DENUMIRE').AsString := aNode.Strings[GridRecentFilterDENUMIRE.Index];
    FieldByName('FILTER_STRING').AsString := aNode.Strings[GridRecentFilterFILTER_STRING.Index];
    FieldByName('ID_UTILIZATOR').AsInteger := aNode.Values[GridRecentFilterID_UTILIZATOR.Index];
    FieldByName('LOGIN_MOD').AsInteger := aNode.Values[GridRecentFilterID_LOGIN.Index];
    FieldByName('STARE').AsInteger := 1;
    Post;
  end;
end;


function TFrmFiltru.GetFilter: String;
var aFilter : String;
begin
  try
    aFilter := lbFiltru.Caption;
    AddFilterToList(aFilter);
    Result := aFilter;
  except
    raise;
  end;
end;

procedure TFrmFiltru.AddFilterToList(aFilter: String);
begin
  with QryCache do begin
    Append;
    FieldByName('DENUMIRE').AsString := 'Filtru Aplicat';
    FieldByName('DATA_FILTRU').AsDateTime := Now;
    FieldByName('FILTER_STRING').AsString := aFilter;
    FieldByName('ID_UTILIZATOR').AsInteger := IdUtilizator;
    FieldByName('ID_LOGIN').AsInteger := IdLogin;
    FieldByName('STARE').AsInteger := 1;
    Post;
  end;
end;

procedure TFrmFiltru.edtFiltruChange(Sender: TObject);
begin
  lbFiltru.Caption := edtFiltru.Text;
end;

procedure TFrmFiltru.edtCacheFiltruChange(Sender: TObject);
begin
  lbFiltru.Caption := edtCacheFiltru.Text;
end;

procedure TFrmFiltru.edtCacheFiltruButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var aFilter : String;
    OldEdit : Boolean;
begin
   aFilter := edtFiltru.Text;
   if FilterCondition(FDataSource, aFilter) then begin
       OldEdit := QryCache.State in [dsEdit, dsInsert];
       if not OldEdit then QryCache.Edit;
       edtFiltru.Text := aFilter;
       QryCache.Post;
       if OldEdit then QryCache.Edit;
   end;
end;


procedure TFrmFiltru.FilterTreeChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  edtFiltruChange(nil);
end;

procedure TFrmFiltru.GridRecentFilterChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  edtCacheFiltruChange(nil);
end;

end.


