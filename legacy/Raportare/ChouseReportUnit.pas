unit ChouseReportUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxCntner, dxTL, dxDBCtrl, dxDBTL, StdCtrls, Buttons, Db, ImgList, ZDataSet,
  dxExEdtr, ZAbstractRODataset, ZAbstractDataset;

type
  TFrmChouseReport = class(TForm)
    ListaReports: TdxDBTreeList;
    BtnOk: TBitBtn;
    BtnCancel: TBitBtn;
    DTReports: TDataSource;
    ListaReportsNAME: TdxDBTreeListMaskColumn;
    ImgStare: TImageList;
    QryReports: TZQuery;
    procedure ListaReportsChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure ListaReportsGetImageIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure ListaReportsGetStateIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure ListaReportsMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure ListaReportsGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure ListaReportsKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    FCurentReportId: Integer;
    FCurentReportName: String;
    FMultiSelect: Boolean;
    function GetCurentReport: String;
    procedure SetMultiSelect(const Value: Boolean);
    { Private declarations }
  public
    { Public declarations }
    procedure InvalidateNodes;
    function GetReportLists: String;
    procedure SetSelectedNode(const Node: TdxTreeListNode; const Valoare: Integer);
    procedure SetReportLists(const aRepList: String);
    property CurentReportId: Integer read FCurentReportId;
    property CurentReportName: String read FCurentReportName;
    property CurentReport: String read GetCurentReport;
    property MultiSelect: Boolean read FMultiSelect write SetMultiSelect default True;
  end;

implementation


{$R *.DFM}

{ TFrmChouseReport }

function TFrmChouseReport.GetCurentReport: String;
begin
  Result := '('+IntToStr(FCurentReportId)+')\'+FCurentReportName;
end;

procedure TFrmChouseReport.ListaReportsChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Node = nil then Exit;
  BtnOk.Enabled := not Node.HasChildren;
  FCurentReportId   := TdxDBTreeListNode(Node).Id;
  FCurentReportName := Node.Strings[ListaReportsNAME.Index];
  Node.MakeVisible;
  Node.Focused := True;
end;

procedure TFrmChouseReport.SetMultiSelect(const Value: Boolean);
begin
  FMultiSelect := Value;
end;

procedure TFrmChouseReport.ListaReportsGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Integer(Node.Data);
end;

procedure TFrmChouseReport.ListaReportsGetStateIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TFrmChouseReport.ListaReportsMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var Info: TdxTreeListHitInfo;
    aNode       : TdxDBTreeListNode;
begin
  Info  := ListaReports.GetHitInfo(Point(X,Y));
  aNode := TdxDBTreeListNode(Info.Node);
  if (aNode <> nil) and
     (not aNode.HasChildren) and
     (Info.hitType = htIcon) then begin
       SetSelectedNode(aNode, 1-Integer(aNode.Data));
    ListaReports.Invalidate;
  end;
end;

function TFrmChouseReport.GetReportLists: String;
var J: Integer;
    aRepList: String;

  procedure AddString(const aNode: TdxTreeListNode);
  begin
    if aRepList <> '' then
       aRepList := aRepList + ',';
    aRepList := aRepList + IntToStr(Integer(TdxDBTreeListNode(aNode).Id));
  end;

  procedure GetChildrens(aNode: TdxTreeListNode);
  var I: Integer;
   begin
     if aNode = nil then Exit;
     for I := 0 to aNode.Count-1 do
       if aNode.Items[I].HasChildren then
          GetChildrens(aNode.Items[I])
       else
          if Integer(aNode.Items[I].Data) = 1 then
             AddString(aNode.Items[I]);
   end;
begin
  aRepList := '';
  for J := 0 to ListaReports.Count-1 do
   if ListaReports.Items[J].HasChildren then
      GetChildrens(ListaReports.Items[J])
   else
      if Integer(ListaReports.Items[J].Data) = 1 then
         AddString(ListaReports.Items[J]);
   Result := aRepList;
end;

procedure TFrmChouseReport.ListaReportsGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TFrmChouseReport.ListaReportsKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
var aNode: TdxTreeListNode;
begin
  aNode := ListaReports.FocusedNode;
  if (Key = VK_SPACE) and (aNode <> nil) and (not aNode.HasChildren) then begin
     aNode.Data := Pointer(1- Integer(aNode.Data));
     ListaReports.Invalidate;
  end;
end;

procedure TFrmChouseReport.SetReportLists(const aRepList: String);
var I, Valoare, Err : Integer;
    aNode: TdxDBTreeListNode;
begin
  with TStringList.Create do
    try
       CommaText := aRepList;
       for I := 0 to Count - 1 do begin
         val(Strings[I], Valoare, Err);
         if Err > 0 then Continue;
         aNode := ListaReports.FindNodeByKeyValue(Valoare);
         if aNode <> nil then  begin
            SetSelectedNode(aNode, 1);
            ListaReportsChangeNode(ListaReports, nil, aNode);
         end;
       end;
    finally
       Free;
    end;
end;

procedure TFrmChouseReport.SetSelectedNode(const Node: TdxTreeListNode;
  const Valoare: Integer);
var ParentID: TdxTreeListNode;
  function GetCorectIndex(const aIndex: Integer): Integer;
   var I:Integer;
   begin
     Result := aIndex;
     for I := 0 to ParentID.Count - 1 do
       if Integer(ParentID.Items[I].Data) <> aIndex then begin
          Result := 2;
          Break;
       end;
   end;
begin
  InvalidateNodes;
  Node.Data := Pointer(Valoare);
  ParentID := Node.Parent;
  while ParentID <> nil do begin
    ParentID.Data := Pointer(GetCorectIndex(Valoare));
    ParentID := ParentID.Parent;
  end;
end;

procedure TFrmChouseReport.InvalidateNodes;
var
    //aNode : TdxTreeListNode;
    J : Integer;

  procedure InvalidateNode(lNode : TdxTreeListNode);
  var I : Integer;
  begin
   lNode.Data := Pointer(0);
    for I := 0 to lNode.Count -  1 do
      InvalidateNode(lNode.Items[I]);
  end;

begin
  if MultiSelect then Exit;
  for J := 0 to ListaReports.Count - 1 do
    InvalidateNode(ListaReports.Items[J]);
end;

end.
