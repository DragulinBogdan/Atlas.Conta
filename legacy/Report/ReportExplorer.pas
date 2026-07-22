unit ReportExplorer;

interface

uses
  ZDataSet, Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, ComCtrls, dxDBGrid, dxCntner, dxTL, dxDBCtrl, dxDBCtrl,
  JvXPCore, JvXPBar, frxClass, frxDock, frxPreview, dxDBTLCl, dxGrClms,
  CustomReport, Db, ImgList, dxmdaset, Menus;
  
type
  TfrmRepExplorer = class(TForm)
    pnTools: TPanel;
    pnLeft: TPanel;
    StatusBar: TStatusBar;
    Spliter: TSplitter;
    pnClient: TPanel;
    pnRapPreview: TPanel;
    SpliterH: TSplitter;
    pnRapList: TPanel;
    pnFunctii: TPanel;
    SpliterH2: TSplitter;
    pnRapTree: TPanel;
    TreeRap: TdxDBTreeList;
    ListaRap: TdxDBGrid;
    BarStandard: TJvXPBar;
    BarGenerale: TJvXPBar;
    PreviewBar: TJvXPBar;
    ToolBar: TfrxToolBar;
    Preview: TfrxPreview;
    TreeRapFolder: TdxDBTreeListColumn;
    ListaRapItemName: TdxDBGridColumn;
    ListaRapItemFolder: TdxDBGridColumn;
    ListaRapMarime: TdxDBGridColumn;
    ListaRapModificat: TdxDBGridDateColumn;
    ListaRapID: TdxDBGridColumn;
    DTFolders: TDataSource;
    DTReports: TDataSource;
    ADOQuery1: TZQuery;
    ADOQuery2: TZQuery;
    Imagini: TImageList;
    TblReports: TdxMemData;
    ListaRapType: TdxDBGridColumn;
    TblReportsITEM_TYPE: TIntegerField;
    TblReportsITEM_NAME: TStringField;
    TblReportsFOLDER_ID: TIntegerField;
    TblReportsITEM_ID: TIntegerField;
    TblReportsMODIFIED: TDateTimeField;
    TblReportsCREATED: TDateTimeField;
    TblReportsITEM_SIZE: TIntegerField;
    ppRepTree: TPopupMenu;
    ppNewFolder: TMenuItem;
    ppSubFolderNou: TMenuItem;
    ppListaRap: TPopupMenu;
    N1: TMenuItem;
    ppDeleteFolder: TMenuItem;
    ppRenameDir: TMenuItem;
    ppOpenReport: TMenuItem;
    N2: TMenuItem;
    ppDirectorNou: TMenuItem;
    ppRaportNou: TMenuItem;
    N3: TMenuItem;
    ppTipareste: TMenuItem;
    ppPreview: TMenuItem;
    N4: TMenuItem;
    ppStergeRaport: TMenuItem;
    ppRenameRaport: TMenuItem;
    procedure TreeRapGetImageIndex(Sender: TObject; Node: TdxTreeListNode;
      var Index: Integer);
    procedure ListaImageIndex(Sender: TObject; Node: TdxTreeListNode;
      var Index: Integer);
    procedure TreeRapGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure FormCreate(Sender: TObject);
    procedure BarGeneraleItems0Click(Sender: TObject);
    procedure TreeRapChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure ListaRapItemFolderGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure ListaRapDblClick(Sender: TObject);
    procedure ListaRapCompare(Sender: TObject; Node1,
      Node2: TdxTreeListNode; var Compare: Integer);
    procedure ppRaportNouClick(Sender: TObject);
  private
    { Private declarations }
    FFolderId: Integer;
    FFolderDataSet: TDataSet;
    FReportDataSet: TDataSet;
    FReports: TReports;
    procedure SetFolderDataSet(const Value: TDataSet);
    procedure SetReportDataSet(const Value: TDataSet);
  public
    procedure Notification(AComponent: TComponent; AOperation: TOperation); override;
    { Public declarations }
    procedure SetFolderId(const IdFolder: Integer);
    property  ReportDataSet: TDataSet read FReportDataSet write SetReportDataSet;
    property  FolderDataSet: TDataSet read FFolderDataSet write SetFolderDataSet;
    property  Reports: TReports read FReports write FReports;
  end;

implementation

{$R *.DFM}

procedure TfrmRepExplorer.Notification(AComponent: TComponent;
  AOperation: TOperation);
begin
  inherited Notification(AComponent, AOperation);
  
  if AOperation = opRemove then
     if AComponent = FReportDataSet then FReportDataSet := nil
     else if AComponent = FFolderDataSet then FFolderDataSet := nil;
end;

procedure TfrmRepExplorer.SetFolderId(const IdFolder: Integer);
var lNode: TdxDBTreeListNode;

  procedure GetReportsFromFolder(AFolderId: Integer);
   begin
     with FReportDataSet do begin
       First;
       while not Eof do begin
         if FieldByName('FOLDER_ID').AsInteger = AFolderId then TblReports.CopyRecords(FReportDataSet, False);
         Next;
       end;
     end;
   end;

  procedure GetSubFolders(ANode: TdxDBTreeListNode; AFolders: Boolean);
   var I : Integer;
   begin
     for I := 0 to ANode.Count-1 do begin
       if AFolders then begin
          TblReports.Append;
          TblReports.FieldByName('FOLDER_ID').AsInteger := TdxDBTreeListNode(ANode.Items[I]).Id;
          TblReports.FieldByName('ITEM_NAME').AsString := ANode.Items[I].Strings[TreeRapFolder.Index];
          TblReports.FieldByName('ITEM_TYPE').AsInteger := -1;
          TblReports.Post;
       end
       else GetReportsFromFolder(TdxDBTreeListNode(ANode.Items[I]).Id);
       GetSubFolders(TdxDBTreeListNode(ANode.Items[I]), AFolders);
     end;
   end;

begin

  TblReports.Active := False;
  FFolderId := IdFolder;
  { Daca nu avem dataset de rapoarte ... pa pa !!! }
  if not Assigned(FReportDataSet) then Exit;

  if TblReports.FieldCount < 2 then TblReports.CreateFieldsFromDataSet(FReportDataSet);

  if not TblReports.Active then TblReports.Active := True;

  GetReportsFromFolder(FFolderId);

  lNode := TreeRap.FindNodeByKeyValue(FFolderId);
  
  if lNode <> nil then begin
    if BarGenerale.Items[0].Tag > 0 then
       GetSubFolders(lNode, BarGenerale.Items[0].Tag = 1);

    if not lNode.Focused then begin
       lNode.MakeVisible;
       lNode.Focused := True;
    end;
  end;

end;

procedure TfrmRepExplorer.TreeRapGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.HasChildren then
     if Node.Expanded then Index := 2
     else Index := 1
  else Index := 0;

end;

procedure TfrmRepExplorer.TreeRapGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

type TCrackGrid = class(TdxDBGrid);

procedure TfrmRepExplorer.FormCreate(Sender: TObject);
begin
  FolderDataSet := ADOQuery1;
  ReportDataSet := ADOQuery2;
  with TCrackGrid(ListaRap) do begin
    Images := Imagini;
    OnGetImageIndex := ListaImageIndex;
    OnGetSelectedIndex := TreeRapGetSelectedIndex;
  end;
end;

procedure TfrmRepExplorer.SetFolderDataSet(const Value: TDataSet);
begin
  FFolderDataSet := Value;
  DTFolders.DataSet := FFolderDataSet;
end;

procedure TfrmRepExplorer.SetReportDataSet(const Value: TDataSet);
begin
  FReportDataSet := Value;
end;

procedure TfrmRepExplorer.BarGeneraleItems0Click(Sender: TObject);
begin
  with TJvXPBarItem(Sender) do
    case Tag of
      1 : begin
            Caption := 'Arata toate rapoartele';
            Tag := 2;
          end;
      2 : begin
            Caption := 'Doare rapoarte curente';
            Tag := 0;
          end;
      0 : begin
            Caption := 'Arata subdirectoare';
            Tag := 1;
          end;
    end;
  SetFolderId(FFolderId);
end;

procedure TfrmRepExplorer.TreeRapChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Node <> nil then SetFolderId(TdxDBTreeListNode(Node).Id);
end;

procedure TfrmRepExplorer.ListaImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.Values[ListaRapType.Index] = -1 then Index := 0
  else Index := 4;
end;

procedure TfrmRepExplorer.ListaRapItemFolderGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var
  lRepId: Integer;
  Error: Integer;
  lNode: TdxDBTreeListNode;
begin
  Val(AText, lRepId, Error);
  if Error = 0 then begin
     lNode := TreeRap.FindNodeByKeyValue(lRepId);
     if lNode <> nil then AText := lNode.Values[TreeRapFolder.Index];
  end;
end;

procedure TfrmRepExplorer.ListaRapDblClick(Sender: TObject);
var lNode: TdxDBGridNode;
begin
  lNode := TdxDBGridNode(ListaRap.FocusedNode);
  if (lNode <> nil) and (lNode.Values[ListaRapType.Index] = -1) then
     SetFolderId(lNode.Values[ListaRapItemFolder.Index]);
end;

procedure TfrmRepExplorer.ListaRapCompare(Sender: TObject; Node1,
  Node2: TdxTreeListNode; var Compare: Integer);
var lDir1,
    lDir2: Boolean;
    lVal1,
    lVal2 : String;
begin
  lDir1 := Node1.Values[ListaRapType.Index] = -1;
  lDir2 := Node2.Values[ListaRapType.Index] = -1;
  lVal1 := Node1.Strings[ListaRapItemName.Index];
  lVal2 := Node2.Strings[ListaRapItemName.Index];
  if lDir1 then
     if lDir2 then
        Compare := AnsiCompareText(lVal1, lVal2)
     else Compare := -1
  else if lDir2 then Compare := 1
       else Compare := AnsiCompareText(lVal1, lVal2);
end;

procedure TfrmRepExplorer.ppRaportNouClick(Sender: TObject);
begin
  FReports.NewReport;
end;

end.
